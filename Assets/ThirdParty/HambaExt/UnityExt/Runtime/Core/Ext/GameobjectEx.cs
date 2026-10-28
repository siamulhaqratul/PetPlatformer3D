using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Extension methods for GameObject activation and layer management utilities.
    /// </summary>
    public static class ExGameobject
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Sets the active state of a GameObject, forcing the value if not null.
        /// </summary>
        /// <param name="gameObject">The GameObject to activate or deactivate.</param>
        /// <param name="active">Whether to set the GameObject active or inactive.</param>
        public static void ExSetActiveForce(this GameObject gameObject, bool active)
        {
            if (gameObject != null) { gameObject.SetActive(active); }
        }

        /// <summary>
        /// Sets the active state for all GameObjects in a list, skipping nulls.
        /// </summary>
        /// <param name="objectLists">List of GameObjects to activate or deactivate.</param>
        /// <param name="active">Whether to set each GameObject active or inactive.</param>
        public static void ExSetActiveForce(this List<GameObject> objectLists, bool active)
        {
            if (objectLists != null && objectLists.Count > 0)
            {
                for (int i = 0; i < objectLists.Count; i++)
                {
                    var obj = objectLists[i];
                    if (obj == null) { continue; }
                    obj.SetActive(active);
                }
            }
        }

        /// <summary>
        /// Sets the active state for all GameObjects in a list, but only if their current state differs from the target.
        /// </summary>
        /// <param name="objectLists">List of GameObjects to activate or deactivate.</param>
        /// <param name="active">Whether to set each GameObject active or inactive.</param>
        public static void ExSetActiveObjects(this List<GameObject> objectLists, bool active)
        {
            if (objectLists != null && objectLists.Count > 0)
            {
                for (int i = 0; i < objectLists.Count; i++)
                {
                    var obj = objectLists[i];
                    if (obj == null) { continue; }
                    if (obj.activeInHierarchy == active) { continue; }
                    obj.SetActive(active);
                }
            }
        }

        /// <summary>
        /// Sets the layer of a GameObject by name.
        /// </summary>
        /// <param name="gameObject">The GameObject to set the layer for.</param>
        /// <param name="layerName">The name of the layer to assign.</param>
        public static void ExSetLayer(this GameObject gameObject, string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }

        /// <summary>
        /// Sets the layer for all child transforms of a GameObject recursively.
        /// </summary>
        /// <param name="gameObject">The GameObject whose children will be updated.</param>
        /// <param name="layerName">The name of the layer to assign.</param>
        public static void ExSetLayerToAllChilds(this GameObject gameObject, string layerName)
        {
            gameObject.transform.ExForEachChilds((t) =>
            {
                t.gameObject.ExSetLayer(layerName);
            });
        }

        /// <summary>
        /// Sets the layer for all immediate (non-recursive) child transforms of a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject whose immediate children will be updated.</param>
        /// <param name="layerName">The name of the layer to assign.</param>
        public static void ExSetLayerToImmediateChilds(this GameObject gameObject, string layerName)
        {
            gameObject.transform.ExForEachImmediateChilds((t) =>
            {
                t.gameObject.ExSetLayer(layerName);
            });
        }

        /// <summary>
        /// Sets the layer for all parent transforms in the hierarchy chain of a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject whose parent chain will be updated.</param>
        /// <param name="layerName">The name of the layer to assign.</param>
        public static void ExSetLayerToParents(this GameObject gameObject, string layerName)
        {
            gameObject.transform.ExForEachParentOnChain((t) =>
            {
                t.gameObject.ExSetLayer(layerName);
            });
        }
    }
}