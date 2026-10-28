// -----------------------------------------------------------------------------
//  File: SingletonBehaviourInternal.cs
//  Description: Internal MonoBehaviour singleton implementation in the Ton system.
//  Provides internal access to the MonoBehaviour singleton instance.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Internal MonoBehaviour singleton implementation in the Ton system.
    /// Provides internal access to the MonoBehaviour singleton instance.
    /// </summary>
    public abstract class SingletonBehaviourInternal<T> : SingletonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets the MonoBehaviour singleton instance (internal access).
        /// </summary>
        internal static T Instance
        {
            get
            {
                return SingletonBase<T>.Instance;
            }
        }
    }
}