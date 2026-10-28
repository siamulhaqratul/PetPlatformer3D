// -----------------------------------------------------------------------------
//  File: TonUtil.cs
//  Description: Utility functions for the Ton singleton/multiton system in Unity.
//  Provides helper methods for instance management and related operations.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt.Debugging;

namespace UnityExt.Tons
{
    /// <summary>
    /// Utility class for the Ton system.
    /// Contains helper methods for singleton and multiton management.
    /// </summary>
    internal static class TonUtil
    {
        /// <summary>
        /// Initializes a Ton instance as a singleton or multiton, with options for persistence and uniqueness.
        /// </summary>
        /// <typeparam name="T">Type of MonoBehaviour.</typeparam>
        /// <param name="IsPersistent">If true, instance persists across scenes.</param>
        /// <param name="IsUnique">If true, enforces uniqueness (singleton).</param>
        /// <param name="gameObject">The GameObject hosting the component.</param>
        /// <param name="_instance">Reference to the instance variable.</param>
        /// <param name="thisVar">This instance.</param>
        internal static void InitTon<T>(bool IsPersistent, bool IsUnique, GameObject gameObject, ref T _instance, T thisVar) where T : MonoBehaviour
        {
            // Handles singleton/multiton instance assignment and persistence logic
            if (IsPersistent)
            {
                if (IsUnique)
                {
                    if (_instance == null)
                    {
                        _instance = thisVar;
                        if (gameObject.transform.parent != null)
                        {
                            gameObject.transform.SetParent(null);
                        }
                        GameObject.DontDestroyOnLoad(gameObject);
                    }
                    else if (_instance != thisVar)
                    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        GLog.PrintError("The manager of type " + typeof(T) + " is designed to be a persistent singleton but " +
                            "we have found multiple gameobjects having this script. By design this is an error. " +
                            "If type " + typeof(T) + " should have multiple instances then please do not use singleton or " +
                            "you can use multiton instead. Deleting current instance now.");
#endif
                        if (Application.isPlaying)
                        {
                            GameObject.Destroy(thisVar);
                        }
                        else
                        {
                            GameObject.DestroyImmediate(thisVar);
                        }
                    }
                }
                else
                {
                    _instance = thisVar;
                    if (gameObject.transform.parent != null)
                    {
                        gameObject.transform.SetParent(null);
                    }
                    GameObject.DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                if (IsUnique)
                {
                    if (_instance == null)
                    {
                        _instance = thisVar;
                    }
                    else if (_instance != thisVar)
                    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        GLog.PrintError("The manager of type " + typeof(T) + " is designed to be a singleton but " +
                            "we have found multiple gameobjects having this script. By design this is an error. " +
                            "If type " + typeof(T) + " should have multiple instances then please do not use singleton or " +
                            "you can use multiton instead. Deleting current instance now.");
#endif
                        if (Application.isPlaying)
                        {
                            GameObject.Destroy(thisVar);
                        }
                        else
                        {
                            GameObject.DestroyImmediate(thisVar);
                        }
                    }
                }
                else
                {
                    _instance = thisVar;
                }
            }
        }
    }
}