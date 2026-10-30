using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Idle")]
    [Tooltip("Spin speed, in degrees per second.")]
    [SerializeField] private float _spinSpeed = 180f;
    [Tooltip("How far the coin bobs up and down.")]
    [SerializeField] private float _bobHeight = 0.15f;
    [Tooltip("Bob cycles per second, in radians.")]
    [SerializeField] private float _bobSpeed = 3f;

    [Header("Collect")]
    [Tooltip("Seconds the pickup animation lasts.")]
    [SerializeField] private float _collectDuration = 0.3f;
    [Tooltip("How far the coin flies up when collected.")]
    [SerializeField] private float _collectRise = 1.5f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private Transform _transform;
    private Collider _collider;
    private Vector3 _startPosition;
    private Vector3 _startScale;
    private float _bobPhase;
    private bool _isCollected;

    // ========================= INITIALIZE METHODS ========================

    private void InitializeCoin()
    {
        _transform = transform;
        _collider = GetComponent<Collider>();
        _startPosition = _transform.position;
        _startScale = _transform.localScale;
        _bobPhase = _startPosition.z;
    }

    // ======================= UNITY CALLBACKS =========================

    private void Start()
    {
        InitializeCoin();
    }

    private void OnEnable()
    {
        GameEvents.OnRunReset += HandleRunReset;
    }

    private void OnDisable()
    {
        GameEvents.OnRunReset -= HandleRunReset;
    }

    private void Update()
    {
        if (_isCollected) return;

        _transform.Rotate(0f, _spinSpeed * Time.deltaTime, 0f, Space.World);
        float bob = Mathf.Sin(Time.time * _bobSpeed + _bobPhase) * _bobHeight;
        _transform.position = _startPosition + Vector3.up * bob;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected || !other.TryGetComponent(out PetMotor _)) return;

        Collect();
    }

    // ========================= PRIVATE METHODS =======================

    private void Collect()
    {
        _isCollected = true;
        _collider.enabled = false;

        _transform.DOKill();
        _transform.DOMoveY(_transform.position.y + _collectRise, _collectDuration).SetEase(Ease.OutQuad);
        _transform.DOScale(Vector3.zero, _collectDuration).SetEase(Ease.InBack);
        GameEvents.RaiseCoinCollected(_transform.position);
    }

    private void HandleRunReset()
    {
        if (_transform == null) return;

        _transform.DOKill();
        _transform.position = _startPosition;
        _transform.localScale = _startScale;
        _collider.enabled = true;
        _isCollected = false;
    }
}
