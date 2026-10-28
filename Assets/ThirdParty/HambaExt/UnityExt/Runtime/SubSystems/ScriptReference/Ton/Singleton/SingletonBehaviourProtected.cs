// -----------------------------------------------------------------------------
//  File: SingletonBehaviourProtected.cs
//  Description: Protected MonoBehaviour singleton implementation in the Ton system.
//  Sets persistence to false for derived singletons.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Protected MonoBehaviour singleton implementation in the Ton system.
    /// Sets persistence to false for derived singletons.
    /// </summary>
    public abstract class SingletonBehaviourProtected<T> : SingletonBase<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Indicates singletons are not persistent by default.
        /// </summary>
        protected sealed override bool IsPersistent => false;
    }
}