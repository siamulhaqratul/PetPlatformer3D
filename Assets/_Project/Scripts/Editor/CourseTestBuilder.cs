using Cinemachine;
using UnityEditor;
using UnityEngine;

public static class CourseTestBuilder
{
    // =========================== CONSTANTS ===========================

    private const string UNDO_NAME = "Build Game Systems";
    private const string GAME_MANAGER_NAME = "GameManager";
    private const string OLD_COURSE_LOGIC_NAME = "_CourseLogic";
    private const string COURSE_EFFECTS_NAME = "CourseEffects";
    private const float SHAKE_DURATION = 0.25f;

    // Kenney Digital Audio, Impact Sounds and Music Jingles (CC0). Swap any of these to change a sound.
    private const string AUDIO_FOLDER = "Assets/_Project/Audio/Kenney";
    private const string JUMP_SOUND = AUDIO_FOLDER + "/DigitalAudio/phaseJump1.ogg";
    private const string LAND_SOUND = AUDIO_FOLDER + "/ImpactSounds/impactSoft_medium_000.ogg";
    private const string ROLL_SOUND = AUDIO_FOLDER + "/ImpactSounds/footstep_grass_000.ogg";
    private const string HIT_SOUND = AUDIO_FOLDER + "/DigitalAudio/lowDown.ogg";
    private const string RESPAWN_SOUND = AUDIO_FOLDER + "/DigitalAudio/highUp.ogg";
    private const string FORM_CHANGE_SOUND = AUDIO_FOLDER + "/DigitalAudio/powerUp4.ogg";
    private const string CHECKPOINT_SOUND = AUDIO_FOLDER + "/DigitalAudio/twoTone1.ogg";
    private const string WIN_JINGLE = AUDIO_FOLDER + "/MusicJingles/jingles_NES12.ogg";
    private const string FAIL_JINGLE = AUDIO_FOLDER + "/MusicJingles/jingles_NES13.ogg";
    // "Retro Game" by The_Mountain, from Pixabay (track 593063), free to use under the Pixabay Content License.
    private const string MUSIC_TRACK = "Assets/_Project/Audio/Music/RetroGame.mp3";

    // ========================= STATIC METHODS ========================

    public static void BuildGameSystems(GameObject pet, CinemachineVirtualCamera followCamera)
    {
        PetTestSceneBuilder.DestroyExisting(GAME_MANAGER_NAME);
        PetTestSceneBuilder.DestroyExisting(OLD_COURSE_LOGIC_NAME);

        Transform spawnPoint = LevelBuilder.BuildLevel();
        EnvironmentBuilder.BuildEnvironment();
        BuildGameManager(pet, followCamera, spawnPoint);
    }

    private static void BuildGameManager(GameObject pet, CinemachineVirtualCamera followCamera, Transform spawnPoint)
    {
        var managerObject = new GameObject(GAME_MANAGER_NAME);
        Undo.RegisterCreatedObjectUndo(managerObject, UNDO_NAME);

        var culler = managerObject.AddComponent<CourseCuller>();
        var serializedCuller = new SerializedObject(culler);
        serializedCuller.FindProperty("_pet").objectReferenceValue = pet.transform;
        Transform[] cullTargets = LevelBuilder.CullTargets;
        SerializedProperty targets = serializedCuller.FindProperty("_targets");
        targets.arraySize = cullTargets.Length;
        for (int index = 0; index < cullTargets.Length; index++)
        {
            targets.GetArrayElementAtIndex(index).objectReferenceValue = cullTargets[index];
        }

        serializedCuller.ApplyModifiedPropertiesWithoutUndo();

        var manager = managerObject.AddComponent<GameManager>();
        var serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("_motor").objectReferenceValue = pet.GetComponent<PetMotor>();
        serializedManager.FindProperty("_visual").objectReferenceValue = pet.GetComponent<PetVisual>();
        serializedManager.FindProperty("_input").objectReferenceValue = pet.GetComponent<PetInput>();
        serializedManager.FindProperty("_formController").objectReferenceValue = pet.GetComponent<PetFormController>();
        serializedManager.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint;
        serializedManager.FindProperty("_followCamera").objectReferenceValue = followCamera;
        serializedManager.FindProperty("_autoStartRun").boolValue = false;
        serializedManager.FindProperty("_totalCoins").intValue = LevelBuilder.CoinCount;
        serializedManager.FindProperty("_finishPoint").objectReferenceValue = LevelBuilder.FinishPoint;
        serializedManager.FindProperty("_courseCuller").objectReferenceValue = culler;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();

        BuildGameAudio(managerObject, manager, pet);
        BuildCourseEffects(managerObject, manager, pet, followCamera);
    }

    private static void BuildCourseEffects(GameObject managerObject, GameManager manager, GameObject pet, CinemachineVirtualCamera followCamera)
    {
        var effectsObject = new GameObject(COURSE_EFFECTS_NAME);
        effectsObject.transform.SetParent(managerObject.transform, false);

        Material material = PetTestSceneBuilder.LoadOrCreateDustMaterial();
        ParticleSystem coinSparkle = CourseEffectsFactory.CreateCoinSparkle(effectsObject.transform, material);
        ParticleSystem checkpointBurst = CourseEffectsFactory.CreateCheckpointBurst(effectsObject.transform, material);
        ParticleSystem confetti = CourseEffectsFactory.CreateConfetti(effectsObject.transform, material);

        // A short, uniform bump: felt the same wherever the camera is, gone in a quarter of a second.
        var shakeSource = effectsObject.AddComponent<CinemachineImpulseSource>();
        shakeSource.m_ImpulseDefinition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        shakeSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        shakeSource.m_ImpulseDefinition.m_ImpulseDuration = SHAKE_DURATION;
        if (followCamera.GetComponent<CinemachineImpulseListener>() == null)
        {
            followCamera.gameObject.AddComponent<CinemachineImpulseListener>();
        }

        var effects = effectsObject.AddComponent<CourseEffects>();
        var serializedEffects = new SerializedObject(effects);
        serializedEffects.FindProperty("_gameManager").objectReferenceValue = manager;
        serializedEffects.FindProperty("_pet").objectReferenceValue = pet.transform;
        serializedEffects.FindProperty("_coinSparkle").objectReferenceValue = coinSparkle;
        serializedEffects.FindProperty("_checkpointBurst").objectReferenceValue = checkpointBurst;
        serializedEffects.FindProperty("_confetti").objectReferenceValue = confetti;
        serializedEffects.FindProperty("_shakeSource").objectReferenceValue = shakeSource;
        serializedEffects.ApplyModifiedPropertiesWithoutUndo();
    }

    // Effects, jingles and music get separate sources, so an effect's random pitch never bends a jingle or the music.
    private static void BuildGameAudio(GameObject managerObject, GameManager manager, GameObject pet)
    {
        var effectsSource = managerObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        var jingleSource = managerObject.AddComponent<AudioSource>();
        jingleSource.playOnAwake = false;
        var musicSource = managerObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        var gameAudio = managerObject.AddComponent<GameAudio>();
        var serializedAudio = new SerializedObject(gameAudio);
        serializedAudio.FindProperty("_gameManager").objectReferenceValue = manager;
        serializedAudio.FindProperty("_motor").objectReferenceValue = pet.GetComponent<PetMotor>();
        serializedAudio.FindProperty("_formController").objectReferenceValue = pet.GetComponent<PetFormController>();
        serializedAudio.FindProperty("_effectsSource").objectReferenceValue = effectsSource;
        serializedAudio.FindProperty("_jingleSource").objectReferenceValue = jingleSource;
        serializedAudio.FindProperty("_musicSource").objectReferenceValue = musicSource;
        SetClip(serializedAudio, "_musicClip", MUSIC_TRACK);
        SetClip(serializedAudio, "_jumpClip", JUMP_SOUND);
        SetClip(serializedAudio, "_landClip", LAND_SOUND);
        SetClip(serializedAudio, "_rollClip", ROLL_SOUND);
        SetClip(serializedAudio, "_hitClip", HIT_SOUND);
        SetClip(serializedAudio, "_respawnClip", RESPAWN_SOUND);
        SetClip(serializedAudio, "_formChangeClip", FORM_CHANGE_SOUND);
        SetClip(serializedAudio, "_checkpointClip", CHECKPOINT_SOUND);
        SetClip(serializedAudio, "_winJingle", WIN_JINGLE);
        SetClip(serializedAudio, "_failJingle", FAIL_JINGLE);
        serializedAudio.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetClip(SerializedObject target, string property, string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null)
        {
            Debug.LogError($"Sound not found: {path}");
        }

        target.FindProperty(property).objectReferenceValue = clip;
    }
}
