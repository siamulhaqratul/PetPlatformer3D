using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Sound
{
    internal static class SoundExt
    {
        internal static T EnsureComponent<T>(this GameObject obj) where T : UnityEngine.Component
        {
            T result = obj.GetComponent<T>();

            if (result == null)
            {
                result = obj.AddComponent<T>();
            }
            return result;
        }

        internal static T GetDataWithName<T>(this List<T> entries, string name) where T : ISoundData
        {
            T result = default;
            if (entries != null && entries.Count > 0)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (e == null || e.DataName != name) { continue; }
                    result = e;
                    break;
                }
            }
            return result;
        }

        internal static void ExForEach<T>(this List<T> entries, System.Action<T> Code) where T : class
        {
            if (entries != null && entries.Count > 0)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (e == null) { continue; }
                    Code?.Invoke(e);
                }
            }
        }
    }
}