using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace UnityExt.TextMeshAnimator
{
    /// <summary>Parses dialogue markup into executable commands (pauses, speed changes, animations) and returns a cleaned text string.</summary>
    public class DialogueUtility : MonoBehaviour
    {
        // =========================== CONSTANTS ===========================

        // Remainder of the tag content up to closing angle or end.
        private const string REMAINDER_REGEX = "(.*?((?=>)|(/|$)))";
        private const string PAUSE_REGEX_STRING = "<p:(?<pause>" + REMAINDER_REGEX + ")>";
        private const string SPEED_REGEX_STRING = "<sp:(?<speed>" + REMAINDER_REGEX + ")>";
        private const string ANIM_START_REGEX_STRING = "<anim:(?<anim>" + REMAINDER_REGEX + ")>";
        private const string ANIM_END_REGEX_STRING = "</anim>";

        // ======================= STATIC FIELDS ==========================

        private static readonly Regex _pauseRegex = new Regex(PAUSE_REGEX_STRING);
        private static readonly Regex _speedRegex = new Regex(SPEED_REGEX_STRING);
        private static readonly Regex _animStartRegex = new Regex(ANIM_START_REGEX_STRING);
        private static readonly Regex _animEndRegex = new Regex(ANIM_END_REGEX_STRING);
        private static readonly Dictionary<string, float> _pauseDictionary = new()
        {
            { "tiny", .1f },
            { "short", .25f },
            { "normal", 0.666f },
            { "long", 1f },
            { "read", 2f },
        };

        // ========================= PUBLIC METHODS ========================

        /// <summary>Parses markup tags in the input message and returns a list of dialogue commands while producing a tag-stripped output string.</summary>
        /// <param name="message">Input string containing dialogue markup.</param>
        /// <param name="processedMessage">Output string with markup tags removed.</param>
        /// <returns>List of extracted <see cref="DialogueCommand"/> entries in order of appearance.</returns>
        public static List<DialogueCommand> ProcessInputString(string message, out string processedMessage)
        {
            var result = new List<DialogueCommand>();
            processedMessage = message;

            processedMessage = HandlePauseTags(processedMessage, result);
            processedMessage = HandleSpeedTags(processedMessage, result);
            processedMessage = HandleAnimStartTags(processedMessage, result);
            processedMessage = HandleAnimEndTags(processedMessage, result);

            return result;
        }

        // ========================= PRIVATE METHODS =======================

        /// <summary>Processes animation end tags and appends corresponding commands.</summary>
        private static string HandleAnimEndTags(string processedMessage, List<DialogueCommand> result)
        {
            var animEndMatches = _animEndRegex.Matches(processedMessage);
            foreach (Match match in animEndMatches)
            {
                result.Add(new DialogueCommand
                {
                    position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                    type = DialogueCommandType.AnimEnd,
                });
            }
            processedMessage = Regex.Replace(processedMessage, ANIM_END_REGEX_STRING, string.Empty);
            return processedMessage;
        }

        /// <summary>Processes animation start tags and appends corresponding commands.</summary>
        private static string HandleAnimStartTags(string processedMessage, List<DialogueCommand> result)
        {
            var animStartMatches = _animStartRegex.Matches(processedMessage);
            foreach (Match match in animStartMatches)
            {
                var stringVal = match.Groups["anim"].Value;
                result.Add(new DialogueCommand
                {
                    position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                    type = DialogueCommandType.AnimStart,
                    textAnimValue = GetTextAnimationType(stringVal)
                });
            }
            processedMessage = Regex.Replace(processedMessage, ANIM_START_REGEX_STRING, string.Empty);
            return processedMessage;
        }

        /// <summary>Processes speed change tags and appends corresponding commands.</summary>
        private static string HandleSpeedTags(string processedMessage, List<DialogueCommand> result)
        {
            var speedMatches = _speedRegex.Matches(processedMessage);
            foreach (Match match in speedMatches)
            {
                var stringVal = match.Groups["speed"].Value;
                if (!float.TryParse(stringVal, out var val))
                {
                    val = 150f;
                }
                result.Add(new DialogueCommand
                {
                    position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                    type = DialogueCommandType.TextSpeedChange,
                    floatValue = val
                });
            }
            processedMessage = Regex.Replace(processedMessage, SPEED_REGEX_STRING, string.Empty);
            return processedMessage;
        }

        /// <summary>Processes pause tags and appends corresponding commands.</summary>
        private static string HandlePauseTags(string processedMessage, List<DialogueCommand> result)
        {
            var pauseMatches = _pauseRegex.Matches(processedMessage);
            foreach (Match match in pauseMatches)
            {
                var pauseName = match.Groups["pause"].Value;
                Debug.Assert(_pauseDictionary.ContainsKey(pauseName), "No pause registered for '" + pauseName + "'");
                if (_pauseDictionary.TryGetValue(pauseName, out var dur))
                {
                    result.Add(new DialogueCommand
                    {
                        position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                        type = DialogueCommandType.Pause,
                        floatValue = dur
                    });
                }
            }
            processedMessage = Regex.Replace(processedMessage, PAUSE_REGEX_STRING, string.Empty);
            return processedMessage;
        }

        /// <summary>Attempts to parse a text animation type name into the corresponding enum.</summary>
        private static TextAnimationType GetTextAnimationType(string stringVal)
        {
            TextAnimationType result;
            try
            {
                result = (TextAnimationType)Enum.Parse(typeof(TextAnimationType), stringVal, true);
            }
            catch (ArgumentException)
            {
                Debug.LogError("Invalid Text Animation Type: " + stringVal);
                result = TextAnimationType.none;
            }
            return result;
        }

        /// <summary>Counts visible (non-tag) characters up to the specified index, accounting for inline tags.</summary>
        private static int VisibleCharactersUpToIndex(string message, int index)
        {
            var result = 0;
            var insideBrackets = false;
            for (var i = 0; i < index; i++)
            {
                if (message[i] == '<')
                {
                    insideBrackets = true;
                }
                else if (message[i] == '>')
                {
                    insideBrackets = false;
                    result--;
                }
                if (!insideBrackets)
                {
                    result++;
                }
                else if (i + 6 < index && message.Substring(i, 6) == "sprite")
                {
                    result++;
                }
            }
            return result;
        }
    }

    /// <summary>Command extracted from dialogue markup representing timing, animation, or speed changes.</summary>
    public struct DialogueCommand
    {
        // Character index in visible text where the command occurs.
        public int position;
        // Type of dialogue command.
        public DialogueCommandType type;
        // Optional numeric value (pause duration or speed value).
        public float floatValue;
        // Optional string value (unused presently but reserved for future tags).
        public string stringValue;
        // Text animation enum value for animation start commands.
        public TextAnimationType textAnimValue;
    }

    /// <summary>Supported dialogue command operation types.</summary>
    public enum DialogueCommandType
    {
        /// <summary>Insert a timed pause before continuing.</summary>
        Pause,
        /// <summary>Change the text reveal speed.</summary>
        TextSpeedChange,
        /// <summary>Begin a text animation span.</summary>
        AnimStart,
        /// <summary>End a text animation span.</summary>
        AnimEnd
    }

    /// <summary>Supported inline text animation styles.</summary>
    public enum TextAnimationType
    {
        /// <summary>No animation.</summary>
        none,
        /// <summary>Shake characters.</summary>
        shake,
        /// <summary>Wave characters.</summary>
        wave
    }
}