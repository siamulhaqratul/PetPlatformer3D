// -----------------------------------------------------------------------------
//  File: SingletonSOInternal.cs
//  Description: Internal ScriptableObject singleton implementation in the Ton system.
//  Provides internal access to the ScriptableObject singleton instance.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Internal ScriptableObject singleton implementation in the Ton system.
    /// Provides internal access to the ScriptableObject singleton instance.
    /// </summary>
    public abstract class SingletonSOInternal<T> : SingletonSOProtected<T> where T : ScriptableObject
    {
        /// <summary>
        /// Gets the ScriptableObject singleton instance (internal access).
        /// </summary>
        internal static T Instance
        {
            get
            {
                return instance;
            }
        }
    }
}