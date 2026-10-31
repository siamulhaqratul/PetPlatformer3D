using UnityEngine;
using UnityEngine.UI;

// Switches the canvas scaling and the portrait-aware elements whenever the screen turns between landscape and portrait.
// The scaler expands the canvas to cover at least the reference resolution, so on any aspect ratio
// (4:3, 16:10, 21:9, tall phones) the full layout stays on screen and only gains empty margin.
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvas : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Landscape")]
    [Tooltip("Reference resolution on landscape screens.")]
    [SerializeField] private Vector2 _landscapeResolution = new Vector2(1920f, 1080f);

    [Header("Portrait")]
    [Tooltip("Reference resolution on portrait screens.")]
    [SerializeField] private Vector2 _portraitResolution = new Vector2(1080f, 1920f);

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
        _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
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

        foreach (OrientationLayout layout in _layouts)
        {
            layout.Apply(isPortrait);
        }
    }
}
