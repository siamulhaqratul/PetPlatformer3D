using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hazard : MonoBehaviour
{
    // ======================= UNITY CALLBACKS =========================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PetMotor _)) return;

        GameEvents.RaisePetHit();
    }
}
