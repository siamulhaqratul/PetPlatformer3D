using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class LevelBuilder
{
    // =========================== CONSTANTS ===========================

    private const string UNDO_NAME = "Build Level";
    private const string LEVEL_NAME = "_Level";
    private const string KIT_MODELS_FOLDER = "Assets/_Project/Art/Kenney/PlatformerKit/Models";
    private const float KIT_SCALE = 2f;
    private const float TILE_SIZE = 4f;
    private const float TILE_HEIGHT = 2f;
    private const float TILE_MODEL_SIZE = 2f;
    private const float GAP_LENGTH = 4f;
    private const float STEP_HEIGHT = 3f;
    // Each gate segment has its checkpoint at the start and the gate pair in the middle.
    private const float GATE_SEGMENT_LENGTH = 28f;
    private const string COIN = "coin-gold";
    private const float COIN_BODY_HEIGHT = 0.6f;
    private const float COIN_SCALE = 1.5f;
    private const float COIN_MODEL_CENTER = 0.2f;
    private const float COIN_TRIGGER_RADIUS = 0.45f;
    private const string OUTLINE_SHADER_NAME = "PetShiftRun/Outline";
    private const string COIN_OUTLINE_MATERIAL_PATH = "Assets/_Project/Materials/CoinOutline.mat";
    // The outline copy is this much bigger than the coin, which leaves a rim about a tenth of the coin's width.
    private const float COIN_OUTLINE_SCALE = 1.2f;
    // At 1 the arc follows the Dog's real jump (see _arcHeight). Plus COIN_BODY_HEIGHT, each coin then sits in the
    // middle of what a jumping pet can reach (0.675 under its feet to 2.25 above), which forgives early or late jumps.
    private const float ARC_HEIGHT_SCALE = 1f;
    private const float WALL_CLEARANCE = 1f;
    private const float STEP_LANDING_COIN_HEIGHT = 1.2f;
    private const float COIN_LINE_SPACING = 2.5f;
    private const int COIN_LINE_COUNT = 3;
    private const float ARC_TAKEOFF_LEAD = 1.5f;
    private const float STEP_TAKEOFF_LEAD = 3f;

    // Forward distance and height of the jump curve, measured from the takeoff point.
    private static readonly float[] _arcForward = { 1.5f, 3.5f, 5.5f };
    private static readonly float[] _arcHeight = { 2.3f, 3.6f, 2.9f };
    private const float PLATFORM_THICKNESS = 4f;
    private const float LEFT_LANE = -2f;
    private const float RIGHT_LANE = 2f;
    private const float SIDE_WALL_THICKNESS = 0.5f;
    private const float SIDE_WALL_HEIGHT = 4f;
    private const float SIDE_WALL_INSET = 0.4f;
    private const float FENCE_LENGTH = 2f;
    private const float FENCE_MODEL_OFFSET = 0.42f;
    private const float FENCE_EDGE_INSET = 0.25f;
    private const float BORDER_DECOR_INSET = 0.9f;
    private const float BORDER_DECOR_SCALE = 1.2f;
    private const float SPAWN_HEIGHT = 0.1f;
    private const float TRIGGER_HEIGHT = 20f;
    private const float TRIGGER_DEPTH = 4f;
    private const float COURSE_TRIGGER_WIDTH = 10f;
    private const float HAZARD_TRIGGER_INSET = 0.8f;
    private const float SPIKE_BLOCK_SCALE = 1.2f;
    private const int SPIKES_PER_LANE = 3;
    private const float SPIKE_LANE_START_X = 1.1f;
    private const float SPIKE_BLOCK_SPACING = 1.05f;
    private const string GATE_DOOR = "door-large-open";
    private const float GATE_SCALE = 3.6f;
    // Each door spans from the track centre to the outer edge, so the pair meets in the middle with no gap.
    private const float GATE_WIDTH = 4f;
    private const float GATE_TRIGGER_HEIGHT = 3f;
    private const float GATE_ICON_HEIGHT = 4.4f;
    private const float GATE_ICON_SCALE = 1.1f;
    private const float GATE_ICON_YAW = 180f;
    private const int OVERHEAD_BAR_BLOCKS = 7;
    private const float OVERHEAD_BAR_HALF_WIDTH = 3.15f;
    private const float OVERHEAD_BAR_CLEARANCE = 1.25f;
    private const float ROLL_COIN_DROP = -0.15f;
    private const float LEFT = -1f;
    private const float RIGHT = 1f;
    private const float FLAG_SCALE = 3f;
    private const float SIDE_ISLAND_SPACING = 13f;
    private const float SIDE_ISLAND_NEAR_X = 13f;
    private const float SIDE_ISLAND_FAR_X = 24f;
    private const int RANDOM_SEED = 7;
    // The moving-block model is 1 wide, 1 long and 0.5 tall, so this scale makes one lane-wide, TILE_SIZE-long block.
    private const float SLIDER_BLOCK_SCALE = TILE_SIZE;
    private const float SLIDER_THICKNESS = 0.5f * SLIDER_BLOCK_SCALE;
    private const int SLIDER_BLOCK_COUNT = 4;
    private const float SLIDER_PERIOD = 3.2f;

    private const string GRASS_TILE = "block-grass-large";
    private const string SLIDER_BLOCK = "block-moving-large";
    private const string SNOW_TILE = "block-snow-large";
    private const string SPIKES = "spike-block";
    private const string FLAG = "flag";
    private const string FENCE = "fence-straight";

    private static readonly float[] _bothLanes = { LEFT_LANE, RIGHT_LANE };
    private static readonly float[] _leftLane = { LEFT_LANE };
    private static readonly float[] _rightLane = { RIGHT_LANE };
    private static readonly string[] _grassDecor = { "flowers", "flowers-tall", "grass", "mushrooms", "plant" };
    private static readonly string[] _snowDecor = { "rocks", "stones" };
    private static readonly string[] _grassTrees = { "tree", "tree-pine", "tree-pine-small" };
    private static readonly string[] _snowTrees = { "tree-pine-snow", "tree-pine-snow-small", "tree-snow" };
    // Same dark navy as the UI's text outlines.
    private static readonly Color _coinOutlineColor = new Color(0.12f, 0.13f, 0.2f);

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private static Transform _courseRoot;
    private static Transform _decorRoot;
    private static Transform _logicRoot;
    private static Transform _coinRoot;
    private static Transform _finishPoint;
    private static PetFormController _formController;
    private static PetForm _bunnyForm;
    private static PetForm _catForm;
    private static PetForm _monkeyForm;
    private static PetForm _penguinForm;
    private static readonly List<Transform> _cullTargets = new List<Transform>();
    private static int _coinCount;
    private static System.Random _random;
    private static float _cursorZ;
    private static readonly List<Vector3> _snowZones = new List<Vector3>();

    // ========================= STATIC METHODS ========================

    public static int CoinCount => _coinCount;
    public static Transform FinishPoint => _finishPoint;
    public static Transform[] CullTargets => _cullTargets.OrderBy(target => target.position.z).ToArray();

    public static Transform BuildLevel()
    {
        PetTestSceneBuilder.DestroyExisting(LEVEL_NAME);

        var levelRoot = new GameObject(LEVEL_NAME).transform;
        Undo.RegisterCreatedObjectUndo(levelRoot.gameObject, UNDO_NAME);
        _courseRoot = CreateGroup("Course", levelRoot);
        _decorRoot = CreateGroup("Decor", levelRoot);
        _logicRoot = CreateGroup("Logic", levelRoot);
        _coinRoot = CreateGroup("Coins", levelRoot);
        _random = new System.Random(RANDOM_SEED);
        _cursorZ = -6f;
        _coinCount = 0;
        _cullTargets.Clear();

        Transform spawnPoint = CreateMarker("SpawnPoint", _logicRoot, new Vector3(0f, SPAWN_HEIGHT, 0f));
        BuildMeadow();
        BuildCliffSteps();
        BuildSkyBridge();
        float finishZ = BuildSummit();
        BuildSideIslands(finishZ);
        return spawnPoint;
    }

    // ----- Course sections -----

    // Double-lane platforms are 24-40 long and fenced; single-lane platforms are 14-16 long and open.
    // Every gap is GAP_LENGTH and leads to the same height or lower, so a jump from within
    // 2 units of an edge always lands; climbing happens only on gapless step-ups.
    // Coins only mark the way: short lines on the safe lane, and arcs where the jump climbs, changes lane
    // or clears spikes. Plain straight gaps get none.
    private static void BuildMeadow()
    {
        float startZ = _cursorZ;
        Platform(40f, 0f, _bothLanes, false);
        CoinLine(LEFT_LANE, 0f, startZ + 10f);
        SpikeLane(RIGHT, 0f, 20f);
        CoinArcOver(RIGHT_LANE, 0f, 20f);

        Gap();
        startZ = _cursorZ;
        Platform(GATE_SEGMENT_LENGTH, 0f, _bothLanes, false);
        Checkpoint(startZ + 1f, 0f);
        CoinLine(0f, 0f, startZ + 4f);
        GatePair(startZ + GATE_SEGMENT_LENGTH * 0.5f, 0f, _bunnyForm, _catForm);

        Gap();
        startZ = _cursorZ;
        Platform(24f, 0f, _bothLanes, false);
        OverheadBar(0f, startZ + 10f);
        CoinLine(RIGHT_LANE, ROLL_COIN_DROP, startZ + 6f);
        CoinArcFromEdge(RIGHT_LANE, RIGHT_LANE, 0f, STEP_HEIGHT);
    }

    private static void BuildCliffSteps()
    {
        float startZ = _cursorZ;
        Platform(24f, STEP_HEIGHT, _bothLanes, false);
        CoinLine(0f, STEP_HEIGHT, startZ + 6f);
        CoinArcFromEdge(0f, 0f, STEP_HEIGHT, STEP_HEIGHT * 2f);

        startZ = _cursorZ;
        Platform(24f, STEP_HEIGHT * 2f, _bothLanes, false);
        CoinLine(RIGHT_LANE, STEP_HEIGHT * 2f, startZ + 8f);
        Gap();

        startZ = _cursorZ;
        Platform(16f, STEP_HEIGHT * 2f, _rightLane, false);
        CoinLine(RIGHT_LANE, STEP_HEIGHT * 2f, startZ + 4f);

        Gap();
        startZ = _cursorZ;
        Platform(GATE_SEGMENT_LENGTH, STEP_HEIGHT, _bothLanes, false);
        Checkpoint(startZ + 1f, STEP_HEIGHT);
        CoinLine(RIGHT_LANE, STEP_HEIGHT, startZ + 6f);
        GatePair(startZ + GATE_SEGMENT_LENGTH * 0.5f, STEP_HEIGHT, _monkeyForm, _penguinForm);
        SpikeLane(LEFT, STEP_HEIGHT, startZ + 21f);
        CoinArcOver(LEFT_LANE, STEP_HEIGHT, startZ + 21f);
        CoinArcFromEdge(0f, LEFT_LANE, STEP_HEIGHT);
    }

    private static void BuildSkyBridge()
    {
        Gap();
        float startZ = _cursorZ;
        Platform(14f, STEP_HEIGHT, _leftLane, false);
        CoinLine(LEFT_LANE, STEP_HEIGHT, startZ + 4f);
        CoinArcFromEdge(LEFT_LANE, RIGHT_LANE, STEP_HEIGHT);

        Gap();
        startZ = _cursorZ;
        Platform(14f, STEP_HEIGHT, _rightLane, false);
        CoinLine(RIGHT_LANE, STEP_HEIGHT, startZ + 4f);

        // The slider swings between the lanes, so the jump onto it is timed, and it carries the pet across.
        Gap();
        Slider(STEP_HEIGHT);

        Gap();
        startZ = _cursorZ;
        Platform(24f, 0f, _bothLanes, false);
        Checkpoint(startZ + 1f, 0f);
        CoinLine(0f, 0f, startZ + 8f);
    }

    private static float BuildSummit()
    {
        float snowStartZ = _cursorZ;

        Gap();
        float startZ = _cursorZ;
        Platform(24f, 0f, _bothLanes, true);
        CoinLine(LEFT_LANE, 0f, startZ + 6f);
        CoinArcFromEdge(LEFT_LANE, LEFT_LANE, 0f, STEP_HEIGHT);

        startZ = _cursorZ;
        Platform(24f, STEP_HEIGHT, _bothLanes, true);
        float spikeZ = _cursorZ - 10f;
        SpikeLane(RIGHT, STEP_HEIGHT, spikeZ);
        CoinArcOver(RIGHT_LANE, STEP_HEIGHT, spikeZ);
        CoinLine(LEFT_LANE, STEP_HEIGHT, startZ + 4f);
        Gap();

        startZ = _cursorZ;
        float finishZ = _cursorZ + 16f;
        Platform(40f, STEP_HEIGHT, _bothLanes, true);
        OverheadBar(STEP_HEIGHT, startZ + 7f);
        CoinLine(0f, STEP_HEIGHT + ROLL_COIN_DROP, startZ + 3f);
        Finish(finishZ, STEP_HEIGHT);

        _snowZones.Clear();
        _snowZones.Add(new Vector3(snowStartZ, 0f, _cursorZ + 40f));
        return finishZ;
    }

    // ----- Animal gates and form rewards -----

    public static void ConfigureForms(PetFormController formController, PetForm bunnyForm, PetForm catForm, PetForm monkeyForm, PetForm penguinForm)
    {
        _formController = formController;
        _bunnyForm = bunnyForm;
        _catForm = catForm;
        _monkeyForm = monkeyForm;
        _penguinForm = penguinForm;
    }

    // Two doors side by side, one per lane; running through one turns the pet into that animal.
    private static void GatePair(float z, float topY, PetForm leftForm, PetForm rightForm)
    {
        var pairObject = new GameObject($"GatePair_{z:000}");
        pairObject.transform.SetParent(_logicRoot);
        pairObject.transform.position = new Vector3(0f, topY, z);
        var pair = pairObject.AddComponent<AnimalGatePair>();

        AnimalGate leftGate = BuildGate(pairObject.transform, pair, leftForm, LEFT_LANE, topY, z);
        AnimalGate rightGate = BuildGate(pairObject.transform, pair, rightForm, RIGHT_LANE, topY, z);

        var serializedPair = new SerializedObject(pair);
        serializedPair.FindProperty("_formController").objectReferenceValue = _formController;
        SerializedProperty gates = serializedPair.FindProperty("_gates");
        gates.arraySize = 2;
        gates.GetArrayElementAtIndex(0).objectReferenceValue = leftGate;
        gates.GetArrayElementAtIndex(1).objectReferenceValue = rightGate;
        serializedPair.ApplyModifiedPropertiesWithoutUndo();
    }

    private static AnimalGate BuildGate(Transform parent, AnimalGatePair pair, PetForm form, float laneX, float topY, float z)
    {
        var gateObject = new GameObject($"Gate_{form.DisplayName}");
        gateObject.transform.SetParent(parent);
        gateObject.transform.position = new Vector3(laneX, topY, z);

        var trigger = gateObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = Vector3.up * GATE_TRIGGER_HEIGHT * 0.5f;
        trigger.size = new Vector3(GATE_WIDTH, GATE_TRIGGER_HEIGHT, TRIGGER_DEPTH);

        // The door model is one unit wide, so its x scale is its width.
        GameObject door = PlaceKit(GATE_DOOR, gateObject.transform, gateObject.transform.position);
        door.transform.localScale = new Vector3(GATE_WIDTH, GATE_SCALE, GATE_SCALE);
        door.isStatic = false;

        var icon = (GameObject)PrefabUtility.InstantiatePrefab(form.ModelPrefab, gateObject.transform);
        icon.name = "Icon";
        icon.transform.position = new Vector3(laneX, topY + GATE_ICON_HEIGHT, z);
        icon.transform.rotation = Quaternion.Euler(0f, GATE_ICON_YAW, 0f);
        icon.transform.localScale = Vector3.one * GATE_ICON_SCALE;

        var gate = gateObject.AddComponent<AnimalGate>();
        var serializedGate = new SerializedObject(gate);
        serializedGate.FindProperty("_pair").objectReferenceValue = pair;
        serializedGate.FindProperty("_form").objectReferenceValue = form;
        serializedGate.FindProperty("_icon").objectReferenceValue = icon.transform;
        serializedGate.FindProperty("_door").objectReferenceValue = door.transform;
        serializedGate.ApplyModifiedPropertiesWithoutUndo();
        return gate;
    }

    // A full-width row of floating spike blocks at head height: roll under it or jump over it.
    private static void OverheadBar(float topY, float z)
    {
        for (int index = 0; index < OVERHEAD_BAR_BLOCKS; index++)
        {
            float x = -OVERHEAD_BAR_HALF_WIDTH + index * SPIKE_BLOCK_SPACING;
            PlaceSpikes($"OverheadSpikes_{z:000}_{index}", new Vector3(x, topY + OVERHEAD_BAR_CLEARANCE, z));
        }
    }

    // ----- Coins -----

    private static void CoinLine(float x, float topY, float startZ)
    {
        for (int index = 0; index < COIN_LINE_COUNT; index++)
        {
            PlaceCoin(new Vector3(x, topY + COIN_BODY_HEIGHT, startZ + index * COIN_LINE_SPACING));
        }
    }

    // Arc that follows a jump taken just before the current platform edge (a gap or a step-up).
    // landingTopY is the height of the next platform; coins past the edge never sink below it.
    // Step-ups start further back: from there a jump clears the step in one arc instead of sliding up the face.
    private static void CoinArcFromEdge(float fromX, float toX, float topY, float landingTopY = float.MinValue)
    {
        bool isStepUp = landingTopY > topY;
        float takeoffLead = isStepUp ? STEP_TAKEOFF_LEAD : ARC_TAKEOFF_LEAD;
        CoinArc(fromX, toX, topY, _cursorZ - takeoffLead, _cursorZ, landingTopY);
    }

    // Arc that peaks right above an obstacle at obstacleZ.
    private static void CoinArcOver(float x, float topY, float obstacleZ)
    {
        CoinArc(x, x, topY, obstacleZ - _arcForward[1], float.MaxValue, float.MinValue);
    }

    private static void CoinArc(float fromX, float toX, float topY, float takeoffZ, float edgeZ, float landingTopY)
    {
        for (int index = 0; index < _arcForward.Length; index++)
        {
            float blend = (index + 1f) / _arcForward.Length;
            float x = Mathf.Lerp(fromX, toX, blend);
            float z = takeoffZ + _arcForward[index];
            float y = topY + _arcHeight[index] * ARC_HEIGHT_SCALE + COIN_BODY_HEIGHT;

            bool isStepUp = landingTopY > topY;
            if (isStepUp && Mathf.Abs(z - edgeZ) < WALL_CLEARANCE)
            {
                z = edgeZ - WALL_CLEARANCE;
            }

            // The ledge assist lifts the pet while it is still rising, so it crosses a step higher than a plain jump.
            float landingHeight = isStepUp ? STEP_LANDING_COIN_HEIGHT : COIN_BODY_HEIGHT;
            if (z > edgeZ)
            {
                y = Mathf.Max(y, landingTopY + landingHeight);
            }

            PlaceCoin(new Vector3(x, y, z));
        }
    }

    private static void PlaceCoin(Vector3 center)
    {
        GameObject coin = PlaceKit(COIN, _coinRoot, center + Vector3.down * COIN_MODEL_CENTER * COIN_SCALE);
        coin.name = $"Coin_{_coinCount:00}";
        coin.isStatic = false;
        coin.transform.localScale = Vector3.one * COIN_SCALE;
        AddOutline(coin.GetComponent<MeshRenderer>());

        var trigger = coin.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.center = Vector3.up * COIN_MODEL_CENTER;
        trigger.radius = COIN_TRIGGER_RADIUS;
        coin.AddComponent<Coin>();
        _coinCount++;
        _cullTargets.Add(coin.transform);
    }

    // A second material on a one-submesh renderer draws the whole mesh again, here as the dark outline behind it.
    private static void AddOutline(MeshRenderer renderer)
    {
        Material outline = LoadOrCreateCoinOutline(renderer.GetComponent<MeshFilter>().sharedMesh);
        renderer.sharedMaterials = new[] { renderer.sharedMaterial, outline };
    }

    // Scales around the mesh's own centre, which sits above the model's pivot.
    private static Material LoadOrCreateCoinOutline(Mesh coinMesh)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(COIN_OUTLINE_MATERIAL_PATH);
        if (material == null)
        {
            material = new Material(Shader.Find(OUTLINE_SHADER_NAME));
            AssetDatabase.CreateAsset(material, COIN_OUTLINE_MATERIAL_PATH);
        }

        material.SetColor("_OutlineColor", _coinOutlineColor);
        material.SetFloat("_OutlineScale", COIN_OUTLINE_SCALE);
        material.SetVector("_Center", coinMesh.bounds.center);
        EditorUtility.SetDirty(material);
        return material;
    }

    // ----- Course pieces -----

    private static void Gap()
    {
        _cursorZ += GAP_LENGTH;
    }

    private static void Platform(float length, float topY, float[] lanes, bool isSnow)
    {
        string tileModel = isSnow ? SNOW_TILE : GRASS_TILE;
        float width = lanes.Length * TILE_SIZE;
        float centerX = (lanes[0] + lanes[lanes.Length - 1]) * 0.5f;
        var center = new Vector3(centerX, topY - PLATFORM_THICKNESS, _cursorZ + length * 0.5f);

        // One stretched block per platform, so rounded tile edges leave no seams to see or snag on.
        GameObject block = PlaceKit(tileModel, _courseRoot, center);
        block.transform.localScale = new Vector3(width / TILE_MODEL_SIZE, PLATFORM_THICKNESS, length / TILE_MODEL_SIZE);
        AddBoxCollider(block, false);

        bool isWalled = lanes.Length > 1;
        if (isWalled)
        {
            BuildSideBorder(-1f, width, topY, length, isSnow);
            BuildSideBorder(1f, width, topY, length, isSnow);
        }

        _cursorZ += length;
    }

    // A lane-wide platform of moving blocks, centred on the track and swinging out to each lane.
    // One collider on the root carries the MovingPlatform, so the pet finds it from whatever it stands on.
    private static void Slider(float topY)
    {
        float length = SLIDER_BLOCK_COUNT * TILE_SIZE;
        var sliderObject = new GameObject($"Slider_{_cursorZ:000}");
        sliderObject.transform.SetParent(_courseRoot);
        sliderObject.transform.position = new Vector3(0f, topY, _cursorZ);

        for (int index = 0; index < SLIDER_BLOCK_COUNT; index++)
        {
            var blockPosition = new Vector3(0f, topY - SLIDER_THICKNESS, _cursorZ + (index + 0.5f) * TILE_SIZE);
            GameObject block = PlaceKit(SLIDER_BLOCK, sliderObject.transform, blockPosition);
            block.transform.localScale = Vector3.one * SLIDER_BLOCK_SCALE;
            block.isStatic = false;
        }

        var collider = sliderObject.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, -SLIDER_THICKNESS * 0.5f, length * 0.5f);
        collider.size = new Vector3(TILE_SIZE, SLIDER_THICKNESS, length);
        sliderObject.AddComponent<Rigidbody>().isKinematic = true;

        var platform = sliderObject.AddComponent<MovingPlatform>();
        var serializedPlatform = new SerializedObject(platform);
        serializedPlatform.FindProperty("_travel").vector3Value = new Vector3(RIGHT_LANE, 0f, 0f);
        serializedPlatform.FindProperty("_period").floatValue = SLIDER_PERIOD;
        serializedPlatform.ApplyModifiedPropertiesWithoutUndo();

        _cursorZ += length;
    }

    // Fence line plus an invisible wall, so the player reads the edge and cannot leave the track.
    private static void BuildSideBorder(float side, float width, float topY, float length, bool isSnow)
    {
        float edgeX = side * width * 0.5f;
        float startZ = _cursorZ;

        var wall = new GameObject(side < 0f ? "WallLeft" : "WallRight");
        wall.transform.SetParent(_courseRoot);
        float wallInnerX = edgeX - side * SIDE_WALL_INSET;
        wall.transform.position = new Vector3(wallInnerX + side * SIDE_WALL_THICKNESS * 0.5f, topY + SIDE_WALL_HEIGHT * 0.5f, startZ + length * 0.5f);
        var wallCollider = wall.AddComponent<BoxCollider>();
        wallCollider.size = new Vector3(SIDE_WALL_THICKNESS, SIDE_WALL_HEIGHT, length);

        float fenceYaw = side > 0f ? 90f : -90f;
        float fenceX = edgeX - side * (FENCE_EDGE_INSET + FENCE_MODEL_OFFSET * KIT_SCALE);
        for (float z = startZ + FENCE_LENGTH * 0.5f; z < startZ + length; z += FENCE_LENGTH)
        {
            GameObject fence = PlaceKit(FENCE, _decorRoot, new Vector3(fenceX, topY, z));
            fence.transform.rotation = Quaternion.Euler(0f, fenceYaw, 0f);
        }

        string[] pool = isSnow ? _snowDecor : _grassDecor;
        float decorX = edgeX - side * BORDER_DECOR_INSET;
        for (float z = startZ + RandomRange(1f, 4f); z < startZ + length - 1f; z += RandomRange(4f, 7f))
        {
            GameObject decor = PlaceKit(Pick(pool), _decorRoot, new Vector3(decorX, topY, z));
            decor.transform.localScale = Vector3.one * BORDER_DECOR_SCALE;
            decor.transform.rotation = Quaternion.Euler(0f, RandomRange(0f, 360f), 0f);
        }
    }

    // A row of spike blocks that fills one lane, leaving the other lane open.
    private static void SpikeLane(float side, float topY, float z)
    {
        for (int index = 0; index < SPIKES_PER_LANE; index++)
        {
            float blockX = side * (SPIKE_LANE_START_X + index * SPIKE_BLOCK_SPACING);
            PlaceSpikes($"Spikes_{z:000}_{index}", new Vector3(blockX, topY, z));
        }
    }

    private static void PlaceSpikes(string name, Vector3 position)
    {
        GameObject spikes = PlaceKit(SPIKES, _logicRoot, position);
        spikes.name = name;
        spikes.transform.localScale = Vector3.one * SPIKE_BLOCK_SCALE;

        BoxCollider trigger = AddBoxCollider(spikes, true);
        trigger.size *= HAZARD_TRIGGER_INSET;
        spikes.AddComponent<Hazard>();
        _cullTargets.Add(spikes.transform);
    }

    private static void Checkpoint(float z, float topY)
    {
        GameObject checkpointObject = CreateTrigger($"Checkpoint_{z:000}", new Vector3(0f, topY, z));
        Transform respawnPoint = CreateMarker("RespawnPoint", checkpointObject.transform, Vector3.up * SPAWN_HEIGHT);

        var checkpoint = checkpointObject.AddComponent<Checkpoint>();
        var serializedCheckpoint = new SerializedObject(checkpoint);
        serializedCheckpoint.FindProperty("_respawnPoint").objectReferenceValue = respawnPoint;
        serializedCheckpoint.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Finish(float z, float topY)
    {
        GameObject finish = CreateTrigger("FinishLine", new Vector3(0f, topY, z));
        finish.AddComponent<FinishLine>();
        _finishPoint = finish.transform;

        foreach (float side in _bothLanes)
        {
            GameObject flag = PlaceKit(FLAG, finish.transform, new Vector3(side * 1.9f, topY, z));
            flag.transform.localScale = new Vector3(side * FLAG_SCALE, FLAG_SCALE, FLAG_SCALE);
        }
    }

    // ----- Scenery -----

    private static void BuildSideIslands(float endZ)
    {
        for (float z = -10f; z < endZ + 30f; z += SIDE_ISLAND_SPACING)
        {
            foreach (float side in _bothLanes)
            {
                float nearZ = z + RandomRange(-3f, 3f);
                BuildIsland(new Vector3(Mathf.Sign(side) * (SIDE_ISLAND_NEAR_X + RandomRange(0f, 3f)), RandomRange(-5f, -2f), nearZ), 1);

                if (_random.NextDouble() < 0.6)
                {
                    float farZ = z + RandomRange(-6f, 6f);
                    BuildIsland(new Vector3(Mathf.Sign(side) * (SIDE_ISLAND_FAR_X + RandomRange(0f, 8f)), RandomRange(-9f, 1f), farZ), 2);
                }
            }
        }
    }

    private static void BuildIsland(Vector3 topCenter, int size)
    {
        bool isSnow = IsSnowZone(topCenter.z);
        string tileModel = isSnow ? SNOW_TILE : GRASS_TILE;
        string[] trees = isSnow ? _snowTrees : _grassTrees;
        float offset = (size - 1) * TILE_SIZE * 0.5f;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                var tilePosition = new Vector3(topCenter.x - offset + x * TILE_SIZE, topCenter.y - TILE_HEIGHT, topCenter.z - offset + z * TILE_SIZE);
                PlaceKit(tileModel, _decorRoot, tilePosition);

                var treePosition = new Vector3(tilePosition.x + RandomRange(-1f, 1f), topCenter.y, tilePosition.z + RandomRange(-1f, 1f));
                GameObject tree = PlaceKit(Pick(trees), _decorRoot, treePosition);
                tree.transform.rotation = Quaternion.Euler(0f, RandomRange(0f, 360f), 0f);
            }
        }
    }

    private static bool IsSnowZone(float z)
    {
        foreach (Vector3 zone in _snowZones)
        {
            if (z >= zone.x && z <= zone.z) return true;
        }

        return false;
    }

    // ----- Helpers -----

    private static GameObject PlaceKit(string modelName, Transform parent, Vector3 position)
    {
        var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>($"{KIT_MODELS_FOLDER}/{modelName}.fbx");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, parent);
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * KIT_SCALE;
        instance.isStatic = true;
        return instance;
    }

    private static BoxCollider AddBoxCollider(GameObject target, bool isTrigger)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer meshRenderer in renderers)
        {
            bounds.Encapsulate(meshRenderer.bounds);
        }

        var collider = target.AddComponent<BoxCollider>();
        collider.isTrigger = isTrigger;
        Vector3 scale = target.transform.localScale;
        Vector3 localCenter = bounds.center - target.transform.position;
        collider.center = new Vector3(localCenter.x / scale.x, localCenter.y / scale.y, localCenter.z / scale.z);
        collider.size = new Vector3(bounds.size.x / Mathf.Abs(scale.x), bounds.size.y / scale.y, bounds.size.z / scale.z);
        return collider;
    }

    private static GameObject CreateTrigger(string name, Vector3 position)
    {
        var triggerObject = new GameObject(name);
        triggerObject.transform.SetParent(_logicRoot);
        triggerObject.transform.position = position;

        var trigger = triggerObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = Vector3.up * TRIGGER_HEIGHT * 0.5f;
        trigger.size = new Vector3(COURSE_TRIGGER_WIDTH, TRIGGER_HEIGHT, TRIGGER_DEPTH);
        return triggerObject;
    }

    private static Transform CreateGroup(string name, Transform parent)
    {
        var group = new GameObject(name).transform;
        group.SetParent(parent, false);
        return group;
    }

    private static Transform CreateMarker(string name, Transform parent, Vector3 localPosition)
    {
        var marker = new GameObject(name).transform;
        marker.SetParent(parent, false);
        marker.localPosition = localPosition;
        return marker;
    }

    private static string Pick(string[] options)
    {
        return options[_random.Next(options.Length)];
    }

    private static float RandomRange(float min, float max)
    {
        return min + (float)_random.NextDouble() * (max - min);
    }
}
