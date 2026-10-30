using UnityEngine;

public static class PetDustFactory
{
    // =========================== CONSTANTS ===========================

    private const string BURST_DUST_NAME = "BurstDust";
    private const string RUN_DUST_NAME = "RunDust";
    private const string CUBE_MESH = "Cube.fbx";
    private const float FLAT_ON_GROUND_ANGLE = -90f;

    private static readonly Color _dustColor = new Color(0.95f, 0.92f, 0.86f, 1f);
    private static readonly Vector3 _runDustOffset = new Vector3(0f, 0.05f, -0.4f);

    // ========================= STATIC METHODS ========================

    public static ParticleSystem CreateBurstDust(Transform parent, Material material)
    {
        ParticleSystem particles = CreateBase(BURST_DUST_NAME, parent, Vector3.zero, material);

        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.45f;
        shape.rotation = new Vector3(FLAT_ON_GROUND_ANGLE, 0f, 0f);

        return particles;
    }

    public static ParticleSystem CreateRunDust(Transform parent, Material material)
    {
        ParticleSystem particles = CreateBase(RUN_DUST_NAME, parent, _runDustOffset, material);

        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 2.5f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.25f;

        return particles;
    }

    // A world-space system of small tumbling cubes that shrink away; callers set looping, speed, size and shape.
    public static ParticleSystem CreateBase(string name, Transform parent, Vector3 localPosition, Material material)
    {
        var dustObject = new GameObject(name);
        dustObject.transform.SetParent(parent, false);
        dustObject.transform.localPosition = localPosition;

        var particles = dustObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = _dustColor;
        main.gravityModifier = -0.05f;
        main.startRotation3D = true;
        main.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI);
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI);
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI);
        main.maxParticles = 64;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var particleRenderer = dustObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
        particleRenderer.mesh = Resources.GetBuiltinResource<Mesh>(CUBE_MESH);
        particleRenderer.sharedMaterial = material;

        return particles;
    }
}
