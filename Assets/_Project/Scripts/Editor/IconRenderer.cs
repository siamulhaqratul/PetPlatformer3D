using System.IO;
using UnityEditor;
using UnityEngine;

// Renders Kenney 3D models into outlined UI sprites: hearts for lives (red, or ash grey when lost)
// and pet portraits for the form badge.
public static class IconRenderer
{
    // =========================== CONSTANTS ===========================

    private const string MENU_PATH = "Tools/Pet Shift Run/Render UI Icons";
    private const string HEART_MODEL_PATH = "Assets/_Project/Art/Kenney/PlatformerKit/Models/heart.fbx";
    private const string OUTPUT_FOLDER = "Assets/_Project/UI/Generated";
    private const string HEART_FULL_PATH = OUTPUT_FOLDER + "/heart_full.png";
    private const string HEART_EMPTY_PATH = OUTPUT_FOLDER + "/heart_empty.png";
    private const int TEXTURE_SIZE = 256;
    private const int DEPTH_BITS = 24;
    private const int ANTI_ALIASING = 8;
    private const int RENDER_LAYER = 31;
    private const float FRAME_PADDING = 1.15f;
    private const float CAMERA_DISTANCE = 4f;
    private const float SHADE_MIN = 0.6f;
    private const float ALPHA_SOLID = 0.5f;
    private const float ALPHA_OPAQUE = 0.98f;
    private const int OUTLINE_RADIUS = 9;
    private const string GEAR_PATH = OUTPUT_FOLDER + "/gear.png";
    private const int GEAR_SIZE = 128;
    private const int GEAR_TEETH = 8;
    private const float GEAR_OUTER_RADIUS = 60f;
    private const float GEAR_ROOT_RADIUS = 45f;
    private const float GEAR_HOLE_RADIUS = 18f;
    // Share of each tooth's slice of the circle that the tooth fills; the rest is the gap to the next tooth.
    private const float GEAR_TOOTH_SHARE = 0.5f;

    private static readonly Vector3 _stagePosition = new Vector3(0f, -500f, 0f);
    private static readonly Quaternion _portraitRotation = Quaternion.Euler(0f, 155f, 0f);
    private static readonly Color _heartRed = new Color(0.98f, 0.24f, 0.3f);
    private static readonly Color _heartRedOutline = new Color(0.45f, 0.06f, 0.12f);
    private static readonly Color _heartAsh = new Color(0.72f, 0.74f, 0.8f);
    private static readonly Color _heartAshOutline = new Color(0.36f, 0.38f, 0.45f);
    private static readonly Color _portraitOutline = new Color(0.12f, 0.13f, 0.2f);

    // ========================= STATIC METHODS ========================

    [MenuItem(MENU_PATH)]
    private static void RenderFromMenu()
    {
        RenderHearts();
    }

    public static void EnsureHeartSprites(out Sprite full, out Sprite empty)
    {
        if (!File.Exists(HEART_FULL_PATH) || !File.Exists(HEART_EMPTY_PATH))
        {
            RenderHearts();
        }

        full = AssetDatabase.LoadAssetAtPath<Sprite>(HEART_FULL_PATH);
        empty = AssetDatabase.LoadAssetAtPath<Sprite>(HEART_EMPTY_PATH);
    }

    // Front three-quarter portrait of a model in its own colours, with a dark outline.
    public static Sprite EnsurePortrait(string modelPath)
    {
        string outputPath = $"{OUTPUT_FOLDER}/{Path.GetFileNameWithoutExtension(modelPath)}_icon.png";
        if (!File.Exists(outputPath))
        {
            Directory.CreateDirectory(OUTPUT_FOLDER);
            Texture2D render = RenderModel(modelPath, _portraitRotation);
            Texture2D outlined = Recolor(render, null, _portraitOutline);
            WriteSprite(outlined, outputPath);
            Object.DestroyImmediate(render);
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
    }

    // White gear with a hole for the settings buttons, drawn in code because the UI Pack has no settings icon.
    public static Sprite EnsureGearSprite()
    {
        if (!File.Exists(GEAR_PATH))
        {
            Directory.CreateDirectory(OUTPUT_FOLDER);
            WriteSprite(DrawGear(), GEAR_PATH);
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(GEAR_PATH);
    }

    private static Texture2D DrawGear()
    {
        var texture = new Texture2D(GEAR_SIZE, GEAR_SIZE, TextureFormat.RGBA32, false);
        var pixels = new Color[GEAR_SIZE * GEAR_SIZE];
        float center = (GEAR_SIZE - 1) * 0.5f;
        float toothPeriod = 2f * Mathf.PI / GEAR_TEETH;

        for (int y = 0; y < GEAR_SIZE; y++)
        {
            for (int x = 0; x < GEAR_SIZE; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                // Angle from the nearest tooth's centre line, then the distance in pixels to that tooth's side.
                float toothAngle = Mathf.Repeat(Mathf.Atan2(dy, dx), toothPeriod) - toothPeriod * 0.5f;
                float sideDistance = (toothPeriod * GEAR_TOOTH_SHARE * 0.5f - Mathf.Abs(toothAngle)) * radius;
                float edgeRadius = Mathf.Lerp(GEAR_ROOT_RADIUS, GEAR_OUTER_RADIUS, Mathf.Clamp01(sideDistance + 0.5f));
                float coverage = Mathf.Clamp01(edgeRadius - radius + 0.5f) * Mathf.Clamp01(radius - GEAR_HOLE_RADIUS + 0.5f);
                pixels[y * GEAR_SIZE + x] = new Color(1f, 1f, 1f, coverage);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static void RenderHearts()
    {
        Directory.CreateDirectory(OUTPUT_FOLDER);

        Texture2D render = RenderModel(HEART_MODEL_PATH, Quaternion.identity);
        WriteSprite(Recolor(render, _heartRed, _heartRedOutline), HEART_FULL_PATH);
        WriteSprite(Recolor(render, _heartAsh, _heartAshOutline), HEART_EMPTY_PATH);
        Object.DestroyImmediate(render);
        Debug.Log("Heart icons rendered.");
    }

    // Renders the model head-on with an orthographic camera framed to its bounds, on a transparent background.
    private static Texture2D RenderModel(string modelPath, Quaternion rotation)
    {
        var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        GameObject model = Object.Instantiate(modelAsset, _stagePosition, rotation);
        SetLayerRecursively(model.transform, RENDER_LAYER);
        Bounds bounds = GetBounds(model);

        var cameraObject = new GameObject("IconCamera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) * FRAME_PADDING;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.cullingMask = 1 << RENDER_LAYER;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = CAMERA_DISTANCE * 2f;
        cameraObject.transform.position = bounds.center + Vector3.back * CAMERA_DISTANCE;
        cameraObject.transform.rotation = Quaternion.identity;

        var renderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, DEPTH_BITS, RenderTextureFormat.ARGB32)
        {
            antiAliasing = ANTI_ALIASING,
        };
        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        var texture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), 0, 0);
        texture.Apply();
        RenderTexture.active = previous;

        camera.targetTexture = null;
        Object.DestroyImmediate(renderTexture);
        Object.DestroyImmediate(cameraObject);
        Object.DestroyImmediate(model);
        return texture;
    }

    // Optionally swaps the colour (keeping the model's shading), then adds a solid outline around the silhouette.
    private static Texture2D Recolor(Texture2D source, Color? baseColor, Color outlineColor)
    {
        int size = source.width;
        Color[] pixels = source.GetPixels();
        var result = new Color[pixels.Length];
        float brightest = 0f;
        foreach (Color pixel in pixels)
        {
            brightest = Mathf.Max(brightest, pixel.grayscale);
        }

        for (int index = 0; index < pixels.Length; index++)
        {
            Color pixel = pixels[index];
            if (pixel.a <= 0f)
            {
                result[index] = Color.clear;
                continue;
            }

            if (baseColor.HasValue)
            {
                float shade = SHADE_MIN + (1f - SHADE_MIN) * pixel.grayscale / brightest;
                pixel = baseColor.Value * shade;
                pixel.a = pixels[index].a;
            }

            result[index] = pixel;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                if (pixels[index].a >= ALPHA_OPAQUE) continue;

                float coverage = OutlineCoverage(pixels, size, x, y);
                if (coverage <= 0f) continue;

                result[index] = outlineColor;
                result[index].a = Mathf.Max(pixels[index].a, coverage);
            }
        }

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.SetPixels(result);
        texture.Apply();
        return texture;
    }

    private static float OutlineCoverage(Color[] pixels, int size, int x, int y)
    {
        float nearest = float.MaxValue;
        for (int offsetY = -OUTLINE_RADIUS; offsetY <= OUTLINE_RADIUS; offsetY++)
        {
            for (int offsetX = -OUTLINE_RADIUS; offsetX <= OUTLINE_RADIUS; offsetX++)
            {
                int sampleX = x + offsetX;
                int sampleY = y + offsetY;
                bool isInside = sampleX >= 0 && sampleY >= 0 && sampleX < size && sampleY < size;
                if (!isInside || pixels[sampleY * size + sampleX].a < ALPHA_SOLID) continue;

                nearest = Mathf.Min(nearest, Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY));
            }
        }

        return Mathf.Clamp01(OUTLINE_RADIUS + 0.5f - nearest);
    }

    private static void WriteSprite(Texture2D texture, string path)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static Bounds GetBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer modelRenderer in renderers)
        {
            bounds.Encapsulate(modelRenderer.bounds);
        }

        return bounds;
    }

    private static void SetLayerRecursively(Transform target, int layer)
    {
        target.gameObject.layer = layer;
        foreach (Transform child in target)
        {
            SetLayerRecursively(child, layer);
        }
    }
}
