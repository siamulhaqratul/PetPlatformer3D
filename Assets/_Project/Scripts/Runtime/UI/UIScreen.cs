using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIScreen : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Tooltip("Content that pops in when the screen is shown. Optional.")]
    [SerializeField] private RectTransform _content;
    [Tooltip("Seconds to fade the screen in or out.")]
    [SerializeField] private float _fadeDuration = 0.2f;
    [Tooltip("Scale the content pops in from.")]
    [SerializeField] private float _popFromScale = 0.85f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private CanvasGroup _canvasGroup;

    // =========================== PROPERTIES ==========================

    public bool IsVisible => gameObject.activeSelf && _canvasGroup.interactable;

    // ======================= UNITY CALLBACKS =========================

    // Awake is needed because the UI manager shows or hides screens from its own Start.
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    // ========================= PUBLIC METHODS ========================

    public void Show()
    {
        gameObject.SetActive(true);
        _canvasGroup.DOKill();
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 0f;
        _canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true);

        if (_content == null) return;

        _content.DOKill();
        _content.localScale = Vector3.one * _popFromScale;
        _content.DOScale(Vector3.one, _fadeDuration * 2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;

        _canvasGroup.DOKill();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.DOFade(0f, _fadeDuration).SetUpdate(true).OnComplete(() => gameObject.SetActive(false));
    }

    public void HideImmediate()
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
