// -----------------------------------------------------------------------------
//  File: SingletonSO.cs
//  Description: ScriptableObject singleton implementation in the Ton system.
//  Provides access to the ScriptableObject singleton instance.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// ScriptableObject singleton implementation in the Ton system.
    /// Provides access to the ScriptableObject singleton instance.
    /// </summary>
    public abstract class SingletonSO<T> : SingletonSOProtected<T> where T : ScriptableObject
    {
        /// <summary>
        /// Gets the ScriptableObject singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                return instance;
            }
        }
    }
}