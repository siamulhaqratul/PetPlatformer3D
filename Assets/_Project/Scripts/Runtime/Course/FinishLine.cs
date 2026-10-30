using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinishLine : MonoBehaviour
{
    // ======================= UNITY CALLBACKS =========================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PetMotor _)) return;

        GameEvents.RaiseFinishReached();
    }
}
