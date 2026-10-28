using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Extension methods for serially retrieving components in children or parent chains of a Transform.
    /// </summary>
    public static class ExGetComp
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Recursively collects all components of type <typeparamref name="T"/> in the children of the given Transform, optionally including inactive GameObjects.
        /// </summary>
        /// <typeparam name="T">Component type to search for.</typeparam>
        /// <param name="t">The root Transform to search from.</param>
        /// <param name="includeInactive">If true, includes components on inactive GameObjects.</param>
        /// <returns>List of found components in child hierarchy (depth-first order).</returns>
        public static List<T> ExGetComponentsInChildrenSerially<T>(this Transform t, bool includeInactive = false)
        {
            return GetComInChildrenSerially(t, includeInactive);
            List<T> GetComInChildrenSerially(Transform startTransform, bool includeInactive)
            {
                var result = new List<T>();
                GetCom(ref result, startTransform);
                return result;
                void GetCom(ref List<T> compList, Transform t)
                {
                    if (compList == null) { compList = new List<T>(); }
                    var com = t.GetComponent<T>();
                    if (com != null && (t.gameObject.activeInHierarchy || includeInactive))
                    {
                        compList.Add(com);
                    }
                    if (t.childCount > 0)
                    {
                        for (int i = 0; i < t.childCount; i++)
                        {
                            var ch = t.GetChild(i);
                            GetCom(ref compList, ch);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Collects all components of type <typeparamref name="T"/> in the parent chain of the given Transform, optionally including inactive GameObjects.
        /// </summary>
        /// <typeparam name="T">Component type to search for.</typeparam>
        /// <param name="t">The Transform to start from.</param>
        /// <param name="includeInactive">If true, includes components on inactive GameObjects.</param>
        /// <returns>List of found components in parent chain (from self upward).</returns>
        public static List<T> ExGetComponentsInParentSerially<T>(this Transform t, bool includeInactive = false)
        {
            return GetComInParentSerially(t, includeInactive);
            List<T> GetComInParentSerially(Transform startTransform, bool includeInactive)
            {
                var result = new List<T>();
                GetCom(ref result, startTransform);
                return result;
                void GetCom(ref List<T> compList, Transform t)
                {
                    var ptr = t;
                    while (ptr != null)
                    {
                        var com = ptr.GetComponent<T>();
                        if (com != null && (ptr.gameObject.activeInHierarchy || includeInactive))
                        {
                            compList.Add(com);
                        }
                        ptr = ptr.parent;
                    }
                }
            }
        }
    }
}