using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JFeel
{
    /// <summary>
    /// TapEffect provides a scaling animation effect for UI buttons when pressed and released.
    /// Implements <see cref="IPointerDownHandler"/> and <see cref="IPointerUpHandler"/> to trigger the effect.
    /// Inherits from <see cref="JEffect"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class TapEffect : JEffect, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Tween instance controlling the scale animation.
        /// </summary>
        Tween tween = null;

        /// <summary>
        /// Cached reference to this object's Transform.
        /// </summary>
        Transform _transform = null;

        /// <summary>
        /// Cached reference to the Button component.
        /// </summary>
        Button button;

        /// <summary>
        /// Unity Awake callback. Initializes references to Transform and Button.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _transform = transform;
            button = GetComponent<Button>();
        }

        /// <summary>
        /// Called when the pointer is pressed down on the button.
        /// Triggers the hide (press) animation if the button is interactable.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (ReferenceEquals(button, null) == false && button.interactable == false) return;
            Hide(null);
        }

        /// <summary>
        /// Called when the pointer is released from the button.
        /// Triggers the show (release) animation if the button is interactable.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (ReferenceEquals(button, null) == false && button.interactable == false) return;
            Show();
        }

        /// <summary>
        /// Inspector button to trigger the show animation for testing.
        /// </summary>
        [ContextMenu("Show")]
        private void ShowTest() { Show(); }

        /// <summary>
        /// Shows the button by scaling it back to its original size.
        /// Can be triggered from the inspector or programmatically.
        /// </summary>
        /// <param name="force">If true, instantly resets the scale and kills any running tween.</param>
        public override void Show(bool force = false)
        {
            if (_transform == null) return;
            if (force)
            {
                tween.Kill();
                _transform.localScale = Vector3.one;
                _transform.DOKill();
                return;
            }
            base.Show();
            _transform.DOKill();
            tween.Kill();
            tween = _transform.DOScale(1f, 0.25f);
        }

        /// <summary>
        /// Inspector button to trigger the hide animation for testing.
        /// </summary>
        [ContextMenu("Hide")]
        void HideTest() { Hide(null); }

        /// <summary>
        /// Hides the button by scaling it down.
        /// Can be triggered from the inspector, programmatically, or by pointer events.
        /// </summary>
        /// <param name="OnComplete">Optional callback invoked after the animation completes.</param>
        /// <param name="force">If true, instantly resets the scale and kills any running tween.</param>
        public override void Hide(System.Action OnComplete, bool force = false)
        {
            if (_transform == null) return;
            if (force)
            {
                tween.Kill();

                _transform.DOKill();
                _transform.localScale = Vector3.one;
                OnComplete?.Invoke();
                return;
            }
            base.Hide(null);
            _transform.DOKill();
            tween.Kill();
            tween = _transform.DOScale(0.9f, 0.25f).OnComplete(() => { OnComplete?.Invoke(); });
        }
    }
}