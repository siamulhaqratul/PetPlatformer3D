using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

// Builds the Android app icon from the game's own look: the Dog standing on a grass-topped course block under a
// blue sky with blocky clouds. The adaptive icon gets the scene as its background layer and the Dog as its
// foreground, so launchers can mask and animate them apart; the round and legacy icons get both flattened.
public static class AppIconBuilder
{
    // =========================== CONSTANTS ===========================

    private const string MENU_PATH = "Tools/Pet Shift Run/Build App Icon";
    private const string OUTPUT_FOLDER = "Assets/_Project/Art/AppIcon";
    private const string BACKGROUND_PATH = OUTPUT_FOLDER + "/icon_background.png";
    private const string FOREGROUND_PATH = OUTPUT_FOLDER + "/icon_foreground.png";
    private const string LEGACY_PATH = OUTPUT_FOLDER + "/icon_legacy.png";
    private const string ROUND_PATH = OUTPUT_FOLDER + "/icon_round.png";
    private const string DOG_MODEL = "animal-dog";

    // An adaptive layer is 108 dp square (432 px at xxxhdpi); launchers show the middle 72 dp and keep a 66 dp
    // circle clear of any mask, so everything that matters sits inside that circle.
    private const int LAYER_SIZE = 432;
    private const int VISIBLE_SIZE = 288;
    // The Dog's render square; the model fills about 85% of it, which keeps the Dog's corners inside the safe circle.
    private const int DOG_SIZE = 224;
    // Puts the Dog's feet just into the grass top.
    private const int DOG_BOTTOM = 108;
    private const int GRASS_TOP = 130;
    private const int GRASS_DEPTH = 26;
    private const int GRASS_HIGHLIGHT = 7;

    private static readonly Color _skyTop = new Color(0.27f, 0.53f, 0.9f);
    private static readonly Color _skyHorizon = new Color(0.72f, 0.88f, 0.99f);
    private static readonly Color _grass = new Color(0.3f, 0.85f, 0.52f);
    private static readonly Color _grassHighlight = new Color(0.47f, 0.93f, 0.63f);
    private static readonly Color _dirt = new Color(0.91f, 0.48f, 0.3f);
    private static readonly Color _dirtShadow = new Color(0.72f, 0.33f, 0.22f);
    private static readonly Color _cloud = new Color(1f, 1f, 1f, 0.92f);

    // Blocky clouds like the course's, as rectangles (x, y, width, height) in layer pixels, drawn in the top corners.
    private static readonly RectInt[] _clouds =
    {
        new RectInt(76, 300, 64, 18), new RectInt(90, 318, 34, 14),
        new RectInt(298, 262, 60, 16), new RectInt(312, 278, 30, 12),
    };

    // ========================= STATIC METHODS ========================

    [MenuItem(MENU_PATH)]
    public static void BuildAppIcon()
    {
        Directory.CreateDirectory(OUTPUT_FOLDER);

        Texture2D background = DrawBackground();
        Texture2D foreground = DrawForeground();
        Texture2D flattened = Flatten(background, foreground);
        Texture2D legacy = CropVisible(flattened);
        Texture2D round = MaskCircle(legacy);

        Texture2D backgroundAsset = WriteTexture(background, BACKGROUND_PATH);
        Texture2D foregroundAsset = WriteTexture(foreground, FOREGROUND_PATH);
        Texture2D legacyAsset = WriteTexture(legacy, LEGACY_PATH);
        Texture2D roundAsset = WriteTexture(round, ROUND_PATH);
        Object.DestroyImmediate(flattened);

        SetIcons(AndroidPlatformIconKind.Adaptive, backgroundAsset, foregroundAsset);
        SetIcons(AndroidPlatformIconKind.Round, roundAsset);
        SetIcons(AndroidPlatformIconKind.Legacy, legacyAsset);
        AssetDatabase.SaveAssets();
        Debug.Log("App icon built.");
    }

    // ----- Layers -----

    // Sky fading to a pale horizon, the clouds, then the course block: a grass top over orange dirt.
    private static Texture2D DrawBackground()
    {
        var pixels = new Color[LAYER_SIZE * LAYER_SIZE];
        int grassBottom = GRASS_TOP - GRASS_DEPTH;

        for (int y = 0; y < LAYER_SIZE; y++)
        {
            Color rowColor;
            if (y >= GRASS_TOP)
            {
                float skyHeight = Mathf.InverseLerp(GRASS_TOP, LAYER_SIZE - 1, y);
                rowColor = Color.Lerp(_skyHorizon, _skyTop, skyHeight);
            }
            else if (y >= grassBottom)
            {
                rowColor = y >= GRASS_TOP - GRASS_HIGHLIGHT ? _grassHighlight : _grass;
            }
            else
            {
                rowColor = Color.Lerp(_dirtShadow, _dirt, Mathf.InverseLerp(0, grassBottom, y));
            }

            for (int x = 0; x < LAYER_SIZE; x++)
            {
                pixels[y * LAYER_SIZE + x] = rowColor;
            }
        }

        foreach (RectInt cloud in _clouds)
        {
            for (int y = cloud.yMin; y < cloud.yMax; y++)
            {
                for (int x = cloud.xMin; x < cloud.xMax; x++)
                {
                    int index = y * LAYER_SIZE + x;
                    pixels[index] = Color.Lerp(pixels[index], _cloud, _cloud.a);
                }
            }
        }

        return CreateTexture(pixels, LAYER_SIZE);
    }

    // The Dog's outlined portrait, centred and standing on the grass line, on a transparent layer.
    private static Texture2D DrawForeground()
    {
        Texture2D dog = IconRenderer.RenderPortraitTexture(PetFormBuilder.ModelPath(DOG_MODEL), DOG_SIZE);
        var pixels = new Color[LAYER_SIZE * LAYER_SIZE];
        int left = (LAYER_SIZE - DOG_SIZE) / 2;
        Color[] dogPixels = dog.GetPixels();

        for (int y = 0; y < DOG_SIZE; y++)
        {
            for (int x = 0; x < DOG_SIZE; x++)
            {
                pixels[(DOG_BOTTOM + y) * LAYER_SIZE + left + x] = dogPixels[y * DOG_SIZE + x];
            }
        }

        Object.DestroyImmediate(dog);
        return CreateTexture(pixels, LAYER_SIZE);
    }

    // ----- Flattened icons -----

    private static Texture2D Flatten(Texture2D background, Texture2D foreground)
    {
        Color[] below = background.GetPixels();
        Color[] above = foreground.GetPixels();
        var pixels = new Color[below.Length];
        for (int index = 0; index < pixels.Length; index++)
        {
            Color color = Color.Lerp(below[index], above[index], above[index].a);
            color.a = 1f;
            pixels[index] = color;
        }

        return CreateTexture(pixels, LAYER_SIZE);
    }

    // The part of the layers a launcher shows: the middle two thirds.
    private static Texture2D CropVisible(Texture2D source)
    {
        int offset = (LAYER_SIZE - VISIBLE_SIZE) / 2;
        Color[] pixels = source.GetPixels(offset, offset, VISIBLE_SIZE, VISIBLE_SIZE);
        return CreateTexture(pixels, VISIBLE_SIZE);
    }

    // A circle with a one-pixel soft edge, transparent outside.
    private static Texture2D MaskCircle(Texture2D source)
    {
        Color[] pixels = source.GetPixels();
        float center = (VISIBLE_SIZE - 1) * 0.5f;
        float radius = VISIBLE_SIZE * 0.5f;
        for (int y = 0; y < VISIBLE_SIZE; y++)
        {
            for (int x = 0; x < VISIBLE_SIZE; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                pixels[y * VISIBLE_SIZE + x].a = Mathf.Clamp01(radius - distance);
            }
        }

        return CreateTexture(pixels, VISIBLE_SIZE);
    }

    // ----- Assets and Player Settings -----

    // Every density gets the same full-size texture; Unity scales it down for each one.
    private static void SetIcons(PlatformIconKind kind, params Texture2D[] layers)
    {
        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
        foreach (PlatformIcon icon in icons)
        {
            icon.SetTextures(layers);
        }

        PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
    }

    private static Texture2D CreateTexture(Color[] pixels, int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    // Saved uncompressed and without mipmaps, so the icon keeps its exact colours and edges.
    private static Texture2D WriteTexture(Texture2D texture, string path)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
