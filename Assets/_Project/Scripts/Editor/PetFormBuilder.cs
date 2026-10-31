using UnityEditor;
using UnityEngine;

// Creates the Dog, Bunny, Cat, Monkey and Penguin forms: each with its Cube Pets model, animator, icon and movement stats.
// Every gate animal is the Dog's stats with one strength turned up and a lower jump, so the strengths stay distinct.
public static class PetFormBuilder
{
    // =========================== CONSTANTS ===========================

    private const string PETS_FOLDER = "Assets/_Project/Settings/Pets";
    private const string MODELS_FOLDER = "Assets/_Project/Art/Kenney/CubePets/Models";
    private const string DOG_STATS_PATH = PETS_FOLDER + "/DogMovementStats.asset";

    // Jump for every gate animal but the Bunny, so the Bunny's high jump stands out.
    private const float LOW_JUMP_HEIGHT = 3.4f;
    private const float BUNNY_JUMP_HEIGHT = 5f;
    private const float BUNNY_AIR_CONTROL = 0.65f;
    // About twice the Dog's sideways speed, so the Cat switches lanes almost instantly.
    private const float CAT_STEER_SPEED = 14f;
    private const float CAT_STEER_ACCELERATION = 90f;
    private const float CAT_STEER_DECELERATION = 120f;
    // Climbs a ledge this far above its feet: it runs straight up the 3-high steps without jumping.
    private const float MONKEY_LEDGE_ASSIST_HEIGHT = 3f;
    // One slide covers about 15 units at run speed, three times the Dog's.
    private const float PENGUIN_ROLL_DURATION = 1.8f;

    // ========================= STATIC METHODS ========================

    public static void EnsureForms(out PetForm dog, out PetForm bunny, out PetForm cat, out PetForm monkey, out PetForm penguin)
    {
        var dogStats = AssetDatabase.LoadAssetAtPath<PetMovementStats>(DOG_STATS_PATH);
        PetMovementStats bunnyStats = LoadOrCopyStats(dogStats, "Bunny", stats =>
        {
            stats.FindProperty("_jumpHeight").floatValue = BUNNY_JUMP_HEIGHT;
            stats.FindProperty("_airControl").floatValue = BUNNY_AIR_CONTROL;
        });
        PetMovementStats catStats = LoadOrCopyStats(dogStats, "Cat", stats =>
        {
            stats.FindProperty("_jumpHeight").floatValue = LOW_JUMP_HEIGHT;
            stats.FindProperty("_steerSpeed").floatValue = CAT_STEER_SPEED;
            stats.FindProperty("_steerAcceleration").floatValue = CAT_STEER_ACCELERATION;
            stats.FindProperty("_steerDeceleration").floatValue = CAT_STEER_DECELERATION;
        });
        PetMovementStats monkeyStats = LoadOrCopyStats(dogStats, "Monkey", stats =>
        {
            stats.FindProperty("_jumpHeight").floatValue = LOW_JUMP_HEIGHT;
            stats.FindProperty("_ledgeAssistHeight").floatValue = MONKEY_LEDGE_ASSIST_HEIGHT;
        });
        PetMovementStats penguinStats = LoadOrCopyStats(dogStats, "Penguin", stats =>
        {
            stats.FindProperty("_jumpHeight").floatValue = LOW_JUMP_HEIGHT;
            stats.FindProperty("_rollDuration").floatValue = PENGUIN_ROLL_DURATION;
        });

        dog = LoadOrCreateForm("Dog", "animal-dog", dogStats, "Balanced run and jump", "Balanced run and jump");
        bunny = LoadOrCreateForm("Bunny", "animal-bunny", bunnyStats, "Super high jump!", "Super high jump!");
        // Ability lines stay short enough to fit the gate banner at full size.
        cat = LoadOrCreateForm("Cat", "animal-cat", catStats, "Super-fast lane changes", "Super-fast lane changes");
        monkey = LoadOrCreateForm("Monkey", "animal-monkey", monkeyStats, "Climbs tall ledges", "Climbs tall ledges");
        penguin = LoadOrCreateForm("Penguin", "animal-penguin", penguinStats, "Press S for a long slide", "Swipe down for a long slide");
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
