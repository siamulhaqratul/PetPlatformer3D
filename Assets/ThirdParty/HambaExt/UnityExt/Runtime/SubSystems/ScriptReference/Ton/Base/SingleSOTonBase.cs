// -----------------------------------------------------------------------------
//  File: SingleSOTonBase.cs
//  Description: Base class for ScriptableObject singletons in the Ton system.
//  Ensures only one ScriptableObject instance exists.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Base class for ScriptableObject singletons in the Ton system.
    /// Ensures only one ScriptableObject instance exists.
    /// </summary>
    public abstract class SingleSOTonBase<T> : SoTonCore<T> where T : ScriptableObject
    {
        /// <summary>
        /// The singleton ScriptableObject instance.
        /// </summary>
        static T _instance = null;
        protected static T instance => _instance;

        /// <summary>
        /// Unity OnEnable method. Registers the singleton instance.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _instance = this as T;
        }
    }
}