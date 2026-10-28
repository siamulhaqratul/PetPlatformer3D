using DG.Tweening;
using UnityEngine;

namespace JFeel
{
    /// <summary>
    /// ShineEffect is a concrete implementation of <see cref="JEffect"/> that animates a "shine" RectTransform
    /// across a target RectTransform using DOTween. The effect can be triggered via the inspector or programmatically.
    /// 
    /// <para>
    /// <b>Inspector Fields:</b>
    /// <list type="bullet">
    /// <item><description><b>shine</b>: The RectTransform to animate as the shine effect.</description></item>
    /// <item><description><b>target</b>: The RectTransform that defines the destination position for the shine.</description></item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <b>Usage:</b>
    /// <list type="bullet">
    /// <item><description>Call <see cref="Show(bool)"/> to start the shine animation. This can be done from code or via the inspector button.</description></item>
    /// <item><description>The effect automatically starts on <see cref="Start"/> if the component is enabled.</description></item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <b>Animation Details:</b>
    /// <list type="bullet">
    /// <item><description>The shine RectTransform moves to the target's anchored position over 1.2 seconds, with linear easing.</description></item>
    /// <item><description>The animation sequence loops 1000 times, with short intervals before and after each move.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class ShineEffect : JEffect
    {
        /// <summary>
        /// The RectTransform to animate as the shine effect.
        /// </summary>
        [SerializeField] private RectTransform shine;
        /// <summary>
        /// The RectTransform that defines the destination position for the shine.
        /// </summary>
        [SerializeField] private RectTransform target;
        /// <summary>
        /// DOTween animation sequence for the shine effect.
        /// </summary>
        Sequence _seq = null;
        private void Start()
        {
            Show();
        }
        /// <summary>
        /// Inspector button to trigger the show animation for testing.
        /// </summary>
        [ContextMenu("Start")]
        private void ShowTest() { Show(); }

        /// <summary>
        /// Starts or restarts the shine animation sequence.
        /// Can be triggered from the inspector or programmatically.
        /// </summary>
        /// <param name="force">If true, forces the effect to show regardless of state.</param>
        public override void Show(bool force = false)
        {
            if (transform == null || target == null) return;
            base.Show();
            if (_seq == null)
            {
                _seq = DOTween.Sequence();
                _seq.AppendInterval(0.3f);
                _seq.Append(shine.DOAnchorPos(target.anchoredPosition, 1.2f).SetEase(Ease.Linear));
                _seq.AppendInterval(0.3f);
                _seq.SetLoops(1000);
            }
            else
            {
                _seq.Restart();
            }
        }

        private void OnDisable()
        {
            // _seq.Kill();
            //transform.DOKill();
        }
    }
}