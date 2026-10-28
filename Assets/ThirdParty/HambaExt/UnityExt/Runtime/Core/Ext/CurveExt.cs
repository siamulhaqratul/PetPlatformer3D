using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Extension methods for <see cref="AnimationCurve"/> utilities, including min/max time and value extraction.
    /// </summary>
    public static class CurveExt
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Extracts the minimum and maximum time and value from all keys in the given <see cref="AnimationCurve"/>.
        /// </summary>
        /// <param name="curve">The animation curve to analyze.</param>
        /// <param name="minTime">Receives the minimum key time found.</param>
        /// <param name="maxTime">Receives the maximum key time found.</param>
        /// <param name="minValue">Receives the minimum key value found.</param>
        /// <param name="maxValue">Receives the maximum key value found.</param>
        public static void ExGetCurveUltima(this AnimationCurve curve, ref float minTime, ref float maxTime, ref float minValue, ref float maxValue)
        {
            var keys = curve.keys;
            float min_t = float.MaxValue, min_v = float.MaxValue, max_t = float.MinValue, max_v = float.MinValue;
            if(keys.ExIsValid())
            {
                for (int i = 0; i < curve.length; i++)
                {
                    var key = keys[i];
                    if (key.time < min_t)
                    {
                        min_t = key.time;
                    }
                    if (key.time > max_t)
                    {
                        max_t = key.time;
                    }
                    if (key.value < min_v)
                    {
                        min_v = key.value;
                    }
                    if (key.value > max_v)
                    {
                        max_v = key.value;
                    }
                }
                minTime = min_t;
                maxTime = max_t;
                minValue= min_v;
                maxValue= max_v;
            }
        }
    }
}