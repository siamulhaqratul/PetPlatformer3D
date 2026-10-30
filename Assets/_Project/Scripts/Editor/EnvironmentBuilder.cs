using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class EnvironmentBuilder
{
    // =========================== CONSTANTS ===========================

    private const string UNDO_NAME = "Build Environment";
    private const string MATERIALS_FOLDER = "Assets/_Project/Materials";
    private const string SKY_MATERIAL_PATH = MATERIALS_FOLDER + "/Sky.mat";
    private const string CLOUD_MATERIAL_PATH = MATERIALS_FOLDER + "/Cloud.mat";
    private const string SKY_SHADER = "Skybox/Procedural";
    private const string CLOUD_SHADER = "Universal Render Pipeline/Simple Lit";
    private const string SUN_NAME = "Directional Light";
    private const string CLOUDS_NAME = "_Clouds";
    private const string CUBE_MESH = "Cube.fbx";
    private const int CLOUD_COUNT = 34;
    private const float CLOUD_FIELD_LENGTH = 460f;
    private const int RANDOM_SEED = 11;
    private const float CAMERA_FAR_CLIP = 5000f;
    private const string POST_PROCESSING_NAME = "_PostProcessing";
    private const string POST_PROCESSING_PROFILE_PATH = "Assets/_Project/Settings/PostProcessingProfile.asset";

    private static readonly Color _skyTint = new Color(0.42f, 0.62f, 0.95f);
    private static readonly Color _skyGround = new Color(0.66f, 0.8f, 0.92f);
    private static readonly Color _fogColor = new Color(0.74f, 0.87f, 0.97f);
    private static readonly Color _ambientSky = new Color(0.78f, 0.86f, 1f);
    private static readonly Color _ambientEquator = new Color(0.72f, 0.78f, 0.84f);
    private static readonly Color _ambientGround = new Color(0.5f, 0.48f, 0.5f);
    private static readonly Color _sunColor = new Color(1f, 0.95f, 0.85f);
    private static readonly Vector3 _sunRotation = new Vector3(48f, -35f, 0f);
    private static readonly Color _vignetteColor = new Color(0.05f, 0.08f, 0.18f);

    // ========================= STATIC METHODS ========================

    public static void BuildEnvironment()
    {
        SetupSky();
        SetupFog();
        SetupAmbient();
        SetupSun();
        SetupCamera();
        SetupPostProcessing();
        BuildClouds();
    }

    private static void SetupSky()
    {
        Material sky = LoadOrCreateMaterial(SKY_MATERIAL_PATH, SKY_SHADER);
        sky.SetColor("_SkyTint", _skyTint);
        sky.SetColor("_GroundColor", _skyGround);
        sky.SetFloat("_AtmosphereThickness", 0.65f);
        sky.SetFloat("_Exposure", 1.25f);
        sky.SetFloat("_SunSize", 0.03f);
        EditorUtility.SetDirty(sky);
        RenderSettings.skybox = sky;
    }

    private static void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = _fogColor;
        RenderSettings.fogStartDistance = 45f;
        RenderSettings.fogEndDistance = 170f;
    }

    private static void SetupAmbient()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = _ambientSky;
        RenderSettings.ambientEquatorColor = _ambientEquator;
        RenderSettings.ambientGroundColor = _ambientGround;
    }

    private static void SetupSun()
    {
        GameObject sunObject = GameObject.Find(SUN_NAME);
        if (sunObject == null) return;

        var sun = sunObject.GetComponent<Light>();
        Undo.RecordObject(sun, UNDO_NAME);
        Undo.RecordObject(sunObject.transform, UNDO_NAME);
        sunObject.transform.rotation = Quaternion.Euler(_sunRotation);
        sun.color = _sunColor;
        sun.intensity = 1.25f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.75f;
        RenderSettings.sun = sun;
    }

    private static void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Undo.RecordObject(mainCamera, UNDO_NAME);
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCamera.farClipPlane = CAMERA_FAR_CLIP;

        UniversalAdditionalCameraData cameraData = mainCamera.GetUniversalAdditionalCameraData();
        Undo.RecordObject(cameraData, UNDO_NAME);
        cameraData.renderPostProcessing = true;
    }

    // One global volume for the whole scene. The look stays cheap enough for phones and WebGL: a little more
    // contrast and colour, a warm white balance, gentle bloom on the brightest whites and a soft vignette,
    // all drawn in URP's single post-processing pass.
    private static void SetupPostProcessing()
    {
        VolumeProfile profile = LoadOrCreateProfile();

        ColorAdjustments colorAdjustments = GetOrAddEffect<ColorAdjustments>(profile);
        colorAdjustments.postExposure.Override(0f);
        colorAdjustments.contrast.Override(15f);
        colorAdjustments.saturation.Override(10f);

        WhiteBalance whiteBalance = GetOrAddEffect<WhiteBalance>(profile);
        whiteBalance.temperature.Override(6f);

        Bloom bloom = GetOrAddEffect<Bloom>(profile);
        bloom.threshold.Override(0.95f);
        bloom.intensity.Override(0.3f);
        bloom.scatter.Override(0.6f);

        Vignette vignette = GetOrAddEffect<Vignette>(profile);
        vignette.intensity.Override(0.22f);
        vignette.smoothness.Override(0.45f);
        vignette.color.Override(_vignetteColor);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        PetTestSceneBuilder.DestroyExisting(POST_PROCESSING_NAME);
        var volumeObject = new GameObject(POST_PROCESSING_NAME);
        Undo.RegisterCreatedObjectUndo(volumeObject, UNDO_NAME);
        var volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;
    }

    private static VolumeProfile LoadOrCreateProfile()
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(POST_PROCESSING_PROFILE_PATH);
        if (profile != null) return profile;

        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, POST_PROCESSING_PROFILE_PATH);
        return profile;
    }

    // Effects live as hidden sub-assets of the profile, the same way the Volume inspector stores them.
    private static T GetOrAddEffect<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (profile.TryGet(out T effect)) return effect;

        effect = profile.Add<T>();
        effect.name = typeof(T).Name;
        effect.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(effect, profile);
        return effect;
    }

    private static void BuildClouds()
    {
        PetTestSceneBuilder.DestroyExisting(CLOUDS_NAME);

        var root = new GameObject(CLOUDS_NAME).transform;
        Undo.RegisterCreatedObjectUndo(root.gameObject, UNDO_NAME);

        Material cloudMaterial = LoadOrCreateMaterial(CLOUD_MATERIAL_PATH, CLOUD_SHADER);
        cloudMaterial.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(cloudMaterial);

        var random = new System.Random(RANDOM_SEED);
        for (int index = 0; index < CLOUD_COUNT; index++)
        {
            float side = index % 2 == 0 ? -1f : 1f;
            var center = new Vector3(
                side * Range(random, 4f, 55f),
                Range(random, 14f, 34f),
                Range(random, -10f, CLOUD_FIELD_LENGTH));
            BuildCloud(root, center, cloudMaterial, random);
        }
    }

    private static void BuildCloud(Transform parent, Vector3 center, Material material, System.Random random)
    {
        var cloud = new GameObject("Cloud").transform;
        cloud.SetParent(parent);
        cloud.position = center;

        int puffCount = random.Next(3, 6);
        for (int puff = 0; puff < puffCount; puff++)
        {
            var puffObject = new GameObject("Puff");
            puffObject.transform.SetParent(cloud, false);
            puffObject.transform.localPosition = new Vector3(Range(random, -3f, 3f), Range(random, -0.5f, 0.8f), Range(random, -1.5f, 1.5f));
            float size = Range(random, 1.8f, 3.6f);
            puffObject.transform.localScale = new Vector3(size * 1.4f, size * 0.7f, size);

            puffObject.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>(CUBE_MESH);
            var meshRenderer = puffObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            puffObject.isStatic = true;
        }
    }

    private static Material LoadOrCreateMaterial(string path, string shaderName)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;

        material = new Material(Shader.Find(shaderName));
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static float Range(System.Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}
