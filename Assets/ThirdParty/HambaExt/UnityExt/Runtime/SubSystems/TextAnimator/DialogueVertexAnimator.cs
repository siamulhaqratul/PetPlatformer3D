using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UnityExt.TextMeshAnimator
{
    /// <summary>Performs per-character animated reveal of a TMP text object with support for pauses, speed changes, and inline animations.</summary>
    public class DialogueVertexAnimator
    {
        // =========================== CONSTANTS ===========================

        private const float CHAR_ANIM_TIME = 0.07f;
        private const float NOISE_MAGNITUDE_ADJUSTMENT = 0.06f;
        private const float NOISE_FREQUENCY_ADJUSTMENT = 15f;
        private const float WAVE_MAGNITUDE_ADJUSTMENT = 0.06f;

        // ======================= STATIC FIELDS ==========================

        private static readonly Color32 _clear = new Color32(0, 0, 0, 0);
        private static readonly Vector3 _vecZero = Vector3.zero;

        // ========================= READONLY FIELDS ======================

        private readonly TMP_Text _textBox;
        private readonly float _textAnimationScale; // (currently unused externally but kept for future scaling logic)
        private readonly AudioSourceGroup _audioSourceGroup;

        // =================== PRIVATE/PROTECTED FIELDS ====================

        private bool _textAnimating;
        private bool _stopAnimating;
        private float _timeUntilNextDialogueSound;
        private float _lastDialogueSound;

        // =========================== PROPERTIES ==========================

        /// <summary>Indicates whether the animator is currently revealing text.</summary>
        public bool TextAnimating
        {
            get => _textAnimating;
            set => _textAnimating = value;
        }

        // ========================= INITIALIZE METHODS ========================

        /// <summary>Creates a new dialogue vertex animator bound to a TMP text box and optional audio source group.</summary>
        /// <param name="textBox">Target text object to animate.</param>
        /// <param name="audioSourceGroup">Optional audio source group for per-character sounds.</param>
        public DialogueVertexAnimator(TMP_Text textBox, AudioSourceGroup audioSourceGroup)
        {
            _textBox = textBox;
            _audioSourceGroup = audioSourceGroup;
            _textAnimationScale = _textBox.fontSize;
        }

        // ========================= PUBLIC METHODS ========================

        /// <summary>Animates the processed message in, yielding each frame until completion or skip.</summary>
        /// <param name="commands">List of dialogue commands produced by markup parsing.</param>
        /// <param name="processedMessage">Message text with tags removed.</param>
        /// <param name="voiceClip">Optional audio clip to play intermittently during reveal.</param>
        /// <param name="onFinish">Callback invoked when reveal completes.</param>
        /// <param name="speedMultiplier">Speed multiplier for reveal (higher is faster).</param>
        /// <returns>Coroutine enumerator.</returns>
        public IEnumerator AnimateTextIn(List<DialogueCommand> commands, string processedMessage, AudioClip voiceClip, Action onFinish, float speedMultiplier = 1.0f)
        {
            _textAnimating = true;
            float secondsPerCharacter = 0.0015f * (1 / speedMultiplier);
            float timeOfLastCharacter = 0f;

            var textAnimInfo = SeparateOutTextAnimInfo(commands);
            TMP_TextInfo textInfo = _textBox.textInfo;

            // Clear mesh vertices so text is invisible initially.
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfer = textInfo.meshInfo[i];
                if (meshInfer.vertices == null) continue;
                for (int j = 0; j < meshInfer.vertices.Length; j++)
                {
                    meshInfer.vertices[j] = _vecZero;
                }
            }

            _textBox.text = processedMessage;
            _textBox.ForceMeshUpdate();
            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

            var originalColors = new Color32[textInfo.meshInfo.Length][];
            for (int i = 0; i < originalColors.Length; i++)
            {
                var colors = textInfo.meshInfo[i].colors32;
                originalColors[i] = new Color32[colors.Length];
                Array.Copy(colors, originalColors[i], colors.Length);
            }

            int charCount = textInfo.characterCount;
            var charAnimStartTimes = new float[charCount];
            for (int i = 0; i < charCount; i++) charAnimStartTimes[i] = -1f; // not yet started

            int visibleCharacterIndex = 0;
            while (true)
            {
                if (_stopAnimating)
                {
                    for (int i = visibleCharacterIndex; i < charCount; i++)
                    {
                        charAnimStartTimes[i] = Time.unscaledTime;
                    }
                    visibleCharacterIndex = charCount;
                    FinishAnimating(onFinish);
                }

                if (ShouldShowNextCharacter(secondsPerCharacter, timeOfLastCharacter))
                {
                    if (visibleCharacterIndex <= charCount)
                    {
                        ExecuteCommandsForCurrentIndex(commands, visibleCharacterIndex, ref secondsPerCharacter, ref timeOfLastCharacter);
                        if (visibleCharacterIndex < charCount && ShouldShowNextCharacter(secondsPerCharacter, timeOfLastCharacter))
                        {
                            charAnimStartTimes[visibleCharacterIndex] = Time.unscaledTime;
                            if (voiceClip != null)
                            {
                                PlayDialogueSound(voiceClip);
                            }
                            visibleCharacterIndex++;
                            timeOfLastCharacter = Time.unscaledTime;
                            if (visibleCharacterIndex == charCount)
                            {
                                FinishAnimating(onFinish);
                            }
                        }
                    }
                }

                for (int j = 0; j < charCount; j++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[j];
                    if (!charInfo.isVisible) continue; // skip invisible

                    int vertexIndex = charInfo.vertexIndex;
                    int materialIndex = charInfo.materialReferenceIndex;
                    var destinationColors = textInfo.meshInfo[materialIndex].colors32;
                    Color32 theColor = j < visibleCharacterIndex ? originalColors[materialIndex][vertexIndex] : _clear;
                    destinationColors[vertexIndex + 0] = theColor;
                    destinationColors[vertexIndex + 1] = theColor;
                    destinationColors[vertexIndex + 2] = theColor;
                    destinationColors[vertexIndex + 3] = theColor;

                    Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                    Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                    float charSize = 0f;
                    float charAnimStartTime = charAnimStartTimes[j];
                    if (charAnimStartTime >= 0f)
                    {
                        float timeSinceAnimStart = Time.unscaledTime - charAnimStartTime;
                        charSize = Mathf.Min(1f, timeSinceAnimStart / CHAR_ANIM_TIME);
                    }

                    Vector3 animPosAdjustment = GetAnimPosAdjustment(textAnimInfo, j, _textBox.fontSize, Time.unscaledTime);
                    Vector3 offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2f;
                    destinationVertices[vertexIndex + 0] = ((sourceVertices[vertexIndex + 0] - offset) * charSize) + offset + animPosAdjustment;
                    destinationVertices[vertexIndex + 1] = ((sourceVertices[vertexIndex + 1] - offset) * charSize) + offset + animPosAdjustment;
                    destinationVertices[vertexIndex + 2] = ((sourceVertices[vertexIndex + 2] - offset) * charSize) + offset + animPosAdjustment;
                    destinationVertices[vertexIndex + 3] = ((sourceVertices[vertexIndex + 3] - offset) * charSize) + offset + animPosAdjustment;
                }

                _textBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    TMP_MeshInfo theInfo = textInfo.meshInfo[i];
                    theInfo.mesh.vertices = theInfo.vertices;
                    _textBox.UpdateGeometry(theInfo.mesh, i);
                }
                yield return null;
            }
        }

        /// <summary>Skips the current animation and reveals all remaining characters instantly.</summary>
        public void SkipToEndOfCurrentMessage()
        {
            _textAnimating = false;
            _stopAnimating = false;
        }

        // ========================= PRIVATE METHODS =======================

        /// <summary>Executes queued commands whose position matches the current visible index.</summary>
        private void ExecuteCommandsForCurrentIndex(List<DialogueCommand> commands, int visibleCharacterIndex, ref float secondsPerCharacter, ref float timeOfLastCharacter)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                DialogueCommand command = commands[i];
                if (command.position != visibleCharacterIndex) continue;
                switch (command.type)
                {
                    case DialogueCommandType.Pause:
                        timeOfLastCharacter = Time.unscaledTime + command.floatValue;
                        break;
                    case DialogueCommandType.TextSpeedChange:
                        secondsPerCharacter = 1f / command.floatValue;
                        break;
                }
                commands.RemoveAt(i);
                i--;
            }
        }

        /// <summary>Finalizes state after animation completes or is skipped.</summary>
        private void FinishAnimating(Action onFinish)
        {
            _textAnimating = false;
            _stopAnimating = false;
            onFinish?.Invoke();
        }

        /// <summary>Computes positional animation offset for a character based on active animation spans.</summary>
        private Vector3 GetAnimPosAdjustment(TextAnimInfo[] textAnimInfo, int charIndex, float fontSize, float time)
        {
            float x = 0f;
            float y = 0f;
            for (int i = 0; i < textAnimInfo.Length; i++)
            {
                TextAnimInfo info = textAnimInfo[i];
                if (charIndex < info.startIndex || charIndex >= info.endIndex) continue;
                if (info.type == TextAnimationType.shake)
                {
                    float scaleAdjust = fontSize * NOISE_MAGNITUDE_ADJUSTMENT;
                    x += (Mathf.PerlinNoise((charIndex + time) * NOISE_FREQUENCY_ADJUSTMENT, 0f) - 0.5f) * scaleAdjust;
                    y += (Mathf.PerlinNoise((charIndex + time) * NOISE_FREQUENCY_ADJUSTMENT, 1000f) - 0.5f) * scaleAdjust;
                }
                else if (info.type == TextAnimationType.wave)
                {
                    y += Mathf.Sin((charIndex * 1.5f) + (time * 6f)) * fontSize * WAVE_MAGNITUDE_ADJUSTMENT;
                }
            }
            return new Vector3(x, y, 0f);
        }

        /// <summary>Returns true when the elapsed time since last character exceeds the per-character delay.</summary>
        private static bool ShouldShowNextCharacter(float secondsPerCharacter, float timeOfLastCharacter)
        {
            return (Time.unscaledTime - timeOfLastCharacter) > secondsPerCharacter;
        }

        /// <summary>Plays an intermittent dialogue sound using a round-robin audio source group.</summary>
        private void PlayDialogueSound(AudioClip voiceClip)
        {
            if (Time.unscaledTime - _lastDialogueSound > _timeUntilNextDialogueSound)
            {
                _timeUntilNextDialogueSound = UnityEngine.Random.Range(0.02f, 0.08f);
                _lastDialogueSound = Time.unscaledTime;
                _audioSourceGroup?.PlayFromNextSource(voiceClip); // Multiple sources enable overlap.
            }
        }

        /// <summary>Extracts animation span info from start / end command pairs and removes them from command list.</summary>
        private TextAnimInfo[] SeparateOutTextAnimInfo(List<DialogueCommand> commands)
        {
            var tempResult = new List<TextAnimInfo>();
            var animStartCommands = new List<DialogueCommand>();
            var animEndCommands = new List<DialogueCommand>();

            for (int i = 0; i < commands.Count; i++)
            {
                DialogueCommand command = commands[i];
                if (command.type == DialogueCommandType.AnimStart)
                {
                    animStartCommands.Add(command);
                    commands.RemoveAt(i);
                    i--;
                }
                else if (command.type == DialogueCommandType.AnimEnd)
                {
                    animEndCommands.Add(command);
                    commands.RemoveAt(i);
                    i--;
                }
            }

            if (animStartCommands.Count != animEndCommands.Count)
            {
                Debug.LogError("Unequal number of start and end animation commands. Start Commands: " + animStartCommands.Count + " End Commands: " + animEndCommands.Count);
            }
            else
            {
                for (int i = 0; i < animStartCommands.Count; i++)
                {
                    DialogueCommand startCommand = animStartCommands[i];
                    DialogueCommand endCommand = animEndCommands[i];
                    tempResult.Add(new TextAnimInfo
                    {
                        startIndex = startCommand.position,
                        endIndex = endCommand.position,
                        type = startCommand.textAnimValue
                    });
                }
            }
            return tempResult.ToArray();
        }
    }

    /// <summary>Represents an animation span (start-end indices) and type used to offset characters.</summary>
    public struct TextAnimInfo
    {
        /// <summary>Inclusive start index.</summary>
        public int startIndex;
        /// <summary>Exclusive end index.</summary>
        public int endIndex;
        /// <summary>Animation type applied to characters in the span.</summary>
        public TextAnimationType type;
    }
}