using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityExt.Optimization
{
    /// <summary>Container describing a root transform and renderer optimization flags for batching.</summary>
    [System.Serializable]
    public class BatcherData
    {
        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [FormerlySerializedAs("root")]
        [SerializeField] private Transform _root;
        [FormerlySerializedAs("shadow"), FormerlySerializedAs("occlusionCulling")]
        [SerializeField] private bool _shadow = true, _occlusionCulling = false;

        // =========================== PROPERTIES ==========================

        /// <summary>Root transform containing mesh renderers to process.</summary>
        public Transform Root => _root;
        /// <summary>Whether generated renderers should cast/receive shadows.</summary>
        public bool Shadow => _shadow;
        /// <summary>Whether occlusion culling optimization flag should be applied.</summary>
        public bool OcclusionCulling => _occlusionCulling;
    }

    /// <summary>Merges specified static mesh groups into a generated holder, batches them, and applies per-renderer optimization settings.</summary>
    public class EnvironmentOptimizer : MonoBehaviour
    {
        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [FormerlySerializedAs("otherStaticMeshSetRoots")]
        [Tooltip("List of batch groups to be merged and optimized under a generated holder.")]
        [SerializeField] private List<BatcherData> _otherStaticMeshSetRoots;

        [FormerlySerializedAs("shadow"), FormerlySerializedAs("occlusionCulling")]
        [Tooltip("Default shadow (first) and occlusion culling (second) flags for root optimizer call.")]
        [SerializeField] private bool _shadow = true, _occlusionCulling = false;

        // ======================= UNITY CALLBACKS =========================

        /// <summary>Generates combined static mesh hierarchy and applies optimization to contained renderers.</summary>
        private void Awake()
        {
            if (_otherStaticMeshSetRoots != null && _otherStaticMeshSetRoots.Count > 0)
            {
                var otherMeshHolder = new GameObject("_Gen_OtherMeshes_Holder");
                var newRoot = otherMeshHolder.transform;
                newRoot.SetParent(transform, true);
                newRoot.localPosition = Vector3.zero;
                newRoot.localEulerAngles = Vector3.zero;
                newRoot.localScale = Vector3.one;
                for (int i = 0; i < _otherStaticMeshSetRoots.Count; i++)
                {
                    var data = _otherStaticMeshSetRoots[i];
                    CopyObjectsFromAndDisableOlds(data, newRoot);
                }
            }

            StaticBatchingUtility.Combine(gameObject);
            transform.ExOptimizeMeshRenderersInside(_shadow, _occlusionCulling);
        }

        // ========================= PRIVATE METHODS =======================

        /// <summary>Duplicates enabled mesh renderers under a new root, disables originals, and applies optimization flags.</summary>
        /// <param name="data">Batch configuration containing the source root and flags.</param>
        /// <param name="newRoot">Destination parent transform for generated objects.</param>
        private void CopyObjectsFromAndDisableOlds(BatcherData data, Transform newRoot)
        {
            if (data == null || data.Root == null) { return; }
            var renderers = data.Root.GetComponentsInChildren<MeshRenderer>();
            if (renderers == null || renderers.Length == 0) { return; }

            for (int i = 0; i < renderers.Length; i++)
            {
                var rn = renderers[i];
                if (rn == null || !rn.enabled) { continue; }

                var curTr = rn.transform;
                var newObject = new GameObject("_gen_" + rn.gameObject.name);
                var newTr = newObject.transform;
                newTr.SetParent(newRoot, true);
                newTr.position = curTr.position;
                newTr.rotation = curTr.rotation;
                newTr.localScale = curTr.lossyScale;

                var oldMFilter = rn.GetComponent<MeshFilter>();
                var oldMRender = rn.GetComponent<MeshRenderer>();
                rn.enabled = false;

                var newMFilter = newObject.AddComponent<MeshFilter>();
                newMFilter.sharedMesh = oldMFilter != null ? oldMFilter.sharedMesh : null;
                var newMRender = newObject.AddComponent<MeshRenderer>();
                newMRender.sharedMaterial = oldMRender != null ? oldMRender.sharedMaterial : null;
                newMRender.ExOptimize(data.Shadow, data.OcclusionCulling);
            }
        }
    }
}