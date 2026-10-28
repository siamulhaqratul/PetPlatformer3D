// -----------------------------------------------------------------------------
//  File: MultitonSOInternal.cs
//  Description: Internal ScriptableObject multiton implementation in the Ton system.
//  Provides internal access to all ScriptableObject multiton instances.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Internal ScriptableObject multiton implementation in the Ton system.
    /// Provides internal access to all ScriptableObject multiton instances.
    /// </summary>
    public abstract class MultitonSOInternal<T> : MultitonSOProtected<T> where T : ScriptableObject
    {
        /// <summary>
        /// Gets all ScriptableObject multiton instances (internal access).
        /// </summary>
        internal static IReadOnlyList<T> Instances
        {
            get
            {
                return instances;
            }
        }
    }
}