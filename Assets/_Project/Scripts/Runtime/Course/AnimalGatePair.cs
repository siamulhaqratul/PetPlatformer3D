using UnityEngine;

// Two gates side by side: the one the pet runs through decides its form, the other is spent.
public class AnimalGatePair : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    // Seconds of sideways movement looked ahead when picking a gate, so a pet in the middle goes the way it steers.
    private const float STEER_LOOKAHEAD = 0.1f;

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Tooltip("Pet form controller that the chosen gate changes.")]
    [SerializeField] private PetFormController _formController;
    [Tooltip("Gates in this pair.")]
    [SerializeField] private AnimalGate[] _gates;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private bool _isUsed;

    // ======================= UNITY CALLBACKS =========================

    private void OnEnable()
    {
        GameEvents.OnRunReset += Rearm;
        GameEvents.OnPetRespawned += HandlePetRespawned;
    }

    private void OnDisable()
    {
        GameEvents.OnRunReset -= Rearm;
        GameEvents.OnPetRespawned -= HandlePetRespawned;
    }

    // ========================= PUBLIC METHODS ========================

    // The doors meet in the middle, so a pet there touches both; it gets the one it is closest to.
    public void ChooseFor(PetMotor pet)
    {
        if (_isUsed) return;

        _isUsed = true;
        AnimalGate chosenGate = FindClosestGate(pet);
        _formController.ChangeForm(chosenGate.Form);

        foreach (AnimalGate gate in _gates)
        {
            if (gate == chosenGate)
            {
                gate.PlayChosen();
            }
            else
            {
                gate.PlayRejected();
            }
        }
    }

    // ========================= PRIVATE METHODS =======================

    // A tie at dead centre with no steering goes to the first gate.
    private AnimalGate FindClosestGate(PetMotor pet)
    {
        float petX = pet.transform.position.x + pet.Velocity.x * STEER_LOOKAHEAD;
        AnimalGate closestGate = _gates[0];
        foreach (AnimalGate gate in _gates)
        {
            bool isCloser = Mathf.Abs(gate.transform.position.x - petX) < Mathf.Abs(closestGate.transform.position.x - petX);
            if (isCloser)
            {
                closestGate = gate;
            }
        }

        return closestGate;
    }

    // Gates still ahead of the respawn point can be chosen again.
    private void HandlePetRespawned(float respawnZ)
    {
        if (transform.position.z < respawnZ) return;

        Rearm();
    }

    private void Rearm()
    {
        _isUsed = false;
        foreach (AnimalGate gate in _gates)
        {
            gate.ResetGate();
        }
    }
}
