// -----------------------------------------------------------------------------
//  File: GeneralDatafile.cs
//  Description: Base class for general-purpose data storage in the Ton system.
//  Provides hooks for scene loading and cleanup.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityExt.Tons
{
    /// <summary>
    /// Base class for general-purpose data storage in the Ton system.
    /// Provides hooks for cleanup before level loading and starting a new Unity level.
    /// </summary>
    public abstract class GeneralDatafile : SoTonCore<GeneralDatafile>
    {
        /// <summary>
        /// Called before a new scene is loaded, for cleanup logic.
        /// </summary>
        /// <param name="currentScene">The current Unity scene.</param>
        protected internal virtual void OnCleanupBeforeLevelLoading(Scene currentScene)
        {
            // Override to implement cleanup logic before loading a new scene.
        }

        /// <summary>
        /// Called when starting a new Unity level.
        /// </summary>
        /// <param name="newSceneName">The name of the new scene.</param>
        protected internal virtual void OnStartANewUnityLevel(string newSceneName)
        {
            // Override to implement logic for starting a new level.
        }
    }
}