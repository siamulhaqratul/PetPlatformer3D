using UnityEngine;

// Slides back and forth along a path and reports how far it moved this frame, so the pet can ride it.
// It runs before other scripts, so a riding pet is carried by this frame's movement, not last frame's.
[DefaultExecutionOrder(-100)]
public class MovingPlatform : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Path")]
    [Tooltip("Offset from the start position to each end of the path; the platform swings this far either way.")]
    [SerializeField] private Vector3 _travel = new Vector3(2f, 0f, 0f);
    [Tooltip("Seconds for a full trip there and back.")]
    [SerializeField] private float _period = 3.2f;
    [Tooltip("Where in its trip the platform starts, from 0 to 1.")]
    [Range(0f, 1f)]
    [SerializeField] private float _startPhase;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private Transform _transform;
    private Vector3 _startPosition;
    private Vector3 _delta;
    private float _time;

    // ========================= INITIALIZE METHODS ========================

    private void InitializePlatform()
    {
        _transform = transform;
        _startPosition = _transform.position;
    }

    // =========================== PROPERTIES ==========================

    public Vector3 Delta => _delta;

    // ======================= UNITY CALLBACKS =========================

    // Awake is needed because the run reset can move the platform back before its Start runs.
    private void Awake()
    {
        InitializePlatform();
    }

    private void OnEnable()
    {
        GameEvents.OnRunReset += ResetPlatform;
    }

    private void OnDisable()
    {
        GameEvents.OnRunReset -= ResetPlatform;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        Vector3 nextPosition = PositionAt(_time);
        _delta = nextPosition - _transform.position;
        _transform.position = nextPosition;
    }

    // ========================= PRIVATE METHODS =======================

    // Every run starts with the platform in the same place, so a run plays the same way each time.
    private void ResetPlatform()
    {
        _time = 0f;
        _delta = Vector3.zero;
        _transform.position = PositionAt(_time);
    }

    private Vector3 PositionAt(float time)
    {
        float cycle = (time / _period + _startPhase) * Mathf.PI * 2f;
        return _startPosition + _travel * Mathf.Sin(cycle);
    }
}
