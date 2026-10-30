using UnityEngine;

// Moves, resizes and scales one UI element for portrait screens; its built (landscape) layout is restored otherwise.
[RequireComponent(typeof(RectTransform))]
public class OrientationLayout : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Portrait")]
    [Tooltip("Anchored position used on portrait screens.")]
    [SerializeField] private Vector2 _portraitPosition;
    [Tooltip("Size used on portrait screens.")]
    [SerializeField] private Vector2 _portraitSize;
    [Tooltip("Uniform scale used on portrait screens.")]
    [SerializeField] private float _portraitScale = 1f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private RectTransform _rect;
    private Vector2 _landscapePosition;
    private Vector2 _landscapeSize;
    private Vector3 _landscapeScale;
    private bool _isInitialized;

    // ========================= INITIALIZE METHODS ========================

    // Called on first use rather than from Start, because the canvas applies its layout before any Start runs.
    private void InitializeLayout()
    {
        _rect = (RectTransform)transform;
        _landscapePosition = _rect.anchoredPosition;
        _landscapeSize = _rect.sizeDelta;
        _landscapeScale = _rect.localScale;
        _isInitialized = true;
    }

    // ========================= PUBLIC METHODS ========================

    public void Configure(Vector2 portraitPosition, Vector2 portraitSize, float portraitScale)
    {
        _portraitPosition = portraitPosition;
        _portraitSize = portraitSize;
        _portraitScale = portraitScale;
    }

    public void Apply(bool isPortrait)
    {
        if (!_isInitialized)
        {
            InitializeLayout();
        }

        _rect.anchoredPosition = isPortrait ? _portraitPosition : _landscapePosition;
        _rect.sizeDelta = isPortrait ? _portraitSize : _landscapeSize;
        _rect.localScale = isPortrait ? Vector3.one * _portraitScale : _landscapeScale;
    }
}
