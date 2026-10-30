using UnityEngine;

[CreateAssetMenu(fileName = "PetForm", menuName = "Pet Shift Run/Pet Form")]
public class PetForm : ScriptableObject
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Identity")]
    [Tooltip("Name shown in the HUD and the change banner.")]
    [SerializeField] private string _displayName;
    [Tooltip("Ability description for keyboard players.")]
    [SerializeField] private string _abilityText;
    [Tooltip("Ability description for touch players.")]
    [SerializeField] private string _touchAbilityText;
    [Tooltip("Icon shown in the HUD and the change banner.")]
    [SerializeField] private Sprite _icon;

    [Header("Look")]
    [Tooltip("Cube Pets model used for this form.")]
    [SerializeField] private GameObject _modelPrefab;
    [Tooltip("Animator controller for the model.")]
    [SerializeField] private RuntimeAnimatorController _animatorController;

    [Header("Movement")]
    [Tooltip("Movement values for this form.")]
    [SerializeField] private PetMovementStats _stats;

    // =========================== PROPERTIES ==========================

    public string DisplayName => _displayName;
    public string AbilityText => _abilityText;
    public string TouchAbilityText => _touchAbilityText;
    public Sprite Icon => _icon;
    public GameObject ModelPrefab => _modelPrefab;
    public RuntimeAnimatorController AnimatorController => _animatorController;
    public PetMovementStats Stats => _stats;
}
