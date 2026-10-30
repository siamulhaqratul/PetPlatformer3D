using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Tooltip("Where the pet respawns after reaching this checkpoint.")]
    [SerializeField] private Transform _respawnPoint;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private bool _isReached;

    // ======================= UNITY CALLBACKS =========================

    private void OnEnable()
    {
        GameEvents.OnRunReset += HandleRunReset;
    }

    private void OnDisable()
    {
        GameEvents.OnRunReset -= HandleRunReset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isReached || !other.TryGetComponent(out PetMotor _)) return;

        _isReached = true;
        GameEvents.RaiseCheckpointReached(_respawnPoint.position);
    }

    // ========================= PRIVATE METHODS =======================

    private void HandleRunReset()
    {
        _isReached = false;
    }
}
