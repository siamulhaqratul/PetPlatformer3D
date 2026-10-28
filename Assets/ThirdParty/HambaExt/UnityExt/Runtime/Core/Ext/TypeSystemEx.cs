using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace UnityExt
{
    /// <summary>Extension methods for discovering interface/class implementations and converting MonoBehaviour collections.</summary>
    public static class ExTypeSystem
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>Finds all components of type <typeparamref name="T"/> in the currently active scene's root hierarchy.</summary>
        /// <typeparam name="T">Interface or class type to search for.</typeparam>
        /// <param name="mono">Context MonoBehaviour (unused; enables extension syntax).</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        /// <returns>List of implementations found in active scene.</returns>
        public static List<T> GetImplementationFromCurrentlyLoadedMainScene<T>(this MonoBehaviour mono, bool includeInactive) where T : class
        {
            var result = new List<T>();
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            rootObjects.ExForEachSafe((r) =>
            {
                var comps = r.GetComponentsInChildren<T>(includeInactive);
                if (comps.ExIsValid()) { result.AddRange(comps); }
            });
            return result;
        }
        /// <summary>Gets all objects in the project implementing <typeparamref name="T"/> by scanning active MonoBehaviours.</summary>
        /// <typeparam name="T">Interface or class type to match.</typeparam>
        /// <param name="mono">Context MonoBehaviour (unused; enables extension syntax).</param>
        /// <param name="acitivityMode">Include or exclude inactive objects.</param>
        /// <param name="sortMode">Sort mode applied by Unity object search.</param>
        /// <returns>List of implementations.</returns>
        public static List<T> GetImplementedInterfaces<T>(this MonoBehaviour mono, 
            FindObjectsInactive acitivityMode = FindObjectsInactive.Include, FindObjectsSortMode sortMode = FindObjectsSortMode.None) where T : class
        {
            var allMonos = MonoBehaviour.FindObjectsByType<MonoBehaviour>(acitivityMode, sortMode);
            var result = new List<T>();
            allMonos.ExForEachSafe((i) =>
            {
                var r = i as T;
                if (r != null)
                {
                    result.Add(r);
                }
            });
            return result;
        }
        /// <summary>Gets the first object implementing <typeparamref name="T"/> among all active MonoBehaviours.</summary>
        /// <typeparam name="T">Interface or class type to look for.</typeparam>
        /// <param name="mono">Context MonoBehaviour (unused; enables extension syntax).</param>
        /// <param name="acitivityMode">Include or exclude inactive objects.</param>
        /// <param name="sortMode">Sort mode applied by Unity object search.</param>
        /// <returns>First matching implementation or null.</returns>
        public static T GetImplementedInterface<T>(this MonoBehaviour mono,
            FindObjectsInactive acitivityMode = FindObjectsInactive.Include, FindObjectsSortMode sortMode = FindObjectsSortMode.None) where T : class
        {
            T result = null;
            var allScripts = MonoBehaviour.FindObjectsByType<MonoBehaviour>(acitivityMode, sortMode);
            allScripts.ExForEachSafe((i) =>
            {
                var intfs = i as T;
                if (intfs != null)
                {
                    result = intfs;
                }
            });
            return result;
        }
        //todo child mono type from UIF. i.e. ExGetChildScript<> 
        
        /// <summary>Determines whether interface/class <typeparamref name="T"/> is assignable from a provided <see cref="Type"/>.</summary>
        /// <typeparam name="T">Target interface/class to test.</typeparam>
        /// <param name="interfaceType">Type instance to evaluate.</param>
        /// <returns>True if assignable; otherwise false.</returns>
        public static bool ExInterfaceIsImplementedOnClass<T>(this Type interfaceType)
        {
            return interfaceType.GetType().IsAssignableFrom(typeof(T));
        }
        /// <summary>Returns first element in list whose runtime type exactly equals <typeparamref name="T1"/>.</summary>
        /// <typeparam name="T1">Desired type to extract.</typeparam>
        /// <typeparam name="T2">Base or common list element type.</typeparam>
        /// <param name="scriptList">List of scripts to scan (null-safe).</param>
        /// <returns>First casted instance or null.</returns>
        public static T1 ExGetTypedScript<T1, T2>(this List<T2> scriptList) where T1 : class where T2 : class
        {
            T1 result = null;
            if (scriptList != null && scriptList.Count > 0)
            {
                for (int i = 0; i < scriptList.Count; i++)
                {
                    var sc = scriptList[i];
                    if (sc == null) { continue; }
                    if (sc.GetType() == typeof(T1))
                    {
                        result = sc as T1;
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>Returns first element in list whose runtime type exactly equals <typeparamref name="T1"/> (no list null/empty guard).</summary>
        /// <typeparam name="T1">Desired type to extract.</typeparam>
        /// <typeparam name="T2">Base or common list element type.</typeparam>
        /// <param name="scriptList">Non-null list of scripts to scan.</param>
        /// <returns>First casted instance or null.</returns>
        public static T1 ExGetTypedScript_NoListCheck<T1, T2>(this List<T2> scriptList) where T1 : class where T2 : class
        {
            T1 result = null;
            for (int i = 0; i < scriptList.Count; i++)
            {
                var sc = scriptList[i];
                if (sc.GetType() == typeof(T1))
                {
                    result = sc as T1;
                    break;
                }
            }
            return result;
        }
        /// <summary>Converts a list of MonoBehaviours to list of type <typeparamref name="T"/> via safe casting.</summary>
        /// <typeparam name="T">Interface or derived type to cast to.</typeparam>
        /// <param name="scripts">Source list of MonoBehaviours.</param>
        /// <returns>List of successfully cast elements.</returns>
        public static List<T> ExConvertTo<T>(this List<MonoBehaviour> scripts) where T : class
        {
            var result = new List<T>();
            if (scripts != null && scripts.Count > 0)
            {
                for (int i = 0; i < scripts.Count; i++)
                {
                    var nt = scripts[i];
                    if (nt == null) { continue; }
                    var it = nt as T;
                    if (it == null) { continue; }
                    result.Add(it);
                }
            }
            return result;
        }
        /// <summary>Converts an array of MonoBehaviours to list of type <typeparamref name="T"/> via safe casting.</summary>
        /// <typeparam name="T">Interface or derived type to cast to.</typeparam>
        /// <param name="scripts">Source array of MonoBehaviours.</param>
        /// <returns>List of successfully cast elements.</returns>
        public static List<T> ExConvertTo<T>(this MonoBehaviour[] scripts) where T : class
        {
            var result = new List<T>();
            if (scripts != null && scripts.Length > 0)
            {
                for (int i = 0; i < scripts.Length; i++)
                {
                    var nt = scripts[i];
                    if (nt == null) { continue; }
                    var it = nt as T;
                    if (it == null) { continue; }
                    result.Add(it);
                }
            }
            return result;
        }
    }
}