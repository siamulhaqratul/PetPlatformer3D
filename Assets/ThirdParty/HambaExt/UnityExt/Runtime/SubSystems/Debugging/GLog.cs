using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Debugging
{
        /// <summary>Development-only logging and validation helpers (wrapped in DEBUG/EDITOR guards) supporting colored output, assertions, and runtime checks.</summary>
    public static class GLog
    {
                // ========================= PUBLIC METHODS ========================
        #region Colored Prints
                /// <summary>Prints a colored message to the Console (development/editor only).</summary>
                /// <param name="msg">Message content.</param>
                /// <param name="color">Text color.</param>
                /// <param name="highlightObject">Optional context object for pinging/log grouping.</param>
        public static void Print(string msg, Color color, UnityEngine.Object highlightObject = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            //var colHex = ColorUtility.ToHtmlStringRGB(color);
            //Print($"<color=" + colHex + ">" + msg + "</color>");
            var ltxt = string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(color.r * 255f),
                   (byte)(color.g * 255f), (byte)(color.b * 255f), msg);
            if (highlightObject == null) { Debug.Log(ltxt); }
            else { Debug.Log(ltxt, highlightObject); }
#endif
        }
        /// <summary>Prints a colored error message (development/editor only).</summary>
        /// <param name="msg">Message content.</param>
        /// <param name="color">Text color.</param>
        /// <param name="highlightObject">Optional context object.</param>
        public static void PrintError(string msg, Color color, UnityEngine.Object highlightObject = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            //var colHex = ColorUtility.ToHtmlStringRGB(color);
            //Print($"<color=" + colHex + ">" + msg + "</color>");
            var ltxt = string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(color.r * 255f),
                   (byte)(color.g * 255f), (byte)(color.b * 255f), msg);
            if (highlightObject == null) { Debug.LogError(ltxt); }
            else { Debug.LogError(ltxt, highlightObject); }     
#endif
        }
        /// <summary>Prints a colored warning message (development/editor only).</summary>
        /// <param name="msg">Message content.</param>
        /// <param name="color">Text color.</param>
        /// <param name="highlightObject">Optional context object.</param>
        public static void PrintWarning(string msg, Color color, UnityEngine.Object highlightObject = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            //var colHex = ColorUtility.ToHtmlStringRGB(color);
            //Print($"<color=" + colHex + ">" + msg + "</color>");
            var ltxt = string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(color.r * 255f),
                   (byte)(color.g * 255f), (byte)(color.b * 255f), msg);
            if (highlightObject == null) { Debug.LogWarning(ltxt); }
            else { Debug.LogWarning(ltxt, highlightObject); }
#endif
        }
        #endregion
        #region Prints
        /// <summary>Prints a plain message.</summary>
        /// <param name="msg">Message content.</param>
        /// <param name="highlightObject">Optional context object.</param>
        public static void Print(string msg, UnityEngine.Object highlightObject = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (highlightObject == null) { Debug.Log(msg); }
            else { Debug.Log(msg, highlightObject); }
#endif
        }
        /// <summary>Prints an error message.</summary>
        /// <param name="msg">Message content.</param>
        /// <param name="highlightObject">Optional context object.</param>
        public static void PrintError(string msg, UnityEngine.Object highlightObject = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (highlightObject == null) { Debug.LogError(msg); }
            else { Debug.LogError(msg, highlightObject); }
#endif
        }
        /// <summary>Prints a warning message.</summary>
        /// <param name="msg">Message content.</param>
        /// <param name="highlightObject">Optional context object.</param>
        public static void PrintWarning(string msg, UnityEngine.Object highlightObject = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (highlightObject == null) { Debug.LogWarning(msg); }
            else { Debug.LogWarning(msg, highlightObject); }
#endif
        }
        #endregion
        /// <summary>Throws an exception if save data flag is off (development/editor only).</summary>
        /// <param name="isSaveDataOn">Whether save data system is enabled.</param>
        /// <param name="additionalMessage">Extra context info.</param>
        /// <param name="script">Optional script instance.</param>
        public static void CheckSaveData(bool isSaveDataOn, string additionalMessage = "", MonoBehaviour script = null)
        {
            if (!isSaveDataOn)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                GLog.Throw("You need to turn on 'data saving' in the related script: " + (script == null ? "NOT specified!" : script.GetType().Name) + Environment.NewLine +
                    " And script instance name: "+ (script == null ? "NOT specified!" : script.gameObject.name) + Environment.NewLine +
                    " And script instance in unity hierarchy: " + (script == null ? "NOT specified!" : script.transform.ExGetHierarchy()) + Environment.NewLine +
                    " and additional message: " + additionalMessage);
#endif
            }
        }

        /// <summary>Throws an exception (development/editor only).</summary>
        /// <param name="msg">Exception message.</param>
        public static void Throw(string msg)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            throw new System.Exception(msg);
#endif
        }
        /// <summary>Throws an <see cref="ArgumentException"/> (development/editor only).</summary>
        /// <param name="msg">Exception message.</param>
        /// <param name="parameterName">Name of invalid parameter.</param>
        public static void ThrowArgument(string msg, string parameterName)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            throw new System.ArgumentException(msg, parameterName);
#endif
        }
        /// <summary>Validates a UnityEngine.Object reference (throws if null in development/editor).</summary>
        /// <param name="obj">Object to test.</param>
        /// <param name="objName">Name used in error message.</param>
        /// <param name="additionalInfo">Extra context information.</param>
        /// <param name="optionalTransform">Optional transform for hierarchy path.</param>
        public static void CheckUnityObjectForAssignment(this UnityEngine.Object obj, string objName, string additionalInfo = "", Transform optionalTransform = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (obj == null)
            {
                var str = objName + " is not assigned!" + Environment.NewLine;
                str += "AdditionalInfo: " + additionalInfo + Environment.NewLine;
                str += optionalTransform == null ? "" : "Affected object located in the hierarchy: " + optionalTransform.ExGetHierarchy();
                GLog.Throw(str);
            }
#endif
        }
        /// <summary>Validates an object reference (throws if null in development/editor).</summary>
        /// <param name="obj">Object reference to validate.</param>
        /// <param name="objName">Name used in error message.</param>
        /// <param name="additionalInfo">Extra context.</param>
        /// <param name="optionalTransform">Optional transform for hierarchy path.</param>
        public static void CheckObjectForAssignment(this object obj, string objName, string additionalInfo = "", Transform optionalTransform = null)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (obj == null)
            {
                var str = objName + " is null!" + Environment.NewLine;
                str += "AdditionalInfo: " + additionalInfo + Environment.NewLine;
                str += optionalTransform == null ? "" : "Affected object located in the hierarchy: " + optionalTransform.ExGetHierarchy();
                GLog.Throw(str);
            }
#endif
        }
        /// <summary>Validates a collection (non-null, non-empty, no null elements).</summary>
        /// <typeparam name="T">Element reference type.</typeparam>
        /// <param name="list">List to validate.</param>
        /// <param name="objName">Collection name for diagnostics.</param>
        /// <param name="additionalInfo">Extra context info.</param>
        /// <param name="optionalTransform">Optional transform for hierarchy path.</param>
        public static void CheckCollection<T>(this List<T> list, string objName, string additionalInfo = "", Transform optionalTransform = null) where T : class
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            var invalid = !list.ExIsValid();
            var anyNull = list.ExHasAnyNull();
            if (invalid || anyNull)
            {
                var str = "";
                if (invalid) { str = "Collection named: '" + objName + "' is not valid!" + Environment.NewLine; }
                else if (anyNull) { str = "Collection named: '" + objName + "' has null element!" + Environment.NewLine; }
                str += "AdditionalInfo: " + additionalInfo + Environment.NewLine;
                str += optionalTransform == null ? "" : "Affected object located in the hierarchy: " + optionalTransform.ExGetHierarchy();
                GLog.Throw(str);
            }
#endif
        }
        /// <summary>Triggers a debug break (development/editor only).</summary>
        public static void ExecBreak()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Break();
#endif
        }
        /// <summary>Asserts condition (throws if condition is true; inverted semantics).</summary>
        /// <param name="isTrue">Condition: if true an exception is thrown (debug/editor only).</param>
        /// <param name="msg">Message for exception.</param>
        public static void Assert(bool isTrue, string msg)
        {
            if (isTrue == false) { return; }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            throw new System.Exception(msg);
#endif
        }
        /// <summary>Executes logic safely, catching exceptions in development/editor builds.</summary>
        /// <param name="logic">Action to run.</param>
        public static void CheckCode(OnDoAnything logic)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            try
            {
                logic.Invoke();
            }
            catch (System.Exception ex)
            {
                Throw(ex.Message);
            }
#else
            logic?.Invoke();
#endif
        }
    }
}