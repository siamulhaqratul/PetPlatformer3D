// -----------------------------------------------------------------------------
//  File: PersistentMultitonBehaviour.cs
//  Description: Persistent MonoBehaviour multiton implementation in the Ton system.
//  Provides access to all persistent MonoBehaviour multiton instances.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Persistent MonoBehaviour multiton implementation in the Ton system.
    /// Provides access to all persistent MonoBehaviour multiton instances.
    /// </summary>
    public abstract class PersistentMultitonBehaviour<T> : PersistentMultitonBehaviourProtected<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets all persistent MonoBehaviour multiton instances.
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