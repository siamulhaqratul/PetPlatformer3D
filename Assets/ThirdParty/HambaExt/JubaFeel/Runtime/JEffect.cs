using System.Collections;
using UnityEngine;

namespace JFeel
{
    /// <summary>
    /// Abstract base class for visual or interactive effects in the JFeel system.
    /// Provides a framework for showing and hiding effects, with optional delayed start.
    /// 
    /// <para>
    /// <b>Usage:</b> Inherit from this class to implement custom effects. 
    /// Override <see cref="Show"/> and <see cref="Hide"/> to define effect behavior.
    /// </para>
    /// 
    /// <para>
    /// <b>Inspector Options:</b>
    /// <list type="bullet">
    /// <item><description><b>showOnStart</b>: If true, the effect will show automatically on Start.</description></item>
    /// <item><description><b>useDelay</b>: If true, the effect will wait for <b>delayAmount</b> seconds before showing.</description></item>
    /// <item><description><b>delayAmount</b>: The delay time in seconds before showing the effect (if <b>useDelay</b> is enabled).</description></item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <b>Methods:</b>
    /// <list type="bullet">
    /// <item><description><see cref="Show(bool)"/>: Shows the effect. Can be overridden for custom logic.</description></item>
    /// <item><description><see cref="Hide(System.Action, bool)"/>: Hides the effect and invokes an optional completion callback.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public abstract class JEffect : MonoBehaviour
    {
        [Header("Advanced")]
        [SerializeField] protected bool showOnStart = false;
        [SerializeField] bool useDelay = false;
        [SerializeField] float delayAmount = 1f;
        /// <summary>
        /// Unity Awake callback. Override for initialization logic.
        /// </summary>
        protected virtual void Awake() { }
        /// <summary>
        /// Unity Start coroutine. Handles optional delayed effect showing on start.
        /// </summary>
        /// <returns>IEnumerator for coroutine handling.</returns>
        protected virtual IEnumerator Start()
        {
            if (showOnStart)
            {
                if (useDelay)
                {
                    yield return new WaitForSeconds(delayAmount);
                }
                Show();
            }
        }
        /// <summary>
        /// Shows the effect. Override to implement custom show logic.
        /// </summary>
        /// <param name="force">If true, forces the effect to show regardless of state.</param>
        public virtual void Show(bool force = false) { }
        /// <summary>
        /// Hides the effect. Override to implement custom hide logic.
        /// </summary>
        /// <param name="OnComplete">Optional callback invoked after hiding is complete.</param>
        /// <param name="force">If true, forces the effect to hide regardless of state.</param>
        public virtual void Hide(System.Action OnComplete = null, bool force = false) { OnComplete?.Invoke(); }
    }
}