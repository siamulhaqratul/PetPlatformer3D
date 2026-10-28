// -----------------------------------------------------------------------------
//  File: MultitonBehaviour.cs
//  Description: Multiton pattern MonoBehaviour implementation in the Ton system.
//  Provides access to all MonoBehaviour multiton instances.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Multiton pattern MonoBehaviour implementation in the Ton system.
    /// Provides access to all MonoBehaviour multiton instances.
    /// </summary>
    public abstract class MultitonBehaviour<T> : MultitonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets all MonoBehaviour multiton instances.
        /// </summary>
        public static IReadOnlyList<T> Instances
        {
            get
            {
                return MultitonBase<T>.Instances;
            }
        }
    }
}