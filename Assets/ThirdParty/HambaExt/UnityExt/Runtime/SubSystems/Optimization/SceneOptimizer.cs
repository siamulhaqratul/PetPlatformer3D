using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityExt.Optimization
{
    /// <summary>Editor utility to remove unused MeshFilters (those without MeshCollider) and associated MeshRenderers, then mark prefabs and scene dirty.</summary>
    public class SceneOptimizer : MonoBehaviour
    {
        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [FormerlySerializedAs("targets")]
        [Tooltip("Root transforms whose child MeshFilters will be scanned for removal (filters without MeshCollider).")]
        [SerializeField] private List<Transform> _targets;

#if UNITY_EDITOR
        // ======================= UNITY EDITOR METHODS ====================

        [ContextMenu("Optimize")]
        private void DoIt()
        {
            if (_targets == null || _targets.Count == 0) { return; }
            var dirty = new List<UnityEngine.Object>();
            _targets.ExForEachSafe(root =>
            {
                if (root == null) { return; }
                var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
                meshFilters.ExForEachSafe(mf =>
                {
                    if (mf == null) { return; }
                    var collider = mf.GetComponent<MeshCollider>();
                    if (collider != null) { return; }

                    var tr = mf.transform;
                    var renderer = tr.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        DestroyImmediate(renderer);
                    }
                    DestroyImmediate(mf);
                    dirty.Add(tr);
                    dirty.Add(tr.gameObject);
                });
            });

            dirty.ExForEachSafe(obj =>
            {
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            });

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
#endif
    }
}