using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityExt.TaskManagement;

namespace UnityExt.TextMeshAnimator
{
    /// <summary>Controls typed dialogue reveal for a TMP text component with optional audio and markup-driven effects.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class PlayDialogue : MonoBehaviour
    {
        // =========================== CONSTANTS ===========================

    private static readonly bool ENABLE_EFFECT = true; // Use static readonly instead of const to avoid unreachable-code warnings if toggled.

        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [FormerlySerializedAs("autoPlayWhenEnable")]
        [Tooltip("Automatically begin the dialogue effect when this component is enabled.")]
        [SerializeField] private bool _autoPlayWhenEnable = true;

        [FormerlySerializedAs("typingClip")]
        [Tooltip("Optional audio clip played intermittently while revealing text.")]
        [SerializeField] private AudioClip _typingClip;

        [FormerlySerializedAs("audioSourceGroup")]
        [Tooltip("Audio source group used for round-robin playback of typing sounds.")]
        [SerializeField] private AudioSourceGroup _audioSourceGroup;

        // =================== PRIVATE/PROTECTED FIELDS ====================

        private TMP_Text _textBox;
        private DialogueVertexAnimator _dialogueAnim;
        private TaskHandle _routine;
        private string _startStr = string.Empty;
        private bool _completed;

        // =========================== PROPERTIES ==========================

        /// <summary>True once the current dialogue animation has fully revealed the text.</summary>
        public bool Completed => _completed;

        // ======================= UNITY CALLBACKS =========================

        private void Awake()
        {
            LoadIfRequired();
            _completed = false;
        }

        private void OnEnable()
        {
            if (_autoPlayWhenEnable)
            {
                StartEffect();
            }
        }

        private void OnDisable()
        {
            StopEffect();
        }

        // ========================= PUBLIC METHODS ========================

        /// <summary>Stops the typing effect, reveals all text immediately, and restores the original string.</summary>
        public void StopEffect()
        {
            if (!ENABLE_EFFECT) return;
            LoadIfRequired();
            _dialogueAnim.SkipToEndOfCurrentMessage();
            if (_routine != null)
            {
                _routine.TryKill();
                _routine = null;
            }
            _textBox.text = _startStr;
            _textBox.ForceMeshUpdate();
        }

        /// <summary>Starts (or restarts) the typing effect for the current text content.</summary>
        /// <param name="speedMultiplier">Multiplier applied to base character reveal speed (higher = faster).</param>
        public void StartEffect(float speedMultiplier = 1.0f)
        {
            if (!ENABLE_EFFECT) return;
            LoadIfRequired();
            _completed = false;
            if (_routine != null)
            {
                _routine.TryKill();
                _routine = null;
            }
            _dialogueAnim.SkipToEndOfCurrentMessage();
            _startStr = _textBox.text;
            List<DialogueCommand> commands = DialogueUtility.ProcessInputString(_textBox.text, out string totalTextMessage);
            _routine = TaskManager.StartTask(_dialogueAnim.AnimateTextIn(commands, totalTextMessage, _typingClip, () => { _completed = true; }, speedMultiplier));
        }

        // ========================= PRIVATE METHODS =======================

        /// <summary>Lazy-loads component references and animator wrapper.</summary>
        private void LoadIfRequired()
        {
            if (_textBox == null)
            {
                _textBox = GetComponent<TMP_Text>();
            }
            if (_dialogueAnim == null)
            {
                _dialogueAnim = new DialogueVertexAnimator(_textBox, _audioSourceGroup);
            }
        }
    }
}