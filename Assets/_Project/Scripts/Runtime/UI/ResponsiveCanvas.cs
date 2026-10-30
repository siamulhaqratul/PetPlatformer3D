using UnityEngine;
using UnityEngine.UI;

// Switches the canvas scaling and the portrait-aware elements whenever the screen turns between landscape and portrait.
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvas : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Landscape")]
    [Tooltip("Reference resolution on landscape screens.")]
    [SerializeField] private Vector2 _landscapeResolution = new Vector2(1920f, 1080f);
    [Tooltip("Width (0) to height (1) match on landscape screens.")]
    [Range(0f, 1f)]
    [SerializeField] private float _landscapeMatch = 0.5f;

    [Header("Portrait")]
    [Tooltip("Reference resolution on portrait screens.")]
    [SerializeField] private Vector2 _portraitResolution = new Vector2(1080f, 1920f);
    [Tooltip("Width (0) to height (1) match on portrait screens. Matching width keeps everything inside a narrow screen.")]
    [Range(0f, 1f)]
    [SerializeField] private float _portraitMatch;

    [Header("Elements")]
    [Tooltip("Elements that move or resize in portrait.")]
    [SerializeField] private OrientationLayout[] _layouts;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private CanvasScaler _scaler;
    private bool _isPortrait;
    private bool _hasApplied;

    // ======================= UNITY CALLBACKS =========================

    // Awake is needed so the layout is in place before the UI manager's Start starts its idle tweens.
    private void Awake()
    {
        _scaler = GetComponent<CanvasScaler>();
        ApplyOrientation();
    }

    // Unity calls this on the root canvas whenever the screen size changes, e.g. when a phone rotates.
    private void OnRectTransformDimensionsChange()
    {
        if (_scaler == null) return;

        ApplyOrientation();
    }

    // ========================= PRIVATE METHODS =======================

    private void ApplyOrientation()
    {
        bool isPortrait = Screen.height > Screen.width;
        if (_hasApplied && isPortrait == _isPortrait) return;

        _isPortrait = isPortrait;
        _hasApplied = true;
        _scaler.referenceResolution = isPortrait ? _portraitResolution : _landscapeResolution;
        _scaler.matchWidthOrHeight = isPortrait ? _portraitMatch : _landscapeMatch;

        foreach (OrientationLayout layout in _layouts)
        {
            layout.Apply(isPortrait);
        }
    }
}
