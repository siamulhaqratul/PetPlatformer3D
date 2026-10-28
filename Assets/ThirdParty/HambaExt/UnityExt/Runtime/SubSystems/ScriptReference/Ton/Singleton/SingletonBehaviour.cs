// -----------------------------------------------------------------------------
//  File: SingletonBehaviour.cs
//  Description: Singleton pattern MonoBehaviour implementation in the Ton system.
//  Provides access to the MonoBehaviour singleton instance.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Singleton pattern MonoBehaviour implementation in the Ton system.
    /// Provides access to the MonoBehaviour singleton instance.
    /// </summary>
    public abstract class SingletonBehaviour<T> : SingletonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets the MonoBehaviour singleton instance.
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