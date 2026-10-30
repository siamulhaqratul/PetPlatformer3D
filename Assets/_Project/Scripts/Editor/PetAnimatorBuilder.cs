using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PetAnimatorBuilder
{
    // =========================== CONSTANTS ===========================

    private const string CONTROLLER_FOLDER = "Assets/_Project/Animation";
    private const string PREVIEW_CLIP_PREFIX = "__preview__";
    private const string IDLE_STATE = "idle";

    private static readonly string[] _loopingClips = { "idle", "run", "walk", "dance" };
    private static readonly string[] _stateClips = { "idle", "run", "dance", "gesture-positive" };

    // ========================= STATIC METHODS ========================

    public static AnimatorController GetOrCreateController(string modelPath)
    {
        string modelName = System.IO.Path.GetFileNameWithoutExtension(modelPath);
        string controllerPath = $"{CONTROLLER_FOLDER}/{modelName}.controller";

        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (existing != null) return existing;

        EnsureLoopingClips(modelPath);
        Dictionary<string, AnimationClip> clips = LoadClips(modelPath);

        if (!AssetDatabase.IsValidFolder(CONTROLLER_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Animation");
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        foreach (string stateName in _stateClips)
        {
            if (!clips.TryGetValue(stateName, out AnimationClip clip))
            {
                Debug.LogWarning($"{modelName} has no '{stateName}' clip.");
                continue;
            }

            AnimatorState state = stateMachine.AddState(stateName);
            state.motion = clip;

            if (stateName == IDLE_STATE)
            {
                stateMachine.defaultState = state;
            }
        }

        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void EnsureLoopingClips(string modelPath)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
        ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0
            ? importer.clipAnimations
            : importer.defaultClipAnimations;

        foreach (ModelImporterClipAnimation clip in clips)
        {
            clip.loopTime = _loopingClips.Contains(clip.name);
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    private static Dictionary<string, AnimationClip> LoadClips(string modelPath)
    {
        return AssetDatabase.LoadAllAssetsAtPath(modelPath)
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith(PREVIEW_CLIP_PREFIX))
            .ToDictionary(clip => clip.name);
    }
}
