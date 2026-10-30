using Cinemachine;
using UnityEngine;

// When the pet falls off the course, the follow camera stops dropping with it: it keeps following forward but rises
// above the ledge and tilts down, so the fall stays in view. Following resumes with a clean cut when the pet respawns.
public class CameraFallHold : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    // Ground up to this far above the pet's feet still counts as somewhere to land (the controller's step height).
    private const float STEP_UP_TOLERANCE = 0.3f;

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("References")]
    [Tooltip("Follow camera that peeks over the edge during a fall.")]
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [Tooltip("Motor of the pet the camera follows.")]
    [SerializeField] private PetMotor _motor;

    [Header("Fall Detection")]
    [Tooltip("How far below its last footing the pet must drop, with nothing under it, before it counts as falling off.")]
    [SerializeField] private float _fallDrop = 0.5f;
    [Tooltip("How far down to look for ground under the pet. Anything further counts as a fall.")]
    [SerializeField] private float _groundProbeDistance = 50f;
    [Tooltip("Seconds ahead along the pet's path to look for somewhere to land, so a drop onto a lower platform is not a fall.")]
    [SerializeField] private float _landingLookahead = 0.4f;
    [Tooltip("Layers that count as ground under the pet.")]
    [SerializeField] private LayerMask _groundLayers = ~0;

    [Header("Peek")]
    [Tooltip("How far behind the falling pet the camera follows. Short, so the camera reaches over the edge.")]
    [SerializeField] private float _peekDistance = 3.5f;
    [Tooltip("How high above the ledge the camera rises, high enough to see over the edge and down into the drop.")]
    [SerializeField] private float _peekHeight = 7f;
    [Tooltip("Seconds the camera takes to catch up with its peek position.")]
    [SerializeField] private float _peekSmoothTime = 0.15f;
    [Tooltip("Aim point relative to the pet while it falls (the normal aim looks ahead of it).")]
    [SerializeField] private Vector3 _fallAimOffset = new Vector3(0f, 0.6f, 0f);
    [Tooltip("Aim damping while the pet falls, low so the camera tilts down with it.")]
    [SerializeField] private float _fallAimDamping = 0.2f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private Transform _petTransform;
    private Transform _cameraTransform;
    private CinemachineComposer _composer;
    private Vector3 _normalAimOffset;
    private float _normalHorizontalDamping;
    private float _normalVerticalDamping;
    private Vector3 _peekVelocity;
    private float _lastGroundY;
    private bool _isHolding;

    // ========================= INITIALIZE METHODS ========================

    private void InitializeHold()
    {
        _petTransform = _motor.transform;
        _cameraTransform = _virtualCamera.transform;
        _composer = _virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        _normalAimOffset = _composer.m_TrackedObjectOffset;
        _normalHorizontalDamping = _composer.m_HorizontalDamping;
        _normalVerticalDamping = _composer.m_VerticalDamping;
        _lastGroundY = _petTransform.position.y;
    }

    // ======================= UNITY CALLBACKS =========================

    private void Start()
    {
        InitializeHold();
    }

    private void OnEnable()
    {
        _motor.OnLanded += HandleLanded;
        GameEvents.OnPetRespawned += HandlePetRespawned;
        GameEvents.OnRunReset += ResumeFollowing;
    }

    private void OnDisable()
    {
        _motor.OnLanded -= HandleLanded;
        GameEvents.OnPetRespawned -= HandlePetRespawned;
        GameEvents.OnRunReset -= ResumeFollowing;
    }

    // Runs in Update so the camera position is set before Cinemachine reads it in LateUpdate.
    private void Update()
    {
        if (_petTransform == null) return;

        if (_isHolding)
        {
            UpdatePeek();
            return;
        }

        if (_motor.IsGrounded)
        {
            _lastGroundY = _petTransform.position.y;
            return;
        }

        if (!HasFallenOff()) return;

        StartHold();
    }

    // ========================= PRIVATE METHODS =======================

    // A normal jump is back over solid ground by the time it drops below its take-off height, so only a real fall
    // is below the last footing with nothing to land on, either under the pet or a moment ahead on its path.
    private bool HasFallenOff()
    {
        Vector3 feet = _petTransform.position;
        bool isBelowLedge = feet.y < _lastGroundY - _fallDrop;
        if (!isBelowLedge || _motor.Velocity.y >= 0f) return false;

        Vector3 sidewaysVelocity = new Vector3(_motor.Velocity.x, 0f, _motor.Velocity.z);
        Vector3 aheadPoint = feet + sidewaysVelocity * _landingLookahead;
        return !HasLandingBelow(feet) && !HasLandingBelow(aheadPoint);
    }

    // The ray starts just above the pet's feet, so a platform top higher than that (a wall the pet will hit) is not
    // counted: a ray that starts inside a collider passes through it. The pet's own collider is skipped the same way.
    private bool HasLandingBelow(Vector3 point)
    {
        Vector3 probeOrigin = point + Vector3.up * STEP_UP_TOLERANCE;
        return Physics.Raycast(probeOrigin, Vector3.down, _groundProbeDistance, _groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void StartHold()
    {
        _isHolding = true;
        _virtualCamera.Follow = null;
        _composer.m_TrackedObjectOffset = _fallAimOffset;
        _composer.m_HorizontalDamping = _fallAimDamping;
        _composer.m_VerticalDamping = _fallAimDamping;
        _peekVelocity = Vector3.zero;
    }

    // Follows the pet forward and sideways, but at a fixed height above the ledge it fell from.
    private void UpdatePeek()
    {
        Vector3 petPosition = _petTransform.position;
        var peekPosition = new Vector3(petPosition.x, _lastGroundY + _peekHeight, petPosition.z - _peekDistance);
        _cameraTransform.position = Vector3.SmoothDamp(_cameraTransform.position, peekPosition, ref _peekVelocity, _peekSmoothTime);
    }

    // Puts the follow and aim back to normal; the caller decides whether the camera cuts or eases back.
    private void StopHold()
    {
        _isHolding = false;
        _virtualCamera.Follow = _petTransform;
        _composer.m_TrackedObjectOffset = _normalAimOffset;
        _composer.m_HorizontalDamping = _normalHorizontalDamping;
        _composer.m_VerticalDamping = _normalVerticalDamping;
    }

    // A pet that survives its drop is followed again, smoothly rather than with a cut.
    private void HandleLanded(float fallSpeed)
    {
        if (!_isHolding) return;

        StopHold();
    }

    private void HandlePetRespawned(float respawnZ)
    {
        ResumeFollowing();
    }

    // The pet has already been moved, so the camera cuts straight to its normal framing instead of swooping back.
    private void ResumeFollowing()
    {
        if (_petTransform == null) return;

        _lastGroundY = _petTransform.position.y;
        if (!_isHolding) return;

        StopHold();
        _virtualCamera.PreviousStateIsValid = false;
    }
}
