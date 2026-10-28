// -----------------------------------------------------------------------------
//  File: MultitonSOProtected.cs
//  Description: Protected ScriptableObject multiton base class in the Ton system.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Tons
{
    /// <summary>
    /// Protected ScriptableObject multiton base class in the Ton system.
    /// </summary>
    public abstract class MultitonSOProtected<T> : MultiSOTonBase<T> where T : ScriptableObject
    {
        // Inherit all logic from MultiSOTonBase<T>
    }
}