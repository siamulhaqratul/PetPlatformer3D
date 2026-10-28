// -----------------------------------------------------------------------------
//  File: SingletonSOProtected.cs
//  Description: Protected ScriptableObject singleton base class in the Ton system.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Protected ScriptableObject singleton base class in the Ton system.
    /// </summary>
    public abstract class SingletonSOProtected<T> : SingleSOTonBase<T> where T : ScriptableObject
    {
        // Inherit all logic from SingleSOTonBase<T>
    }
}