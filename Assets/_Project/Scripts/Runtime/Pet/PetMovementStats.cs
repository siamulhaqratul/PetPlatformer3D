using UnityEngine;

[CreateAssetMenu(fileName = "PetMovementStats", menuName = "Pet Shift Run/Pet Movement Stats")]
public class PetMovementStats : ScriptableObject
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Run")]
    [Tooltip("Forward speed the pet runs at automatically, in units per second.")]
    [SerializeField] private float _forwardSpeed = 8.5f;
    [Tooltip("How fast the pet reaches its forward speed, in units per second squared.")]
    [SerializeField] private float _forwardAcceleration = 20f;

    [Header("Steer")]
    [Tooltip("Maximum sideways speed, in units per second.")]
    [SerializeField] private float _steerSpeed = 7f;
    [Tooltip("How fast sideways speed builds up while steering.")]
    [SerializeField] private float _steerAcceleration = 45f;
    [Tooltip("How fast sideways speed drops when steering is released.")]
    [SerializeField] private float _steerDeceleration = 60f;
    [Tooltip("Multiplier on steer acceleration while in the air.")]
    [Range(0f, 1f)]
    [SerializeField] private float _airControl = 0.8f;

    [Header("Jump")]
    [Tooltip("Peak height of a full jump, in units.")]
    [SerializeField] private float _jumpHeight = 3.8f;
    [Tooltip("Gravity applied while rising, in units per second squared.")]
    [SerializeField] private float _gravity = 34f;
    [Tooltip("Gravity multiplier while falling, for a snappier landing.")]
    [SerializeField] private float _fallGravityMultiplier = 1.6f;
    [Tooltip("Upward speed is multiplied by this when jump is released early.")]
    [Range(0f, 1f)]
    [SerializeField] private float _jumpCutMultiplier = 0.9f;
    [Tooltip("Maximum falling speed, in units per second.")]
    [SerializeField] private float _maxFallSpeed = 28f;

    [Header("Forgiveness")]
    [Tooltip("Seconds after leaving a ledge during which a jump still counts.")]
    [SerializeField] private float _coyoteTime = 0.1f;
    [Tooltip("Seconds a jump press is remembered before landing.")]
    [SerializeField] private float _jumpBufferTime = 0.12f;

    [Header("Ledges")]
    [Tooltip("A ledge this far above the feet is climbed automatically when the pet runs into it.")]
    [SerializeField] private float _ledgeAssistHeight = 0.8f;
    [Tooltip("Seconds pressed against a wall before it counts as a bonk.")]
    [SerializeField] private float _bonkTime = 0.15f;

    [Header("Swipe Lanes")]
    [Tooltip("Sideways distance between lanes a swipe moves the pet.")]
    [SerializeField] private float _laneSpacing = 2f;
    [Tooltip("Furthest lane from the centre line, in units.")]
    [SerializeField] private float _maxLaneOffset = 2f;
    [Tooltip("Distance from the lane centre at which steering starts easing off.")]
    [SerializeField] private float _laneSteerSoftness = 0.6f;

    [Header("Roll")]
    [Tooltip("Seconds a duck-and-roll lasts.")]
    [SerializeField] private float _rollDuration = 0.6f;
    [Tooltip("Collider height while rolling.")]
    [SerializeField] private float _rollHeight = 0.9f;
    [Tooltip("Collider radius while rolling.")]
    [SerializeField] private float _rollRadius = 0.45f;
    [Tooltip("Downward speed when ducking in the air, in units per second.")]
    [SerializeField] private float _slamSpeed = 24f;

    [Header("Glide")]
    [Tooltip("Whether holding jump while falling slows the fall.")]
    [SerializeField] private bool _canGlide;
    [Tooltip("Fastest fall while gliding, in units per second.")]
    [SerializeField] private float _glideFallSpeed = 2.2f;
    [Tooltip("Forward speed multiplier while gliding.")]
    [Range(0f, 1f)]
    [SerializeField] private float _glideForwardMultiplier = 0.85f;

    // =========================== PROPERTIES ==========================

    public float LedgeAssistHeight => _ledgeAssistHeight;
    public float BonkTime => _bonkTime;
    public float LaneSpacing => _laneSpacing;
    public float MaxLaneOffset => _maxLaneOffset;
    public float LaneSteerSoftness => _laneSteerSoftness;
    public float RollDuration => _rollDuration;
    public float RollHeight => _rollHeight;
    public float RollRadius => _rollRadius;
    public float SlamSpeed => _slamSpeed;
    public bool CanGlide => _canGlide;
    public float GlideFallSpeed => _glideFallSpeed;
    public float GlideForwardMultiplier => _glideForwardMultiplier;

    public float ForwardSpeed => _forwardSpeed;
    public float ForwardAcceleration => _forwardAcceleration;
    public float SteerSpeed => _steerSpeed;
    public float SteerAcceleration => _steerAcceleration;
    public float SteerDeceleration => _steerDeceleration;
    public float AirControl => _airControl;
    public float JumpHeight => _jumpHeight;
    public float Gravity => _gravity;
    public float FallGravityMultiplier => _fallGravityMultiplier;
    public float JumpCutMultiplier => _jumpCutMultiplier;
    public float MaxFallSpeed => _maxFallSpeed;
    public float CoyoteTime => _coyoteTime;
    public float JumpBufferTime => _jumpBufferTime;
}
