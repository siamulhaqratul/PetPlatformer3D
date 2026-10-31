using UnityEngine;

// Moves, resizes, scales and optionally re-anchors one UI element for portrait screens; its built (landscape) layout
// is restored otherwise.
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
    [Tooltip("Whether portrait uses its own anchor, e.g. the top or bottom edge, so the element follows that edge on tall phones.")]
    [SerializeField] private bool _hasPortraitAnchor;
    [Tooltip("Anchor used on portrait screens when Has Portrait Anchor is on; the position is measured from it.")]
    [SerializeField] private Vector2 _portraitAnchor = new Vector2(0.5f, 0.5f);

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private RectTransform _rect;
    private Vector2 _landscapePosition;
    private Vector2 _landscapeSize;
    private Vector3 _landscapeScale;
    private Vector2 _landscapeAnchorMin;
    private Vector2 _landscapeAnchorMax;
    private bool _isInitialized;

    // ========================= INITIALIZE METHODS ========================

    // Called on first use rather than from Start, because the canvas applies its layout before any Start runs.
    private void InitializeLayout()
    {
        _rect = (RectTransform)transform;
        _landscapePosition = _rect.anchoredPosition;
        _landscapeSize = _rect.sizeDelta;
        _landscapeScale = _rect.localScale;
        _landscapeAnchorMin = _rect.anchorMin;
        _landscapeAnchorMax = _rect.anchorMax;
        _isInitialized = true;
    }

    // ========================= PUBLIC METHODS ========================

    public void Configure(Vector2 portraitPosition, Vector2 portraitSize, float portraitScale)
    {
        _portraitPosition = portraitPosition;
        _portraitSize = portraitSize;
        _portraitScale = portraitScale;
    }

    public void ConfigureAnchor(Vector2 portraitAnchor)
    {
        _hasPortraitAnchor = true;
        _portraitAnchor = portraitAnchor;
    }

    public void Apply(bool isPortrait)
    {
        if (!_isInitialized)
        {
            InitializeLayout();
        }

        bool usePortraitAnchor = isPortrait && _hasPortraitAnchor;
        _rect.anchorMin = usePortraitAnchor ? _portraitAnchor : _landscapeAnchorMin;
        _rect.anchorMax = usePortraitAnchor ? _portraitAnchor : _landscapeAnchorMax;
        _rect.anchoredPosition = isPortrait ? _portraitPosition : _landscapePosition;
        _rect.sizeDelta = isPortrait ? _portraitSize : _landscapeSize;
        _rect.localScale = isPortrait ? Vector3.one * _portraitScale : _landscapeScale;
    }
}
