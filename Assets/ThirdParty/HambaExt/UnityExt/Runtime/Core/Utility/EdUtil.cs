using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>Editor-side and scene utility extension helpers for asset validation, scene traversal, and GameObject operations.</summary>
    public static class EdUtil
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>Checks if a given GameObject originates from an FBX (model) asset file (Editor only).</summary>
        /// <param name="gameObject">Target GameObject.</param>
        /// <returns>True if source asset path is valid in editor; otherwise false (or always false in build).</returns>
        public static bool IsItFBXModelFile(GameObject gameObject)
        {
#if UNITY_EDITOR
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
            return assetPath.IsStringValid();
#else
            return false;
#endif
        }
        /// <summary>Converts an absolute path under the project Assets folder to a Unity relative "Assets/..." path.</summary>
        /// <param name="absolutePath">Full system path.</param>
        /// <returns>Relative path beginning with "Assets".</returns>
        /// <exception cref="System.ArgumentException">Thrown if the path isn't under the project Assets directory.</exception>
        public static string AssetsRelativePath(this string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
            {
                return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            }
            else
            {
                throw new System.ArgumentException("Full path does not contain the current project's Assets folder", "absolutePath");
            }
        }
        /// <summary>Searches the project for an asset of type <typeparamref name="T"/> whose name contains the provided search string.</summary>
        /// <typeparam name="T">Unity Object type to search (Material, Texture, etc.).</typeparam>
        /// <param name="searchName">Substring to look for in asset names.</param>
        /// <param name="typeName">Type name string used in AssetDatabase filter (defaults to "Material").</param>
        /// <returns>The first matching asset or null.</returns>
        public static T TryLoadAsset<T>(string searchName, string typeName = "Material") where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            T result = null;
            string[] guids1 = UnityEditor.AssetDatabase.FindAssets("t:" + typeName, null);

            foreach (string guid1 in guids1)
            {
                var a_path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid1);
                var obj_target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(a_path);
                if (obj_target.name.Contains(searchName))
                {
                    result = obj_target;
                    break;
                }
            }
            return result;
#else
            return default;
#endif
        }
        /// <summary>Collects all GameObjects in the active scene whose names contain the provided substring.</summary>
        /// <param name="lst">Reference to list assigned with results (will be reinitialized).</param>
        /// <param name="wName">Substring to match in GameObject names.</param>
        public static void GetAllGameobjectsWitMatchingName(ref List<GameObject> lst, string wName)
        {
            lst = new List<GameObject>();
            var all = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            if (all != null && all.Length > 0)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    var g = all[i];
                    if (g == null) { continue; }
                    GetObjects(ref lst, g, wName);
                }
            }

            void GetObjects(ref List<GameObject> _lst, GameObject g, string _wName)
            {
                if (g == null) { return; }
                if (g.name.Contains(_wName) && _lst.Contains(g) == false) { _lst.Add(g); }
                if (g.transform.childCount > 0)
                {
                    for (int i = 0; i < g.transform.childCount; i++)
                    {
                        var chObj = g.transform.GetChild(i).gameObject;
                        GetObjects(ref _lst, chObj, _wName);
                    }
                }
            }
        }
        /// <summary>Finds the first GameObject in the active scene hierarchy which has a component of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Component type to search for.</typeparam>
        /// <returns>The GameObject hosting the component, or null if none found.</returns>
        public static GameObject GetScriptObjectFromScene<T>() where T : MonoBehaviour
        {
            T result = null;
            var all = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            if (all != null && all.Length > 0)
            {
                foreach (var g in all)
                {
                    if (g == null) { continue; }
                    GetObjects(ref result, g);
                }
            }
            return result == null ? null : result.gameObject;

            void GetObjects(ref T _result, GameObject g)
            {
                if (g == null) { return; }
                if (g.GetComponent<T>() != null)
                {
                    _result = g.GetComponent<T>();
                    return;
                }
                if (g.transform.childCount > 0)
                {
                    for (int i = 0; i < g.transform.childCount; i++)
                    {
                        var chObj = g.transform.GetChild(i).gameObject;
                        GetObjects(ref _result, chObj);
                    }
                }
            }
        }
        /// <summary>Destroys all immediate (first-level) child GameObjects of the provided parent using DestroyImmediate.</summary>
        /// <param name="obj">Parent GameObject whose children will be removed.</param>
        public static void DestroyAllImmediateChildsOf(GameObject obj)
        {
            if (obj == null) { return; }
            var lst = new List<GameObject>();

            if (obj.transform.childCount > 0)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    lst.Add(obj.transform.GetChild(i).gameObject);
                }
            }

            foreach (var g in lst)
            {
                UnityEngine.Object.DestroyImmediate(g);
            }
        }
        /// <summary>Retrieves the first immediate child GameObject whose name contains the specified substring.</summary>
        /// <param name="obj">Parent GameObject.</param>
        /// <param name="wName">Substring to search in child names.</param>
        /// <returns>Matching child GameObject or null.</returns>
        public static GameObject GetImmediateChildWithName(GameObject obj, string wName)
        {
            GameObject result = null;
            if (obj.transform.childCount > 0)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    if (obj.transform.GetChild(i).gameObject.name.Contains(wName))
                    {
                        result = obj.transform.GetChild(i).gameObject;
                        break;
                    }
                }
            }
            return result;
        }
    }
}