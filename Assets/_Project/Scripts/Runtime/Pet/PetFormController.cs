using System;
using UnityEngine;

// Owns the pet's current animal form: swaps the movement stats and the model, and tells the HUD.
public class PetFormController : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("References")]
    [Tooltip("Motor that receives each form's movement stats.")]
    [SerializeField] private PetMotor _motor;
    [Tooltip("Visuals that receive each form's model.")]
    [SerializeField] private PetVisual _visual;

    [Header("Forms")]
    [Tooltip("Form every run starts in.")]
    [SerializeField] private PetForm _startForm;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private PetForm _currentForm;

    // =========================== PROPERTIES ==========================

    public PetForm CurrentForm => _currentForm;
    public PetForm StartForm => _startForm;

    // ============================ EVENTS =============================

    // The bool is true when the change came from a gate, false for silent resets and respawns.
    public event Action<PetForm, bool> OnFormChanged;

    // ========================= PUBLIC METHODS ========================

    public void ChangeForm(PetForm form)
    {
        if (form == _currentForm) return;

        ApplyForm(form, true);
    }

    public void SetFormSilently(PetForm form)
    {
        ApplyForm(form, false);
    }

    // ========================= PRIVATE METHODS =======================

    private void ApplyForm(PetForm form, bool isFromGate)
    {
        _currentForm = form;
        _motor.SetStats(form.Stats);
        _visual.SetModel(form.ModelPrefab, form.AnimatorController, isFromGate);
        OnFormChanged?.Invoke(form, isFromGate);
    }
}
