// -----------------------------------------------------------------------------
//  File: PersistentMultitonBehaviourProtected.cs
//  Description: Protected persistent MonoBehaviour multiton base class in the Ton system.
//  Handles scene unload events and persistence.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityExt.Tons
{
    /// <summary>
    /// Protected persistent MonoBehaviour multiton base class in the Ton system.
    /// Handles scene unload events and persistence.
    /// </summary>
    public abstract class PersistentMultitonBehaviourProtected<T> : MultitonBase<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Indicates multitons are persistent by default.
        /// </summary>
        protected sealed override bool IsPersistent => true;

        /// <summary>
        /// Called on awake. Subscribes to scene unload events.
        /// </summary>
        protected override void OnAwake()
        {
            base.OnAwake();
            SceneManager.sceneUnloaded += OnUnloadUnityScene;
        }

        /// <summary>
        /// Called on destroy. Unsubscribes from scene unload events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneUnloaded -= OnUnloadUnityScene;
        }

        /// <summary>
        /// Called when a Unity scene is unloaded. Override for custom logic.
        /// </summary>
        protected virtual void OnUnloadUnityScene(Scene scene) { }
    }
}