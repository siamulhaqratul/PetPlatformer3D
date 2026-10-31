using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PetTestSceneBuilder
{
    // =========================== CONSTANTS ===========================

    private const string MENU_PATH = "Tools/Pet Shift Run/Build Pet Test Setup";
    private const string STATS_FOLDER = "Assets/_Project/Settings/Pets";
    private const string DOG_STATS_PATH = STATS_FOLDER + "/DogMovementStats.asset";
    private const string DOG_MODEL_PATH = "Assets/_Project/Art/Kenney/CubePets/Models/animal-dog.fbx";
    private const string PET_NAME = "Pet";
    private const string VISUAL_NAME = "Visual";
    private const string TILT_ROOT_NAME = "TiltRoot";
    private const string SQUASH_PIVOT_NAME = "SquashPivot";
    private const string ROLL_PIVOT_NAME = "RollPivot";
    private const string DUST_MATERIAL_PATH = "Assets/_Project/Materials/DustPuff.mat";
    private const string DUST_SHADER_NAME = "Universal Render Pipeline/Particles/Simple Lit";
    private const string GREYBOX_NAME = "_TestTrack";
    private const string VCAM_NAME = "PetFollowCam";
    private const float CONTROLLER_RADIUS_SCALE = 0.45f;
    private const float CONTROLLER_STEP_OFFSET = 0.3f;
    private const float CONTROLLER_SKIN_WIDTH = 0.05f;
    private const float CONTROLLER_SLOPE_LIMIT = 50f;

    private static readonly Vector3 _cameraFollowOffset = new Vector3(0f, 5.5f, -9f);
    private static readonly Vector3 _cameraAimOffset = new Vector3(0f, 0.5f, 5f);

    // ========================= STATIC METHODS ========================

    [MenuItem(MENU_PATH)]
    private static void BuildPetTestSetup()
    {
        PetMovementStats stats = LoadOrCreateDogStats();
        PetFormBuilder.EnsureForms(out PetForm dog, out PetForm bunny, out PetForm cat, out PetForm monkey, out PetForm penguin);
        GameObject pet = BuildPet(stats, dog);
        DestroyExisting(GREYBOX_NAME);
        CinemachineVirtualCamera followCamera = BuildCamera(pet.transform);
        LevelBuilder.ConfigureForms(pet.GetComponent<PetFormController>(), bunny, cat, monkey, penguin);
        CourseTestBuilder.BuildGameSystems(pet, followCamera);
        UIBuilder.BuildUI();

        EditorSceneManager.MarkSceneDirty(pet.scene);
        Selection.activeGameObject = pet;
        Debug.Log("Pet test setup built. Press Play: A/D or arrows steer, Space jumps.");
    }

    private static PetMovementStats LoadOrCreateDogStats()
    {
        var stats = AssetDatabase.LoadAssetAtPath<PetMovementStats>(DOG_STATS_PATH);
        if (stats != null) return stats;

        if (!AssetDatabase.IsValidFolder(STATS_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Settings", "Pets");
        }

        stats = ScriptableObject.CreateInstance<PetMovementStats>();
        AssetDatabase.CreateAsset(stats, DOG_STATS_PATH);
        AssetDatabase.SaveAssets();
        return stats;
    }

    private static GameObject BuildPet(PetMovementStats stats, PetForm startForm)
    {
        DestroyExisting(PET_NAME);

        var pet = new GameObject(PET_NAME);
        Undo.RegisterCreatedObjectUndo(pet, MENU_PATH);

        Transform tiltRoot = CreateChild(TILT_ROOT_NAME, pet.transform);
        Transform squashPivot = CreateChild(SQUASH_PIVOT_NAME, tiltRoot);

        var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(DOG_MODEL_PATH);
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, squashPivot);
        visual.name = VISUAL_NAME;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        Animator animator = visual.GetComponent<Animator>();
        if (animator == null)
        {
            animator = visual.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = PetAnimatorBuilder.GetOrCreateController(DOG_MODEL_PATH);
        animator.applyRootMotion = false;

        var controller = pet.AddComponent<CharacterController>();
        FitControllerToModel(controller, visual);

        // The roll spins around the pet's middle, so the model hangs below a pivot at that height.
        Transform rollPivot = CreateChild(ROLL_PIVOT_NAME, squashPivot);
        rollPivot.localPosition = Vector3.up * controller.center.y;
        visual.transform.SetParent(rollPivot, true);

        var input = pet.AddComponent<PetInput>();
        var motor = pet.AddComponent<PetMotor>();
        var motorObject = new SerializedObject(motor);
        motorObject.FindProperty("_stats").objectReferenceValue = stats;
        motorObject.FindProperty("_input").objectReferenceValue = input;
        motorObject.FindProperty("_runOnStart").boolValue = false;
        motorObject.ApplyModifiedPropertiesWithoutUndo();

        Material dustMaterial = LoadOrCreateDustMaterial();
        ParticleSystem burstDust = PetDustFactory.CreateBurstDust(pet.transform, dustMaterial);
        ParticleSystem runDust = PetDustFactory.CreateRunDust(pet.transform, dustMaterial);

        var petVisual = pet.AddComponent<PetVisual>();
        var visualObject = new SerializedObject(petVisual);
        visualObject.FindProperty("_motor").objectReferenceValue = motor;
        visualObject.FindProperty("_animator").objectReferenceValue = animator;
        visualObject.FindProperty("_tiltRoot").objectReferenceValue = tiltRoot;
        visualObject.FindProperty("_squashPivot").objectReferenceValue = squashPivot;
        visualObject.FindProperty("_rollPivot").objectReferenceValue = rollPivot;
        visualObject.FindProperty("_model").objectReferenceValue = visual.transform;
        visualObject.FindProperty("_burstDust").objectReferenceValue = burstDust;
        visualObject.FindProperty("_runDust").objectReferenceValue = runDust;
        visualObject.ApplyModifiedPropertiesWithoutUndo();

        var formController = pet.AddComponent<PetFormController>();
        var formObject = new SerializedObject(formController);
        formObject.FindProperty("_motor").objectReferenceValue = motor;
        formObject.FindProperty("_visual").objectReferenceValue = petVisual;
        formObject.FindProperty("_startForm").objectReferenceValue = startForm;
        formObject.ApplyModifiedPropertiesWithoutUndo();

        pet.transform.position = Vector3.up * 0.1f;
        return pet;
    }

    private static Transform CreateChild(string childName, Transform parent)
    {
        var child = new GameObject(childName).transform;
        child.SetParent(parent, false);
        return child;
    }

    public static Material LoadOrCreateDustMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(DUST_MATERIAL_PATH);
        if (material != null) return material;

        material = new Material(Shader.Find(DUST_SHADER_NAME));
        AssetDatabase.CreateAsset(material, DUST_MATERIAL_PATH);
        return material;
    }

    private static void FitControllerToModel(CharacterController controller, GameObject visual)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer meshRenderer in renderers)
        {
            bounds.Encapsulate(meshRenderer.bounds);
        }

        float radius = Mathf.Min(bounds.size.x, bounds.size.z) * CONTROLLER_RADIUS_SCALE;
        controller.height = Mathf.Max(bounds.size.y, radius * 2f);
        controller.radius = radius;
        controller.center = bounds.center - controller.transform.position;
        controller.stepOffset = CONTROLLER_STEP_OFFSET;
        controller.skinWidth = CONTROLLER_SKIN_WIDTH;
        controller.slopeLimit = CONTROLLER_SLOPE_LIMIT;
        Debug.Log($"Pet bounds {bounds.size}, controller height {controller.height:F2}, radius {radius:F2}");
    }

    private static CinemachineVirtualCamera BuildCamera(Transform target)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<CinemachineBrain>() == null)
        {
            Undo.AddComponent<CinemachineBrain>(mainCamera.gameObject);
        }

        DestroyExisting(VCAM_NAME);

        var cameraObject = new GameObject(VCAM_NAME);
        Undo.RegisterCreatedObjectUndo(cameraObject, MENU_PATH);

        var virtualCamera = cameraObject.AddComponent<CinemachineVirtualCamera>();
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
        virtualCamera.m_Lens.FieldOfView = 55f;

        var transposer = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        transposer.m_FollowOffset = _cameraFollowOffset;
        transposer.m_XDamping = 0.6f;
        transposer.m_YDamping = 1.2f;
        transposer.m_ZDamping = 0.2f;

        var composer = virtualCamera.AddCinemachineComponent<CinemachineComposer>();
        composer.m_TrackedObjectOffset = _cameraAimOffset;
        composer.m_HorizontalDamping = 1f;
        composer.m_VerticalDamping = 1.5f;

        var fallHold = cameraObject.AddComponent<CameraFallHold>();
        var serializedHold = new SerializedObject(fallHold);
        serializedHold.FindProperty("_virtualCamera").objectReferenceValue = virtualCamera;
        serializedHold.FindProperty("_motor").objectReferenceValue = target.GetComponent<PetMotor>();
        serializedHold.ApplyModifiedPropertiesWithoutUndo();

        cameraObject.transform.position = target.position + _cameraFollowOffset;
        return virtualCamera;
    }

    public static void DestroyExisting(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing == null) return;

        Undo.DestroyObjectImmediate(existing);
    }
}
