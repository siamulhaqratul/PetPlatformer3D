using Cinemachine;
using UnityEngine;

// Visual feedback for course events: a sparkle where a coin was picked up, a burst across the track at a
// checkpoint, a camera shake when a life is lost, and confetti at the finish.
// Each effect is one world-space particle system that emits wherever it is needed, so nothing is spawned.
public class CourseEffects : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("References")]
    [Tooltip("Game manager whose life and state changes trigger effects.")]
    [SerializeField] private GameManager _gameManager;
    [Tooltip("The pet, where the finish confetti bursts from.")]
    [SerializeField] private Transform _pet;

    [Header("Particles")]
    [Tooltip("Gold cubes that burst from a collected coin.")]
    [SerializeField] private ParticleSystem _coinSparkle;
    [Tooltip("Cubes that shoot up across the track at a checkpoint.")]
    [SerializeField] private ParticleSystem _checkpointBurst;
    [Tooltip("Coloured cubes thrown up around the pet at the finish.")]
    [SerializeField] private ParticleSystem _confetti;

    [Header("Counts")]
    [Tooltip("Cubes per coin sparkle.")]
    [SerializeField] private int _coinSparkleCount = 16;
    [Tooltip("Cubes per checkpoint burst.")]
    [SerializeField] private int _checkpointBurstCount = 36;
    [Tooltip("Cubes of finish confetti.")]
    [SerializeField] private int _confettiCount = 90;
    [Tooltip("Height above the pet the confetti bursts from.")]
    [SerializeField] private float _confettiHeight = 1f;

    [Header("Camera Shake")]
    [Tooltip("Sends the shake to the follow camera's impulse listener.")]
    [SerializeField] private CinemachineImpulseSource _shakeSource;
    [Tooltip("Direction and strength of the shake when a life is lost.")]
    [SerializeField] private Vector3 _lifeLostShake = new Vector3(0.2f, -0.35f, 0f);

    // ======================= UNITY CALLBACKS =========================

    private void OnEnable()
    {
        GameEvents.OnCoinCollected += HandleCoinCollected;
        GameEvents.OnCheckpointReached += HandleCheckpointReached;
        GameEvents.OnRunReset += ClearAll;
        _gameManager.OnLifeLost += HandleLifeLost;
        _gameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnCoinCollected -= HandleCoinCollected;
        GameEvents.OnCheckpointReached -= HandleCheckpointReached;
        GameEvents.OnRunReset -= ClearAll;
        _gameManager.OnLifeLost -= HandleLifeLost;
        _gameManager.OnStateChanged -= HandleStateChanged;
    }

    // ========================= PRIVATE METHODS =======================

    private void HandleCoinCollected(Vector3 position)
    {
        EmitAt(_coinSparkle, position, _coinSparkleCount);
    }

    private void HandleCheckpointReached(Vector3 respawnPosition)
    {
        EmitAt(_checkpointBurst, respawnPosition, _checkpointBurstCount);
    }

    private void HandleLifeLost()
    {
        _shakeSource.GenerateImpulseWithVelocity(_lifeLostShake);
    }

    private void HandleStateChanged(GameState state)
    {
        if (state != GameState.Won) return;

        EmitAt(_confetti, _pet.position + Vector3.up * _confettiHeight, _confettiCount);
    }

    // The system's own shape still spreads the particles; this only moves the shape's centre.
    private void EmitAt(ParticleSystem particles, Vector3 position, int count)
    {
        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            applyShapeToPosition = true,
        };
        particles.Emit(emitParams, count);
    }

    // A restart clears effects left over from the last run, such as confetti still falling.
    private void ClearAll()
    {
        _coinSparkle.Clear();
        _checkpointBurst.Clear();
        _confetti.Clear();
    }
}
