using UnityEngine;

// Builds the particle systems CourseEffects plays. They share the pet dust's cube look, so the
// effects match the blocky art; each only emits when told to.
public static class CourseEffectsFactory
{
    // =========================== CONSTANTS ===========================

    private const string COIN_SPARKLE_NAME = "CoinSparkle";
    private const string CHECKPOINT_BURST_NAME = "CheckpointBurst";
    private const string CONFETTI_NAME = "Confetti";
    private const float POINT_UP_ANGLE = -90f;

    // ========================= READONLY FIELDS ======================

    private static readonly Color _coinGold = new Color(1f, 0.82f, 0.25f);
    private static readonly Color _coinLight = new Color(1f, 0.97f, 0.7f);
    private static readonly Color _checkpointGreen = new Color(0.35f, 0.9f, 0.55f);
    private static readonly Color _checkpointWhite = new Color(0.95f, 1f, 0.95f);
    private static readonly Color[] _confettiColors =
    {
        new Color(1f, 0.35f, 0.4f),
        new Color(1f, 0.8f, 0.2f),
        new Color(0.3f, 0.75f, 1f),
        new Color(0.4f, 0.9f, 0.45f),
        new Color(0.85f, 0.5f, 1f),
    };
    // In the box's own axes, which are turned to fire upward: as wide as the course, a little deep, and flat.
    private static readonly Vector3 _checkpointLineSize = new Vector3(8f, 0.5f, 0.1f);

    // ========================= STATIC METHODS ========================

    public static ParticleSystem CreateCoinSparkle(Transform parent, Material material)
    {
        ParticleSystem particles = PetDustFactory.CreateBase(COIN_SPARKLE_NAME, parent, Vector3.zero, material);
        SetOneShot(particles, 32);

        ParticleSystem.MainModule main = particles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(_coinGold, _coinLight);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.28f);
        main.gravityModifier = 0.4f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        return particles;
    }

    public static ParticleSystem CreateCheckpointBurst(Transform parent, Material material)
    {
        ParticleSystem particles = PetDustFactory.CreateBase(CHECKPOINT_BURST_NAME, parent, Vector3.zero, material);
        SetOneShot(particles, 64);

        ParticleSystem.MainModule main = particles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(_checkpointGreen, _checkpointWhite);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.gravityModifier = 1f;

        // A thin box across the track; a box fires along its own Z axis, so it is turned to point up.
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = _checkpointLineSize;
        shape.rotation = new Vector3(POINT_UP_ANGLE, 0f, 0f);
        shape.randomDirectionAmount = 0.15f;
        return particles;
    }

    public static ParticleSystem CreateConfetti(Transform parent, Material material)
    {
        ParticleSystem particles = PetDustFactory.CreateBase(CONFETTI_NAME, parent, Vector3.zero, material);
        SetOneShot(particles, 160);

        ParticleSystem.MainModule main = particles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(CreateConfettiGradient()) { mode = ParticleSystemGradientMode.RandomColor };
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(7f, 11f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
        main.gravityModifier = 1.2f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.5f;
        shape.rotation = new Vector3(POINT_UP_ANGLE, 0f, 0f);

        // Tumbling, and slowed by the air, so the confetti flutters down instead of dropping like stones.
        ParticleSystem.RotationOverLifetimeModule rotation = particles.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-6f, 6f);
        ParticleSystem.LimitVelocityOverLifetimeModule drag = particles.limitVelocityOverLifetime;
        drag.enabled = true;
        drag.drag = 1.5f;
        return particles;
    }

    // Bursts only when CourseEffects emits, never on its own.
    private static void SetOneShot(ParticleSystem particles, int maxParticles)
    {
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = maxParticles;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
    }

    // A fixed-mode gradient holds each colour until its key, so each cube picks one clean colour rather than a blend.
    private static Gradient CreateConfettiGradient()
    {
        var colorKeys = new GradientColorKey[_confettiColors.Length];
        float step = 1f / _confettiColors.Length;
        for (int index = 0; index < _confettiColors.Length; index++)
        {
            colorKeys[index] = new GradientColorKey(_confettiColors[index], (index + 1) * step);
        }

        var gradient = new Gradient { mode = GradientMode.Fixed };
        gradient.SetKeys(colorKeys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }
}
