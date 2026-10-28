// -----------------------------------------------------------------------------
//  File: SingletonBase.cs
//  Description: Base class for MonoBehaviour singletons in the Ton system.
//  Ensures only one instance exists and manages persistence if needed.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Base class for MonoBehaviour singletons in the Ton system.
    /// Ensures only one instance exists and manages persistence if needed.
    /// </summary>
    public abstract class SingletonBase<T> : TonBehaviour<T> where T : MonoBehaviour
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        static T _instance = null;
        protected static T Instance => _instance;

        /// <summary>
        /// Indicates if the singleton should persist across scenes.
        /// </summary>
        protected abstract bool IsPersistent { get; }

        /// <summary>
        /// Called on Awake. Registers the singleton instance.
        /// </summary>
        protected override void OnAwake()
        {
            // Register this instance as the singleton
            base.OnAwake();
            TonUtil.InitTon<T>(IsPersistent, IsUnique : true, gameObject, ref _instance, this as T);
        }
    }
}