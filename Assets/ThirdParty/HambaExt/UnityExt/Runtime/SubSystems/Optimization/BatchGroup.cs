
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityExt.Optimization
{
    /// <summary>Applies batching-friendly settings to child MeshRenderers (shadows, probes, motion vectors) and triggers static batching.</summary>
    /// <remarks>This component should be attached to a GameObject that serves as a parent for multiple static MeshRenderers. It configures shadow settings and disables light and reflection probes as well as motion vectors on all child MeshRenderers, then combines them into a single static batch for improved rendering performance.</remarks>
    public class BatchGroup : MonoBehaviour
    {
        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [FormerlySerializedAs("activateShadowSetting")]
        [Tooltip("If true, shadow related settings will be applied to child MeshRenderers.")]
        [SerializeField] private bool _activateShadowSetting = true;

        [FormerlySerializedAs("castShadow"), FormerlySerializedAs("recieveShadow"), FormerlySerializedAs("shadowCasterStatic")]
        [Tooltip("Shadow casting (first), shadow receiving (second), and static shadow caster flag (third) when shadow settings are active.")]
        [SerializeField] private bool _castShadow = true, _receiveShadow = true, _shadowCasterStatic = true;

        // ======================= UNITY CALLBACKS =========================

        private void Start()
        {
            // NOTE: If platform-specific material application is required, ensure it completes before batching.
            var renderers = GetComponentsInChildren<MeshRenderer>(true);
            renderers.ExForEachSafe(r =>
            {
                if (r == null)
                {
                    return;
                }

                if (_activateShadowSetting)
                {
                    r.shadowCastingMode = _castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = _receiveShadow;
                    if (_castShadow)
                    {
                        r.staticShadowCaster = _shadowCasterStatic;
                    }
                }

                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            });

            StaticBatchingUtility.Combine(gameObject);
        }
    }
}