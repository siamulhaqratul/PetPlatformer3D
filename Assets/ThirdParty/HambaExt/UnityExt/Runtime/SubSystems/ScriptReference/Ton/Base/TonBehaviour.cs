// -----------------------------------------------------------------------------
//  File: TonBehaviour.cs
//  Description: Base MonoBehaviour for Ton patterns. Handles custom awake, scene events, and editor hooks.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityExt.Tons
{
    /// <summary>
    /// Base MonoBehaviour for Ton patterns. Handles custom awake, scene events, and editor hooks.
    /// </summary>
    public abstract class TonBehaviour<T> : MonoBehaviour, ICustomAwakeHandler where T : MonoBehaviour
    {
        /// <summary>
        /// Marker to ensure custom awake logic only runs once.
        /// </summary>
        MarkerObject awakeFlag = null;

        /// <summary>
        /// Custom awake handler for Ton patterns.
        /// </summary>
        void ICustomAwakeHandler.OnCustomAwake()
        {
            if (awakeFlag != null) { return; }
            OnAwake();
        }

        /// <summary>
        /// Unity Awake method. Calls custom awake logic.
        /// </summary>
        protected virtual void Awake()
        {
            if (awakeFlag != null) { return; }
            OnAwake();
        }

        /// <summary>
        /// Custom awake logic for Ton patterns. Used internally.
        /// </summary>
        protected virtual void OnAwake()
        {
            SceneManager.sceneLoaded += OnLoadUnityScene;
            awakeFlag = new MarkerObject();
        }

        /// <summary>
        /// Unity OnDestroy method. Unsubscribes from scene events.
        /// </summary>
        protected virtual void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnLoadUnityScene;
        }

        /// <summary>
        /// Unity OnEnable method. Override for custom logic.
        /// </summary>
        protected virtual void OnEnable() { }

        /// <summary>
        /// Unity OnDisable method. Override for custom logic.
        /// </summary>
        protected virtual void OnDisable() { }

        /// <summary>
        /// Called when a Unity scene is loaded. Override for custom logic.
        /// </summary>
        protected virtual void OnLoadUnityScene(Scene scene, LoadSceneMode mode) { }

        /// <summary>
        /// Editor update hook for custom logic.
        /// </summary>
        protected virtual void OnEditorUpdate() { }

        /// <summary>
        /// Editor gizmo drawing hook.
        /// </summary>
        protected virtual void OnDrawGizmoEditor() { }

        /// <summary>
        /// Editor selected gizmo drawing hook.
        /// </summary>
        protected virtual void OnDrawGizmoSelectedEditor() { }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor validation hook.
        /// </summary>
        void OnValidate()
        {
            OnEditorUpdate();
        }

        /// <summary>
        /// Unity Editor gizmo drawing hook.
        /// </summary>
        void OnDrawGizmos()
        {
            OnDrawGizmoEditor();
        }

        /// <summary>
        /// Unity Editor selected gizmo drawing hook.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            OnDrawGizmoSelectedEditor();
        }
#endif
    }
}