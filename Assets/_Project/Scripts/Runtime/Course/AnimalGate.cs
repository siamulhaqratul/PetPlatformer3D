using DG.Tweening;
using UnityEngine;

// One door of a gate pair; a small model of its animal spins above it.
[RequireComponent(typeof(Collider))]
public class AnimalGate : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Gate")]
    [Tooltip("Pair this gate belongs to.")]
    [SerializeField] private AnimalGatePair _pair;
    [Tooltip("Form the pet takes when running through this gate.")]
    [SerializeField] private PetForm _form;

    [Header("Door")]
    [Tooltip("Arch model, which sinks into the ground once the pair is used.")]
    [SerializeField] private Transform _door;
    [Tooltip("Seconds before the arch starts sinking.")]
    [SerializeField] private float _sinkDelay = 0.15f;
    [Tooltip("Seconds the arch takes to sink.")]
    [SerializeField] private float _sinkDuration = 0.35f;

    [Header("Icon")]
    [Tooltip("Floating animal model above the door.")]
    [SerializeField] private Transform _icon;
    [Tooltip("Icon spin speed, in degrees per second.")]
    [SerializeField] private float _spinSpeed = 90f;
    [Tooltip("How far the icon bobs up and down.")]
    [SerializeField] private float _bobHeight = 0.2f;
    [Tooltip("Bob cycles per second, in radians.")]
    [SerializeField] private float _bobSpeed = 2.5f;
    [Tooltip("Seconds for the icon to pop away once the gate is used.")]
    [SerializeField] private float _popDuration = 0.35f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private Vector3 _iconStartPosition;
    private Vector3 _iconStartScale;
    private Vector3 _doorStartScale;
    private bool _isSpent;

    // ========================= INITIALIZE METHODS ========================

    private void InitializeGate()
    {
        _iconStartPosition = _icon.localPosition;
        _iconStartScale = _icon.localScale;
        _doorStartScale = _door.localScale;
    }

    // =========================== PROPERTIES ==========================

    public PetForm Form => _form;

    // ======================= UNITY CALLBACKS =========================

    // Awake is needed because the run reset can re-arm gates before their Start runs.
    private void Awake()
    {
        InitializeGate();
    }

    private void Update()
    {
        if (_isSpent) return;

        _icon.Rotate(0f, _spinSpeed * Time.deltaTime, 0f, Space.World);
        float bob = Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
        _icon.localPosition = _iconStartPosition + Vector3.up * bob;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isSpent || !other.TryGetComponent(out PetMotor pet)) return;

        _pair.ChooseFor(pet);
    }

    // ========================= PUBLIC METHODS ========================

    public void PlayChosen()
    {
        _isSpent = true;
        _icon.DOKill();
        Sequence pop = DOTween.Sequence();
        pop.Append(_icon.DOScale(_iconStartScale * 1.4f, _popDuration * 0.4f).SetEase(Ease.OutQuad));
        pop.Append(_icon.DOScale(Vector3.zero, _popDuration * 0.6f).SetEase(Ease.InBack));
        pop.SetTarget(_icon);
        SinkDoor();
    }

    public void PlayRejected()
    {
        _isSpent = true;
        _icon.DOKill();
        _icon.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack);
        SinkDoor();
    }

    public void ResetGate()
    {
        _isSpent = false;
        _icon.DOKill();
        _icon.localScale = _iconStartScale;
        _door.DOKill();
        _door.localScale = _doorStartScale;
    }

    // ========================= PRIVATE METHODS =======================

    // The arch squashes down into the ground (its pivot is at its base), clearing the camera's path.
    private void SinkDoor()
    {
        _door.DOKill();
        var flattened = new Vector3(_doorStartScale.x, 0f, _doorStartScale.z);
        _door.DOScale(flattened, _sinkDuration).SetDelay(_sinkDelay).SetEase(Ease.InBack);
    }
}
