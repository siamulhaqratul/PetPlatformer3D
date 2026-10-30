using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PetMotor : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    private const float GROUND_STICK_VELOCITY = -2f;
    private const float STEER_INPUT_DEADZONE = 0.01f;
    private const float JUMP_VELOCITY_FACTOR = 2f;
    private const float FRONT_WALL_NORMAL_Z = -0.6f;
    private const float GROUND_NORMAL_Y = 0.7f;
    private const float LEDGE_PROBE_DISTANCE = 0.35f;
    private const float LEDGE_CLEARANCE = 0.25f;

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("References")]
    [Tooltip("Movement values for the current animal form.")]
    [SerializeField] private PetMovementStats _stats;
    [Tooltip("Input source that drives steering and jumping.")]
    [SerializeField] private PetInput _input;
    [Tooltip("Layers the ledge assist treats as climbable ground.")]
    [SerializeField] private LayerMask _groundLayers = ~0;

    [Header("Debug")]
    [Tooltip("Start running on play, for testing without the game manager.")]
    [SerializeField] private bool _runOnStart = true;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private CharacterController _controller;
    private Transform _transform;
    private MovingPlatform _groundPlatform;
    private Vector3 _velocity;
    private Vector3 _standingCenter;
    private float _standingHeight;
    private float _standingRadius;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _blockedTimer;
    private float _rollTimer;
    private float _laneTargetX;
    private int _runStartFrame = -1;
    private bool _hasLaneTarget;
    private bool _isPaused;
    private bool _isRollQueued;
    private bool _isRunning;
    private bool _isJumpRising;
    private bool _isGrounded;
    private bool _isFrozen;
    private bool _isFrontBlocked;
    private bool _isGliding;

    // ========================= INITIALIZE METHODS ========================

    private void InitializeMotor()
    {
        _controller = GetComponent<CharacterController>();
        _transform = transform;
        _isRunning = _runOnStart;
        _standingCenter = _controller.center;
        _standingHeight = _controller.height;
        _standingRadius = _controller.radius;
    }

    // =========================== PROPERTIES ==========================

    public bool IsGrounded => _isGrounded;
    public bool IsRunning => _isRunning;
    public bool IsFrozen => _isFrozen;
    public bool IsRolling => _rollTimer > 0f;
    public bool IsGliding => _isGliding;
    public Vector3 Velocity => _velocity;
    public PetMovementStats Stats => _stats;

    // Input is ignored while paused, so nothing pressed on the pause menu carries into the run, and on the frame
    // a run starts, because that press is the one that started the run (Space or swipe up on the start screen).
    private bool CanTakeInput => _isRunning && !_isPaused && Time.frameCount != _runStartFrame;

    // ============================ EVENTS =============================

    public event Action OnJumped;
    public event Action<float> OnLanded;
    public event Action OnBonked;
    public event Action OnRollStarted;
    public event Action OnRollEnded;

    // ======================= UNITY CALLBACKS =========================

    // Awake is needed because the game manager teleports the pet from its own Start.
    private void Awake()
    {
        InitializeMotor();
    }

    private void OnEnable()
    {
        _input.OnJumpPressed += HandleJumpPressed;
        _input.OnJumpReleased += HandleJumpReleased;
        _input.OnDuckPressed += HandleDuckPressed;
        _input.OnSteerSwipe += HandleSteerSwipe;
    }

    private void OnDisable()
    {
        _input.OnJumpPressed -= HandleJumpPressed;
        _input.OnJumpReleased -= HandleJumpReleased;
        _input.OnDuckPressed -= HandleDuckPressed;
        _input.OnSteerSwipe -= HandleSteerSwipe;
    }

    // CharacterController moves in Update so the camera follows without physics-step jitter.
    private void Update()
    {
        if (_controller == null || _isFrozen) return;

        float deltaTime = Time.deltaTime;
        UpdateTimers(deltaTime);
        UpdateRoll(deltaTime);
        UpdateForwardVelocity(deltaTime);
        UpdateSteerVelocity(deltaTime);
        TryJump();
        ApplyGravity(deltaTime);
        RideGroundPlatform();
        Move(deltaTime);
        UpdateFrontWall(deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        bool isGroundHit = hit.normal.y > GROUND_NORMAL_Y;
        if (isGroundHit)
        {
            hit.collider.TryGetComponent(out _groundPlatform);
            return;
        }

        if (hit.normal.z > FRONT_WALL_NORMAL_Z) return;

        _isFrontBlocked = true;
    }

    // ========================= PUBLIC METHODS ========================

    public void SetRunning(bool isRunning)
    {
        if (isRunning && !_isRunning)
        {
            _runStartFrame = Time.frameCount;
        }

        _isRunning = isRunning;
    }

    public void SetPaused(bool isPaused)
    {
        _isPaused = isPaused;
    }

    public void SetFrozen(bool isFrozen)
    {
        _isFrozen = isFrozen;
        ResetMotion();
    }

    public void SetStats(PetMovementStats stats)
    {
        _stats = stats;
    }

    public void TeleportTo(Vector3 position)
    {
        _controller.enabled = false;
        _transform.position = position;
        _controller.enabled = true;
        ResetMotion();
    }

    public void ResetMotion()
    {
        _velocity = Vector3.zero;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
        _isJumpRising = false;
        _hasLaneTarget = false;
        _isRollQueued = false;
        _isGliding = false;
        _groundPlatform = null;

        if (IsRolling)
        {
            EndRoll();
        }
    }

    // ========================= PRIVATE METHODS =======================

    private void HandleJumpPressed()
    {
        if (!CanTakeInput) return;

        _jumpBufferTimer = _stats.JumpBufferTime;
    }

    private void HandleJumpReleased()
    {
        bool isRisingFromJump = _isJumpRising && _velocity.y > 0f;
        if (!isRisingFromJump || _isPaused) return;

        _velocity.y *= _stats.JumpCutMultiplier;
        _isJumpRising = false;
    }

    // On the ground the pet rolls; in the air it slams down and rolls on landing.
    private void HandleDuckPressed()
    {
        if (!CanTakeInput) return;

        if (_isGrounded)
        {
            StartRoll();
            return;
        }

        _velocity.y = -_stats.SlamSpeed;
        _isJumpRising = false;
        _isRollQueued = true;
    }

    private void HandleSteerSwipe(int direction)
    {
        if (!CanTakeInput) return;

        float currentLane = _hasLaneTarget ? _laneTargetX : NearestLane(_transform.position.x);
        _laneTargetX = Mathf.Clamp(currentLane + direction * _stats.LaneSpacing, -_stats.MaxLaneOffset, _stats.MaxLaneOffset);
        _hasLaneTarget = true;
    }

    private float NearestLane(float x)
    {
        float lane = Mathf.Round(x / _stats.LaneSpacing) * _stats.LaneSpacing;
        return Mathf.Clamp(lane, -_stats.MaxLaneOffset, _stats.MaxLaneOffset);
    }

    private void UpdateTimers(float deltaTime)
    {
        _coyoteTimer = _isGrounded ? _stats.CoyoteTime : _coyoteTimer - deltaTime;
        _jumpBufferTimer -= deltaTime;
    }

    private void UpdateRoll(float deltaTime)
    {
        if (!IsRolling) return;

        _rollTimer -= deltaTime;
        if (_rollTimer > 0f) return;

        EndRoll();
    }

    private void StartRoll()
    {
        _rollTimer = _stats.RollDuration;
        SetControllerShape(_stats.RollHeight, _stats.RollRadius);
        OnRollStarted?.Invoke();
    }

    private void EndRoll()
    {
        _rollTimer = 0f;
        SetControllerShape(_standingHeight, _standingRadius);
        OnRollEnded?.Invoke();
    }

    // Keeps the feet in place while the collider shrinks or grows.
    private void SetControllerShape(float height, float radius)
    {
        _controller.height = height;
        _controller.radius = radius;
        float centerY = _standingCenter.y - (_standingHeight - height) * 0.5f;
        _controller.center = new Vector3(_standingCenter.x, centerY, _standingCenter.z);
    }

    private void UpdateForwardVelocity(float deltaTime)
    {
        float targetSpeed = _isRunning ? _stats.ForwardSpeed : 0f;
        if (_isGliding)
        {
            targetSpeed *= _stats.GlideForwardMultiplier;
        }

        _velocity.z = Mathf.MoveTowards(_velocity.z, targetSpeed, _stats.ForwardAcceleration * deltaTime);
    }

    private void UpdateSteerVelocity(float deltaTime)
    {
        float steerInput = ReadSteerInput();
        float targetSpeed = steerInput * _stats.SteerSpeed;
        bool isSteering = Mathf.Abs(steerInput) > STEER_INPUT_DEADZONE;
        float rate = isSteering ? _stats.SteerAcceleration : _stats.SteerDeceleration;

        if (!_isGrounded)
        {
            rate *= _stats.AirControl;
        }

        _velocity.x = Mathf.MoveTowards(_velocity.x, targetSpeed, rate * deltaTime);
    }

    // Held keys or sticks steer directly; otherwise a swipe's lane target is steered to and held.
    private float ReadSteerInput()
    {
        if (!_isRunning) return 0f;

        float manualSteer = _input.Steer;
        if (Mathf.Abs(manualSteer) > STEER_INPUT_DEADZONE)
        {
            _hasLaneTarget = false;
            return manualSteer;
        }

        if (!_hasLaneTarget) return 0f;

        float offset = _laneTargetX - _transform.position.x;
        return Mathf.Clamp(offset / _stats.LaneSteerSoftness, -1f, 1f);
    }

    private void TryJump()
    {
        bool canJump = _jumpBufferTimer > 0f && _coyoteTimer > 0f;
        if (!canJump) return;

        if (IsRolling)
        {
            EndRoll();
        }

        _velocity.y = Mathf.Sqrt(JUMP_VELOCITY_FACTOR * _stats.Gravity * _stats.JumpHeight);
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
        _isJumpRising = true;
        _isGrounded = false;
        OnJumped?.Invoke();
    }

    private void ApplyGravity(float deltaTime)
    {
        if (_isGrounded && _velocity.y <= 0f)
        {
            _velocity.y = GROUND_STICK_VELOCITY;
            _isGliding = false;
            return;
        }

        bool isFalling = _velocity.y < 0f;
        float gravity = isFalling ? _stats.Gravity * _stats.FallGravityMultiplier : _stats.Gravity;
        float maxFallSpeed = Mathf.Max(_stats.MaxFallSpeed, _stats.SlamSpeed);
        _velocity.y = Mathf.Max(_velocity.y - gravity * deltaTime, -maxFallSpeed);

        // Gliding caps the fall speed while jump is held; a slam (duck in the air) cancels it.
        _isGliding = _stats.CanGlide && isFalling && _isRunning && !_isRollQueued && _input.IsJumpHeld;
        if (_isGliding)
        {
            _velocity.y = Mathf.Max(_velocity.y, -_stats.GlideFallSpeed);
        }

        if (isFalling)
        {
            _isJumpRising = false;
        }
    }

    // Carries the pet with the moving platform it stands on. The lane it is steering to moves along too,
    // otherwise the lane steering would pull it off the side of the platform.
    private void RideGroundPlatform()
    {
        if (_groundPlatform == null || !_isGrounded) return;

        Vector3 delta = _groundPlatform.Delta;
        _controller.Move(delta);
        if (_hasLaneTarget)
        {
            _laneTargetX += delta.x;
        }
    }

    private void Move(float deltaTime)
    {
        float fallSpeed = -_velocity.y;
        _isFrontBlocked = false;
        // Set again by OnControllerColliderHit if the pet is still standing on a moving platform after this move.
        _groundPlatform = null;
        CollisionFlags flags = _controller.Move(_velocity * deltaTime);
        bool wasGrounded = _isGrounded;
        _isGrounded = _controller.isGrounded;

        bool hitCeiling = (flags & CollisionFlags.Above) != 0 && _velocity.y > 0f;
        if (hitCeiling)
        {
            _velocity.y = 0f;
        }

        if (!_isGrounded || wasGrounded) return;

        OnLanded?.Invoke(fallSpeed);

        if (!_isRollQueued) return;

        _isRollQueued = false;
        StartRoll();
    }

    private void UpdateFrontWall(float deltaTime)
    {
        bool isPushingIntoWall = _isFrontBlocked && _isRunning;
        if (!isPushingIntoWall || TryClimbLedge())
        {
            _blockedTimer = 0f;
            return;
        }

        // While still rising the pet slides up the wall face until the ledge assist can catch the top.
        bool isRising = _velocity.y > 0f;
        if (isRising)
        {
            _blockedTimer = 0f;
            return;
        }

        _blockedTimer += deltaTime;
        if (_blockedTimer < _stats.BonkTime) return;

        _blockedTimer = 0f;
        OnBonked?.Invoke();
    }

    // Gives just enough upward speed to clear a nearby ledge, so the pet glides over the lip instead of snapping up.
    private bool TryClimbLedge()
    {
        float probeHeight = _stats.LedgeAssistHeight + LEDGE_CLEARANCE;
        Vector3 feet = _transform.position;
        Vector3 probeOrigin = feet + Vector3.forward * (_controller.radius + LEDGE_PROBE_DISTANCE) + Vector3.up * probeHeight;

        bool foundLedge = Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit hit, probeHeight, _groundLayers, QueryTriggerInteraction.Ignore);
        if (!foundLedge) return false;

        float rise = hit.point.y - feet.y;
        bool isWithinReach = rise > 0f && rise <= _stats.LedgeAssistHeight;
        if (!isWithinReach) return false;

        float hopSpeed = Mathf.Sqrt(JUMP_VELOCITY_FACTOR * _stats.Gravity * (rise + LEDGE_CLEARANCE));
        _velocity.y = Mathf.Max(_velocity.y, hopSpeed);
        _isJumpRising = false;
        return true;
    }
}
