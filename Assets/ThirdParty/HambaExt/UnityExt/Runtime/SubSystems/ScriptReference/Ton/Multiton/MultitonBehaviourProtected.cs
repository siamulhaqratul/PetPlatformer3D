// -----------------------------------------------------------------------------
//  File: MultitonBehaviourProtected.cs
//  Description: Protected MonoBehaviour multiton implementation in the Ton system.
//  Sets persistence to false for derived multitons.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Protected MonoBehaviour multiton implementation in the Ton system.
    /// Sets persistence to false for derived multitons.
    /// </summary>
    public abstract class MultitonBehaviourProtected<T> : MultitonBase<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Indicates multitons are not persistent by default.
        /// </summary>
        protected sealed override bool IsPersistent => false;
    }
}