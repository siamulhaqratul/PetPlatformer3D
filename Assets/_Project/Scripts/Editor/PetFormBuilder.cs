using UnityEditor;
using UnityEngine;

// Creates the Dog, Bunny and Bee forms: each with its Cube Pets model, animator, icon and movement stats.
public static class PetFormBuilder
{
    // =========================== CONSTANTS ===========================

    private const string PETS_FOLDER = "Assets/_Project/Settings/Pets";
    private const string MODELS_FOLDER = "Assets/_Project/Art/Kenney/CubePets/Models";
    private const string DOG_STATS_PATH = PETS_FOLDER + "/DogMovementStats.asset";

    private const float BUNNY_JUMP_HEIGHT = 5f;
    private const float BUNNY_AIR_CONTROL = 0.65f;
    private const float BEE_JUMP_HEIGHT = 3.4f;
    private const float BEE_GLIDE_FALL_SPEED = 2.2f;
    private const float BEE_GLIDE_FORWARD_MULTIPLIER = 0.85f;

    // ========================= STATIC METHODS ========================

    public static void EnsureForms(out PetForm dog, out PetForm bunny, out PetForm bee)
    {
        var dogStats = AssetDatabase.LoadAssetAtPath<PetMovementStats>(DOG_STATS_PATH);
        PetMovementStats bunnyStats = LoadOrCopyStats(dogStats, "Bunny", stats =>
        {
            stats.FindProperty("_jumpHeight").floatValue = BUNNY_JUMP_HEIGHT;
            stats.FindProperty("_airControl").floatValue = BUNNY_AIR_CONTROL;
        });
        PetMovementStats beeStats = LoadOrCopyStats(dogStats, "Bee", stats =>
        {
            stats.FindProperty("_jumpHeight").floatValue = BEE_JUMP_HEIGHT;
            stats.FindProperty("_canGlide").boolValue = true;
            stats.FindProperty("_glideFallSpeed").floatValue = BEE_GLIDE_FALL_SPEED;
            stats.FindProperty("_glideForwardMultiplier").floatValue = BEE_GLIDE_FORWARD_MULTIPLIER;
        });

        dog = LoadOrCreateForm("Dog", "animal-dog", dogStats, "Balanced run and jump", "Balanced run and jump");
        bunny = LoadOrCreateForm("Bunny", "animal-bunny", bunnyStats, "Super high jump!", "Super high jump!");
        bee = LoadOrCreateForm("Bee", "animal-bee", beeStats, "Hold SPACE to glide", "Swipe up and hold to glide");
    }

    public static string ModelPath(string modelName)
    {
        return $"{MODELS_FOLDER}/{modelName}.fbx";
    }

    private static PetMovementStats LoadOrCopyStats(PetMovementStats source, string formName, System.Action<SerializedObject> tune)
    {
        string path = $"{PETS_FOLDER}/{formName}MovementStats.asset";
        var stats = AssetDatabase.LoadAssetAtPath<PetMovementStats>(path);
        if (stats != null) return stats;

        stats = Object.Instantiate(source);
        AssetDatabase.CreateAsset(stats, path);

        var serializedStats = new SerializedObject(stats);
        tune(serializedStats);
        serializedStats.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        return stats;
    }

    private static PetForm LoadOrCreateForm(string formName, string modelName, PetMovementStats stats, string ability, string touchAbility)
    {
        string path = $"{PETS_FOLDER}/{formName}Form.asset";
        var form = AssetDatabase.LoadAssetAtPath<PetForm>(path);
        if (form == null)
        {
            form = ScriptableObject.CreateInstance<PetForm>();
            AssetDatabase.CreateAsset(form, path);
        }

        string modelPath = ModelPath(modelName);
        var serializedForm = new SerializedObject(form);
        serializedForm.FindProperty("_displayName").stringValue = formName.ToUpperInvariant();
        serializedForm.FindProperty("_abilityText").stringValue = ability;
        serializedForm.FindProperty("_touchAbilityText").stringValue = touchAbility;
        serializedForm.FindProperty("_icon").objectReferenceValue = IconRenderer.EnsurePortrait(modelPath);
        serializedForm.FindProperty("_modelPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        serializedForm.FindProperty("_animatorController").objectReferenceValue = PetAnimatorBuilder.GetOrCreateController(modelPath);
        serializedForm.FindProperty("_stats").objectReferenceValue = stats;
        serializedForm.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        return form;
    }
}
