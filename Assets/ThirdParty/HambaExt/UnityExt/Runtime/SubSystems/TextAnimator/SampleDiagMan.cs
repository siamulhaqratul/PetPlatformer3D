using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityExt.TaskManagement;

namespace UnityExt.TextMeshAnimator
{
    /// <summary>Sample dialogue manager demonstrating multi-button triggered animated text playback.</summary>
    /// <remarks>This component showcases how to use the DialogueVertexAnimator to display text with typing effects and optional audio feedback.</remarks>
    internal class SampleDiagMan : MonoBehaviour
    {
        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [FormerlySerializedAs("textBox")]
        [Tooltip("TMP_Text component used for displaying dialogue.")]
        [SerializeField] private TMP_Text _textBox;

        [FormerlySerializedAs("typingClip")]
        [Tooltip("Optional audio clip used for character typing sounds.")]
        [SerializeField] private AudioClip _typingClip;

        [FormerlySerializedAs("audioSourceGroup")]
        [Tooltip("Audio source group for round-robin playback of typing sounds.")]
        [SerializeField] private AudioSourceGroup _audioSourceGroup;

        [FormerlySerializedAs("playDialogue1Button")]
        [Tooltip("Button that plays the first dialogue sample.")]
        [SerializeField] private Button _playDialogue1Button;

        [FormerlySerializedAs("playDialogue2Button")]
        [Tooltip("Button that plays the second dialogue sample.")]
        [SerializeField] private Button _playDialogue2Button;

        [FormerlySerializedAs("playDialogue3Button")]
        [Tooltip("Button that plays the third dialogue sample.")]
        [SerializeField] private Button _playDialogue3Button;

        [FormerlySerializedAs("dialogue1")]
        [TextArea]
        [Tooltip("First dialogue text content.")]
        [SerializeField] private string _dialogue1;

        [FormerlySerializedAs("dialogue2")]
        [TextArea]
        [Tooltip("Second dialogue text content.")]
        [SerializeField] private string _dialogue2;

        [FormerlySerializedAs("dialogue3")]
        [TextArea]
        [Tooltip("Third dialogue text content.")]
        [SerializeField] private string _dialogue3;

        // =================== PRIVATE/PROTECTED FIELDS ====================

        private DialogueVertexAnimator _dialogueVertexAnimator;
        private TaskHandle _typeRoutine;

        // =========================== PROPERTIES ==========================

        /// <summary>Returns true if a dialogue animation is currently active.</summary>
        public bool IsAnimating => _dialogueVertexAnimator != null && _dialogueVertexAnimator.TextAnimating;

        // ======================= UNITY CALLBACKS =========================

        private void Awake()
        {
            InitializeAnimator();
            RegisterButtonListeners();
        }

        private void OnDestroy()
        {
            UnregisterButtonListeners();
            if (_typeRoutine != null)
            {
                _typeRoutine.TryKill();
                _typeRoutine = null;
            }
        }

        // ========================= PRIVATE METHODS =======================

        /// <summary>Creates a new animator instance if not already present.</summary>
        private void InitializeAnimator()
        {
            if (_dialogueVertexAnimator == null && _textBox != null)
            {
                _dialogueVertexAnimator = new DialogueVertexAnimator(_textBox, _audioSourceGroup);
            }
        }

        /// <summary>Registers click listeners for all dialogue buttons.</summary>
        private void RegisterButtonListeners()
        {
            if (_playDialogue1Button != null) _playDialogue1Button.onClick.AddListener(PlayDialogue1);
            if (_playDialogue2Button != null) _playDialogue2Button.onClick.AddListener(PlayDialogue2);
            if (_playDialogue3Button != null) _playDialogue3Button.onClick.AddListener(PlayDialogue3);
        }

        /// <summary>Removes click listeners (called on destroy).</summary>
        private void UnregisterButtonListeners()
        {
            if (_playDialogue1Button != null) _playDialogue1Button.onClick.RemoveListener(PlayDialogue1);
            if (_playDialogue2Button != null) _playDialogue2Button.onClick.RemoveListener(PlayDialogue2);
            if (_playDialogue3Button != null) _playDialogue3Button.onClick.RemoveListener(PlayDialogue3);
        }

        /// <summary>Invoked by button 1 to play first dialogue sample.</summary>
        private void PlayDialogue1() => PlayDialogue(_dialogue1);

        /// <summary>Invoked by button 2 to play second dialogue sample.</summary>
        private void PlayDialogue2() => PlayDialogue(_dialogue2);

        /// <summary>Invoked by button 3 to play third dialogue sample.</summary>
        private void PlayDialogue3() => PlayDialogue(_dialogue3);

        /// <summary>Starts the dialogue animation for the provided message.</summary>
        /// <param name="message">Full message (may contain markup tags for effects).
        /// If null or empty the call is ignored.</param>
        private void PlayDialogue(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            InitializeAnimator();
            if (_typeRoutine != null)
            {
                _typeRoutine.TryKill();
                _typeRoutine = null;
            }
            _dialogueVertexAnimator.TextAnimating = false; // reset internal state
            List<DialogueCommand> commands = DialogueUtility.ProcessInputString(message, out string totalTextMessage);
            _typeRoutine = TaskManager.StartTask(_dialogueVertexAnimator.AnimateTextIn(commands, totalTextMessage, _typingClip, null));
        }
    }
}