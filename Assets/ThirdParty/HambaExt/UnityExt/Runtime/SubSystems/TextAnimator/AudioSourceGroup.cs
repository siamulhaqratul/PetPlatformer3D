using UnityEngine;
using UnityEngine.Serialization;

namespace UnityExt.TextMeshAnimator
{
    /// <summary>Cycles through a group of <see cref="AudioSource"/> instances to play typing or UI tick sounds without overlap.</summary>
    public class AudioSourceGroup : MonoBehaviour
    {
        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [FormerlySerializedAs("typingSources")]
        [Tooltip("Audio sources used in a round-robin pattern for consecutive play requests.")]
        [SerializeField] private AudioSource[] _typingSources;

        // =================== PRIVATE/PROTECTED FIELDS ====================

        private int _nextTypeSource;

        // =========================== PROPERTIES ==========================

        /// <summary>Total number of configured audio sources.</summary>
        public int SourceCount => _typingSources == null ? 0 : _typingSources.Length;
        /// <summary>Index of the next audio source that will be used (wraps automatically).</summary>
        public int NextIndex => _nextTypeSource;
        /// <summary>Returns true if at least one valid <see cref="AudioSource"/> is configured.</summary>
        public bool HasSources => SourceCount > 0;

        // ========================= PUBLIC METHODS ========================

        /// <summary>Plays the provided clip on the next audio source and advances the index.</summary>
        /// <param name="clip">The clip to play (ignored if null).</param>
        public void PlayFromNextSource(AudioClip clip)
        {
            if (!HasSources || clip == null) return;
            if (_nextTypeSource >= SourceCount) _nextTypeSource = 0; // Safety if array changed at runtime.

            var nextSource = _typingSources[_nextTypeSource];
            if (nextSource == null)
            {
                // Skip null entries and advance so a single bad slot doesn't block playback.
                AdvanceIndex();
                return;
            }

            nextSource.clip = clip;
            nextSource.Play();
            AdvanceIndex();
        }

        /// <summary>Resets the round-robin index to start from the first source again.</summary>
        public void ResetCycle()
        {
            _nextTypeSource = 0;
        }

        // ========================= PRIVATE METHODS =======================

        /// <summary>Advances the internal index with wrap-around.</summary>
        private void AdvanceIndex()
        {
            if (SourceCount == 0) return;
            _nextTypeSource = (_nextTypeSource + 1) % SourceCount;
        }
    }
}