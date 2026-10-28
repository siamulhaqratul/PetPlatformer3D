// -----------------------------------------------------------------------------
//  File: MultitonSO.cs
//  Description: Multiton pattern ScriptableObject implementation in the Ton system.
//  Provides access to all ScriptableObject multiton instances.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Multiton pattern ScriptableObject implementation in the Ton system.
    /// Provides access to all ScriptableObject multiton instances.
    /// </summary>
    public abstract class MultitonSO<T> : MultitonSOProtected<T> where T : ScriptableObject
    {
        /// <summary>
        /// Gets all ScriptableObject multiton instances.
        /// </summary>
        public static IReadOnlyList<T> Instances
        {
            get
            {
                return instances;
            }
        }
    }
}