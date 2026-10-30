using DG.Tweening;
using UnityEngine;

public class PetVisual : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    private const string IDLE_STATE = "idle";
    private const string RUN_STATE = "run";
    private const string DANCE_STATE = "dance";
    private const string GESTURE_STATE = "gesture-positive";
    private const float CROSSFADE_DURATION = 0.12f;
    private const float MOVING_SPEED_THRESHOLD = 0.5f;
    private const float GROUNDED_ANIMATION_SPEED = 1f;
    private const float FULL_TURN_DEGREES = 360f;

    // ========================= READONLY FIELDS ======================

    private readonly int _idleHash = Animator.StringToHash(IDLE_STATE);
    private readonly int _runHash = Animator.StringToHash(RUN_STATE);
    private readonly int _danceHash = Animator.StringToHash(DANCE_STATE);
    private readonly int _gestureHash = Animator.StringToHash(GESTURE_STATE);

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("References")]
    [Tooltip("Motor whose movement drives the visuals.")]
    [SerializeField] private PetMotor _motor;
    [Tooltip("Animator on the current pet model.")]
    [SerializeField] private Animator _animator;
    [Tooltip("Current pet model, replaced when the form changes.")]
    [SerializeField] private Transform _model;
    [Tooltip("Transform rotated to lean and turn with steering.")]
    [SerializeField] private Transform _tiltRoot;
    [Tooltip("Transform scaled for squash and stretch, pivoting at the feet.")]
    [SerializeField] private Transform _squashPivot;
    [Tooltip("Transform at the pet's middle that spins during a roll.")]
    [SerializeField] private Transform _rollPivot;
    [Tooltip("Particle burst played on jump and landing.")]
    [SerializeField] private ParticleSystem _burstDust;
    [Tooltip("Particle trail emitted while running on the ground.")]
    [SerializeField] private ParticleSystem _runDust;

    [Header("Lean")]
    [Tooltip("Maximum turn angle toward the steering direction, in degrees.")]
    [SerializeField] private float _maxYawAngle = 25f;
    [Tooltip("Maximum sideways roll while steering, in degrees.")]
    [SerializeField] private float _maxRollAngle = 8f;
    [Tooltip("How quickly the lean follows steering.")]
    [SerializeField] private float _leanSharpness = 12f;

    [Header("Squash and Stretch")]
    [Tooltip("Scale applied at take-off.")]
    [SerializeField] private Vector3 _jumpStretch = new Vector3(0.8f, 1.25f, 0.8f);
    [Tooltip("Scale applied on the hardest landing.")]
    [SerializeField] private Vector3 _landSquash = new Vector3(1.3f, 0.7f, 1.3f);
    [Tooltip("Fall speed that produces the full landing squash.")]
    [SerializeField] private float _fullSquashFallSpeed = 20f;
    [Tooltip("Seconds to spring back to normal scale.")]
    [SerializeField] private float _squashRecoverDuration = 0.3f;
    [Tooltip("Seconds the pet takes to shrink away when hit.")]
    [SerializeField] private float _hitShrinkDuration = 0.25f;

    [Header("Roll")]
    [Tooltip("Uniform scale of the pet while tucked into a roll.")]
    [SerializeField] private float _rollScale = 0.62f;
    [Tooltip("Full forward spins during one roll.")]
    [SerializeField] private int _rollTurns = 2;
    [Tooltip("Seconds to tuck into the roll.")]
    [SerializeField] private float _tuckDuration = 0.08f;

    [Header("Form Change")]
    [Tooltip("Dust particles in the form-change burst.")]
    [SerializeField] private int _formChangeDustCount = 24;
    [Tooltip("Scale the new model pops in from.")]
    [SerializeField] private float _formChangePopScale = 0.3f;
    [Tooltip("Seconds for the new model's quick spin.")]
    [SerializeField] private float _formChangeSpinDuration = 0.45f;

    [Header("Animation")]
    [Tooltip("Run animation speed while in the air.")]
    [SerializeField] private float _airAnimationSpeed = 0.35f;
    [Tooltip("Animation speed while gliding, so wings flap faster.")]
    [SerializeField] private float _glideAnimationSpeed = 2f;

    [Header("Dust")]
    [Tooltip("Particles emitted at take-off.")]
    [SerializeField] private int _jumpDustCount = 6;
    [Tooltip("Particles emitted on the hardest landing.")]
    [SerializeField] private int _landDustCount = 14;
    [Tooltip("Landings slower than this fall speed emit no dust.")]
    [SerializeField] private float _minDustFallSpeed = 3f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private int _currentStateHash;
    private bool _isAnimationLocked;

    // =========================== PROPERTIES ==========================

    public Transform SquashPivot => _squashPivot;

    // ======================= UNITY CALLBACKS =========================

    private void OnEnable()
    {
        _motor.OnJumped += HandleJumped;
        _motor.OnLanded += HandleLanded;
        _motor.OnRollStarted += HandleRollStarted;
        _motor.OnRollEnded += HandleRollEnded;
    }

    private void OnDisable()
    {
        _motor.OnJumped -= HandleJumped;
        _motor.OnLanded -= HandleLanded;
        _motor.OnRollStarted -= HandleRollStarted;
        _motor.OnRollEnded -= HandleRollEnded;
    }

    private void Update()
    {
        UpdateLean(Time.deltaTime);
        UpdateLocomotionAnimation();
        UpdateRunDust();
    }

    // ========================= PUBLIC METHODS ========================

    // Replaces the pet model; when animated, the change pops in with a dust burst and a quick spin.
    public void SetModel(GameObject modelPrefab, RuntimeAnimatorController animatorController, bool animate)
    {
        if (_model != null)
        {
            _model.DOKill();
            Destroy(_model.gameObject);
        }

        GameObject model = Instantiate(modelPrefab, _rollPivot);
        _model = model.transform;
        _model.localPosition = Vector3.down * _rollPivot.localPosition.y;
        _model.localRotation = Quaternion.identity;

        if (!model.TryGetComponent(out Animator animator))
        {
            animator = model.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = animatorController;
        animator.applyRootMotion = false;
        _animator = animator;
        _currentStateHash = 0;

        if (!animate) return;

        _burstDust.Emit(_formChangeDustCount);
        PunchScale(Vector3.one * _formChangePopScale);
        _model.DOLocalRotate(new Vector3(0f, FULL_TURN_DEGREES, 0f), _formChangeSpinDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic);
    }

    public void PlayDance()
    {
        _isAnimationLocked = true;
        PlayState(_danceHash);
    }

    public void PlayGesture()
    {
        PlayState(_gestureHash);
    }

    public void PlayHit()
    {
        _squashPivot.DOKill();
        _squashPivot.DOScale(Vector3.zero, _hitShrinkDuration).SetEase(Ease.InBack);
        _burstDust.Emit(_landDustCount);
    }

    public void PlayRespawn()
    {
        ResetVisual();
        PunchScale(Vector3.zero);
        _burstDust.Emit(_jumpDustCount);
    }

    public void PunchScale(Vector3 scale)
    {
        _squashPivot.DOKill();
        _squashPivot.localScale = scale;
        _squashPivot.DOScale(Vector3.one, _squashRecoverDuration).SetEase(Ease.OutBack);
    }

    public void ResetVisual()
    {
        _isAnimationLocked = false;
        _squashPivot.DOKill();
        _squashPivot.localScale = Vector3.one;
        _tiltRoot.localRotation = Quaternion.identity;
        _rollPivot.DOKill();
        _rollPivot.localRotation = Quaternion.identity;
        _currentStateHash = 0;
    }

    // ========================= PRIVATE METHODS =======================

    private void HandleJumped()
    {
        PunchScale(_jumpStretch);
        _burstDust.Emit(_jumpDustCount);
    }

    private void HandleLanded(float fallSpeed)
    {
        if (_motor.IsRolling) return;

        float impact = Mathf.InverseLerp(0f, _fullSquashFallSpeed, fallSpeed);
        PunchScale(Vector3.Lerp(Vector3.one, _landSquash, impact));

        if (fallSpeed < _minDustFallSpeed) return;

        _burstDust.Emit(Mathf.CeilToInt(_landDustCount * impact));
    }

    private void HandleRollStarted()
    {
        _squashPivot.DOKill();
        _squashPivot.DOScale(Vector3.one * _rollScale, _tuckDuration);

        _rollPivot.DOKill();
        _rollPivot.localRotation = Quaternion.identity;
        var spin = new Vector3(FULL_TURN_DEGREES * _rollTurns, 0f, 0f);
        _rollPivot.DOLocalRotate(spin, _motor.Stats.RollDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad);
        _burstDust.Emit(_jumpDustCount);
    }

    private void HandleRollEnded()
    {
        _rollPivot.DOKill();
        _rollPivot.localRotation = Quaternion.identity;
        PunchScale(Vector3.one * _rollScale);
    }

    private void UpdateLean(float deltaTime)
    {
        float steer = Mathf.Clamp(_motor.Velocity.x / _motor.Stats.SteerSpeed, -1f, 1f);
        Quaternion target = Quaternion.Euler(0f, steer * _maxYawAngle, -steer * _maxRollAngle);
        float blend = 1f - Mathf.Exp(-_leanSharpness * deltaTime);
        _tiltRoot.localRotation = Quaternion.Slerp(_tiltRoot.localRotation, target, blend);
    }

    private void UpdateLocomotionAnimation()
    {
        if (_isAnimationLocked) return;

        bool isMoving = _motor.Velocity.z > MOVING_SPEED_THRESHOLD;
        bool isAirborne = !_motor.IsGrounded;
        PlayState(isMoving || isAirborne ? _runHash : _idleHash);
        float airSpeed = _motor.IsGliding ? _glideAnimationSpeed : _airAnimationSpeed;
        _animator.speed = isAirborne ? airSpeed : GROUNDED_ANIMATION_SPEED;
    }

    private void UpdateRunDust()
    {
        ParticleSystem.EmissionModule emission = _runDust.emission;
        emission.enabled = _motor.IsGrounded && _motor.Velocity.z > MOVING_SPEED_THRESHOLD;
    }

    private void PlayState(int stateHash)
    {
        if (_currentStateHash == stateHash) return;

        _currentStateHash = stateHash;
        _animator.speed = GROUNDED_ANIMATION_SPEED;
        _animator.CrossFadeInFixedTime(stateHash, CROSSFADE_DURATION);
    }
}
