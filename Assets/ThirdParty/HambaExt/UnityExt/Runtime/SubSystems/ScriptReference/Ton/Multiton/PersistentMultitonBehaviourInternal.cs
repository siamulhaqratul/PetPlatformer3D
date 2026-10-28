// -----------------------------------------------------------------------------
//  File: PersistentMultitonBehaviourInternal.cs
//  Description: Internal persistent MonoBehaviour multiton implementation in the Ton system.
//  Provides internal access to all persistent MonoBehaviour multiton instances.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Internal persistent MonoBehaviour multiton implementation in the Ton system.
    /// Provides internal access to all persistent MonoBehaviour multiton instances.
    /// </summary>
    public abstract class PersistentMultitonBehaviourInternal<T> : PersistentMultitonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets all persistent MonoBehaviour multiton instances (internal access).
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