// -----------------------------------------------------------------------------
//  File: MultiTonBase.cs
//  Description: Base class for MonoBehaviour multitons in the Ton system.
//  Manages multiple keyed MonoBehaviour instances with persistence options.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityExt.Tons
{
    /// <summary>
    /// Base class for MonoBehaviour multitons in the Ton system.
    /// Manages multiple keyed MonoBehaviour instances with persistence options.
    /// </summary>
    public abstract class MultitonBase<T> : TonBehaviour<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Indicates if the multiton instances should persist across scenes.
        /// </summary>
        protected abstract bool IsPersistent { get; }

        static T _instance = null;
        static List<T> _instances = null;
        /// <summary>
        /// List of all multiton instances.
        /// </summary>
        protected static IReadOnlyList<T> Instances => _instances;

        /// <summary>
        /// Called on Awake. Registers the instance and manages the list.
        /// </summary>
        protected override void OnAwake()
        {
            // Register this instance and manage the multiton list
            base.OnAwake();
            TonUtil.InitTon<T>(IsPersistent, IsUnique : false, gameObject, ref _instance, this as T);
            if (_instances == null) { _instances = new List<T>(); }
            _instances = _instances.ExRemoveNullsUnityObjects();
            _instances.ExRemoveDuplicates();
            if (_instances == null) { _instances = new List<T>(); }
            var data = this as T;
            if (_instances.Contains(data) == false)
            {
                _instances.Add(data);
            }
        }

        /// <summary>
        /// Called when a Unity scene is loaded. Cleans up the instance list.
        /// </summary>
        protected override void OnLoadUnityScene(Scene scene, LoadSceneMode mode)
        {
            // Clean up the multiton list on scene load
            base.OnLoadUnityScene(scene, mode);
            _instances = _instances.ExRemoveNullsUnityObjects();
            _instances.ExRemoveDuplicates();
        }

        /// <summary>
        /// Gets a specific instance of type T1 from the multiton list.
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
        /// Gets all instances of type T1 from the multiton list.
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

        /// <summary>
        /// Called when this instance is destroyed. Removes it from the multiton list.
        /// </summary>
        private void OnDestroy()
        {
            // Remove this instance from the multiton list
            base.OnDestroy();
            var data = this as T;
            if (_instances != null )
            {
                _instances.Remove(data);
            }
        }
    }
}