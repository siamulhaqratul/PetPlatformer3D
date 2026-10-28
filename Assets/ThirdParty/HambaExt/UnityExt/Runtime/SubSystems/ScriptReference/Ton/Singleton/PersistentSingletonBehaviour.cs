// -----------------------------------------------------------------------------
//  File: PersistentSingletonBehaviour.cs
//  Description: Persistent MonoBehaviour singleton implementation in the Ton system.
//  Provides access to the persistent MonoBehaviour singleton instance.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Persistent MonoBehaviour singleton implementation in the Ton system.
    /// Provides access to the persistent MonoBehaviour singleton instance.
    /// </summary>
    public abstract class PersistentSingletonBehaviour<T> : PersistentSingletonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets the persistent MonoBehaviour singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                return SingletonBase<T>.Instance;
            }
        }
    }
}