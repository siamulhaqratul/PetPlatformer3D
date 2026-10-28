// -----------------------------------------------------------------------------
//  File: PersistentSingletonBehaviourInternal.cs
//  Description: Internal persistent MonoBehaviour singleton implementation in the Ton system.
//  Provides internal access to the persistent MonoBehaviour singleton instance.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Internal persistent MonoBehaviour singleton implementation in the Ton system.
    /// Provides internal access to the persistent MonoBehaviour singleton instance.
    /// </summary>
    public abstract class PersistentSingletonBehaviourInternal<T> : PersistentSingletonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets the persistent MonoBehaviour singleton instance (internal access).
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