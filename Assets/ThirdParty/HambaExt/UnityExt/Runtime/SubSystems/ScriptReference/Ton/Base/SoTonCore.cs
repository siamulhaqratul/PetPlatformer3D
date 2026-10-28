// -----------------------------------------------------------------------------
//  File: SoTonCore.cs
//  Description: Core base class for ScriptableObject-based Ton patterns.
//  Manages instance registration, scene events, and editor hooks.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityExt.Tons
{
    /// <summary>
    /// Core base class for ScriptableObject-based Ton patterns.
    /// Manages instance registration, scene events, and editor hooks.
    /// </summary>
    public abstract class SoTonCore<T> : ScriptableObject where T : ScriptableObject
    {
        /// <summary>
        /// Marker to ensure scene subscription only happens once.
        /// </summary>
        MarkerObject hasSubedFlag = null;
        static List<T> _instances = null;
        /// <summary>
        /// List of all registered instances.
        /// </summary>
        public static IReadOnlyList<T> Instances => _instances;

        /// <summary>
        /// Unity OnEnable method. Registers the instance and subscribes to scene events.
        /// </summary>
        protected virtual void OnEnable() 
        {
            if (hasSubedFlag != null) { return; }
            hasSubedFlag = new MarkerObject();
            if (_instances == null) { _instances = new List<T>(); }
            _instances = _instances.ExRemoveNullsUnityObjects();
            _instances.ExRemoveDuplicates();
            if (_instances == null) { _instances = new List<T>(); }
            var data = this as T;
            if (_instances.Contains(data) == false)
            {
                _instances.Add(data);
            }
            SceneManager.sceneLoaded += OnLoadUnityScene;
        } 

        /// <summary>
        /// Unity OnDestroy method. Unsubscribes from scene events.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (hasSubedFlag == null) { return; }
            SceneManager.sceneLoaded -= OnLoadUnityScene;
        }

        /// <summary>
        /// Called when a Unity scene is loaded. Cleans up the instance list.
        /// </summary>
        protected virtual void OnLoadUnityScene(Scene scene, LoadSceneMode mode) 
        {
            _instances = _instances.ExRemoveNullsUnityObjects();
            _instances.ExRemoveDuplicates();
        }
#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor validation hook.
        /// </summary>
        void OnValidate()
        {
            OnEditorUpdate();
        }
#endif
        /// <summary>
        /// Editor update hook for custom logic.
        /// </summary>
        protected virtual void OnEditorUpdate() { }

        /// <summary>
        /// Gets a specific instance of type T1 from the list.
        /// </summary>
        /// <typeparam name="T1">Type to search for.</typeparam>
        /// <returns>The found instance or null.</returns>
        public static T1 GetInstance<T1>() where T1 : MonoBehaviour
        {
            T1 result = null;
            _instances.ExForEachSafe((i) =>
            {
                if (i.GetType() == typeof(T1))
                {
                    result = i as T1;
                }
            });
            return result;
        }

        /// <summary>
        /// Gets all instances of type T1 from the list.
        /// </summary>
        /// <typeparam name="T1">Type to search for.</typeparam>
        /// <returns>List of found instances.</returns>
        public static List<T1> GetInstances<T1>() where T1 : MonoBehaviour
        {
            List<T1> result = new List<T1>();
            _instances.ExForEachSafe((i) =>
            {
                if (i.GetType() == typeof(T1))
                {
                    var res = i as T1;
                    result.Add(res);
                }
            });
            return result;
        }
    }
}