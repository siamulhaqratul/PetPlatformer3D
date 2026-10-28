// -----------------------------------------------------------------------------
//  File: MultitonBehaviourInternal.cs
//  Description: Internal MonoBehaviour multiton implementation in the Ton system.
//  Provides internal access to all MonoBehaviour multiton instances.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Internal MonoBehaviour multiton implementation in the Ton system.
    /// Provides internal access to all MonoBehaviour multiton instances.
    /// </summary>
    public abstract class MultitonBehaviourInternal<T> : MultitonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets all MonoBehaviour multiton instances (internal access).
        /// </summary>
        internal static IReadOnlyList<T> Instances
        {
            get
            {
                return MultitonBase<T>.Instances;
            }
        }
    }
}