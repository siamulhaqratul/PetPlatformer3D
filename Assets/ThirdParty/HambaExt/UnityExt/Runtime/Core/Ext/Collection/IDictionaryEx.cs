using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace UnityExt
{
    /// <summary>
    /// Provides extension methods for IDictionary collections, supporting various key/value type combinations and null-safe operations.
    /// </summary>
    public static class IDictionaryEx
    {
        // ========================= STATIC METHODS ========================

        // ----------- Struct Vs Struct -----------

        /// <summary>Iterates over a dictionary with struct keys and values, invoking the provided action.</summary>
        public static void ExForEach_SS<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : struct
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates over a dictionary with struct keys and values, invoking the provided async function.</summary>
        public static async void ExForEach_SS<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : struct
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                await Code?.Invoke(a.Key, a.Value);
            }
        }

        // ----------- Struct Vs Class -----------

        /// <summary>Iterates over a dictionary with struct keys and class values, skipping null values.</summary>
        public static void ExForEach_SC<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : class
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates over a dictionary with struct keys and class values, skipping null values.</summary>
        public static async void ExForEach_SC<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : class
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }

        // ----------- UnityObject vs Class -----------

        /// <summary>Iterates over a dictionary with UnityEngine.Object keys and class values, skipping null keys or values.</summary>
        public static void ExForEach_UC<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized iteration for UnityEngine.Object keys and class values, using ReferenceEquals for null checks.</summary>
        public static void ExForEach_UC_Optimized<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates over a dictionary with UnityEngine.Object keys and class values, skipping null keys or values.</summary>
        public static async void ExForEach_UC<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized async iteration for UnityEngine.Object keys and class values, using ReferenceEquals for null checks.</summary>
        public static async void ExForEach_UC_Optimized<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }

        // ----------- Struct Vs UnityObject -----------

        // NonAsync

        /// <summary>Iterates over a dictionary with UnityEngine.Object keys and values, skipping null keys or values.</summary>
        public static void ExForEach_UU<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object 
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized iteration for UnityEngine.Object keys and values, using ReferenceEquals for null checks.</summary>
        public static void ExForEach_UU_Optimized<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates over a dictionary with UnityEngine.Object keys and struct values, skipping null keys.</summary>
        public static void ExForEach_US<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized iteration for UnityEngine.Object keys and struct values, using ReferenceEquals for null checks.</summary>
        public static void ExForEach_US_Optimized<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates over a dictionary with struct keys and UnityEngine.Object values, skipping null values.</summary>
        public static void ExForEach_SU<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized iteration for struct keys and UnityEngine.Object values, using ReferenceEquals for null checks.</summary>
        public static void ExForEach_SU_Optimized<T1, T2>(this IDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Value, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }

        // Async

        /// <summary>Asynchronously iterates over a dictionary with UnityEngine.Object keys and values, skipping null keys or values.</summary>
        public static async void ExForEach_UU<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized async iteration for UnityEngine.Object keys and values, using ReferenceEquals for null checks.</summary>
        public static async void ExForEach_UU_Optimized<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates over a dictionary with UnityEngine.Object keys and struct values, skipping null keys.</summary>
        public static async void ExForEach_US<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized async iteration for UnityEngine.Object keys and struct values, using ReferenceEquals for null checks.</summary>
        public static async void ExForEach_US_Optimized<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates over a dictionary with struct keys and UnityEngine.Object values, skipping null values.</summary>
        public static async void ExForEach_SU<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Optimized async iteration for struct keys and UnityEngine.Object values, using ReferenceEquals for null checks.</summary>
        public static async void ExForEach_SU_Optimized<T1, T2>(this IDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValid() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Value, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }

        /// <summary>Returns true if the dictionary is not null and contains at least one element.</summary>
        public static bool ExIsValid<T1, T2>(this IDictionary<T1, T2> data)
        {
            return data != null && data.Count > 0;
        }

        // ----------- Get NullCount -----------

        /// <summary>Counts null values in a dictionary with UnityEngine.Object keys and values.</summary>
        public static int ExNullValueCount_UU<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEach_UU((key, value) =>
            {
                if (value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts null keys in a dictionary with UnityEngine.Object keys and values.</summary>
        public static int ExNullKeyCount_UU<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEach_UU((key, value) =>
            {
                if (key == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts entries with either null key or value in a dictionary with UnityEngine.Object keys and values.</summary>
        public static int ExAnyNullCount_UU<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEach_UU((key, value) =>
            {
                if (key == null || value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts null values in a dictionary with UnityEngine.Object keys and class values.</summary>
        public static int ExNullValueCount_UC<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : class
        {
            var count = 0;
            data.ExForEach_UC((key, value) =>
            {
                if (value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts null keys in a dictionary with UnityEngine.Object keys and class values.</summary>
        public static int ExNullKeyCount_UC<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : class
        {
            var count = 0;
            data.ExForEach_UC((key, value) =>
            {
                if (key == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts entries with either null key or value in a dictionary with UnityEngine.Object keys and class values.</summary>
        public static int ExAnyNullCount_UC<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : class
        {
            var count = 0;
            data.ExForEach_UC((key, value) =>
            {
                if (key == null || value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts null values in a dictionary with struct keys and UnityEngine.Object values.</summary>
        public static int ExNullValueCount_SU<T1, T2>(this IDictionary<T1, T2> data) where T1 : struct where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEach_SU((key, value) =>
            {
                if (value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts null keys in a dictionary with UnityEngine.Object keys and struct values.</summary>
        public static int ExAnyNullCount_US<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : struct
        {
            var count = 0;
            data.ExForEach_US((key, value) =>
            {
                if (key == null) { count++; }
            });
            return count;
        }

        // ----------- Join -----------

        /// <summary>Merges another dictionary into this one, overwriting or adding entries (struct keys/values).</summary>
        public static void ExJoin_SS<T1, T2>(this IDictionary<T1, T2> data, IDictionary<T1, T2> toAdd) where T1 : struct where T2 : struct
        {
            toAdd.ExForEach_SS((key, value) =>
            {
                if (data.ContainsKey(key)) { data[key] = value; }
                else { data.Add(key, value); }
            });
        }
        /// <summary>Merges another dictionary into this one, overwriting or adding entries (struct key, UnityEngine.Object value).</summary>
        public static void ExJoin_SU<T1, T2>(this IDictionary<T1, T2> data1, IDictionary<T1, T2> toAdd) where T1 : struct where T2 : UnityEngine.Object
        {
            toAdd.ExForEach_SU((key, value) =>
            {
                if (data1.ContainsKey(key)) { data1[key] = value; }
                else { data1.Add(key, value); }
            });
        }
        /// <summary>Merges another dictionary into this one, overwriting or adding entries (UnityEngine.Object key, struct value).</summary>
        public static void ExJoin_US<T1, T2>(this IDictionary<T1, T2> data, IDictionary<T1, T2> toAdd) where T1 : UnityEngine.Object where T2 : struct
        {
            toAdd.ExForEach_US((key, value) =>
            {
                if (data.ContainsKey(key)) { data[key] = value; }
                else { data.Add(key, value); }
            });
        }
        /// <summary>Merges another dictionary into this one, overwriting or adding entries (struct key, class value).</summary>
        public static void ExJoin_SC<T1, T2>(this IDictionary<T1, T2> data, IDictionary<T1, T2> toAdd) where T1 : struct where T2 : class
        {
            toAdd.ExForEach_SC((key, value) =>
            {
                if (data.ContainsKey(key)) { data[key] = value; }
                else { data.Add(key, value); }
            });
        }
        /// <summary>Merges another dictionary into this one, overwriting or adding entries (UnityEngine.Object key, class value).</summary>
        public static void ExJoin_UC<T1, T2>(this IDictionary<T1, T2> data, IDictionary<T1, T2> toAdd) where T1 : UnityEngine.Object where T2 : class
        {
            toAdd.ExForEach_UC((key, value) =>
            {
                if (data.ContainsKey(key)) { data[key] = value; }
                else { data.Add(key, value); }
            });
        }

        // ----------- RemoveNullsOnIt -----------

        /// <summary>Removes entries with null keys or values from a dictionary (UnityEngine.Object key, class value).</summary>
        public static void ExRemoveNullsOnIt_UC<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : class
        {
            var validData = new Dictionary<T1, T2>();
            data.ExForEach_UC((key, value) =>
            {
                if (key != null && value != null)
                {
                    if (validData.ContainsKey(key)) { validData[key] = value; }
                    else { validData.Add(key, value); }
                }
            });
            data.Clear();
            validData.ExForEach_UC((key, value) =>
            {
                data.Add(key, value);
            });
        }
        /// <summary>Removes entries with null keys from a dictionary (UnityEngine.Object key, struct value).</summary>
        public static void ExRemoveNullsOnIt_US<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : struct
        {
            var validData = new Dictionary<T1, T2>();
            data.ExForEach_US((key, value) =>
            {
                if (key != null)
                {
                    if (validData.ContainsKey(key)) { validData[key] = value; }
                    else { validData.Add(key, value); }
                }
            });
            data.Clear();
            validData.ExForEach_US((key, value) =>
            {
                data.Add(key, value);
            });
        }
        /// <summary>Removes entries with null values from a dictionary (struct key, UnityEngine.Object value).</summary>
        public static void ExRemoveNullsOnIt_SU<T1, T2>(this IDictionary<T1, T2> data) where T1 : struct where T2 : UnityEngine.Object
        {
            var validData = new Dictionary<T1, T2>();
            data.ExForEach_SU((key, value) =>
            {
                if (value != null)
                {
                    if (validData.ContainsKey(key)) { validData[key] = value; }
                    else { validData.Add(key, value); }
                }
            });
            data.Clear();
            validData.ExForEach_SU((key, value) =>
            {
                data.Add(key, value);
            });
        }
        /// <summary>Removes entries with null values from a dictionary (struct key, class value).</summary>
        public static void ExRemoveNullsOnIt_SC<T1, T2>(this IDictionary<T1, T2> data) where T1 : struct where T2 : class
        {
            var validData = new Dictionary<T1, T2>();
            data.ExForEach_SC((key, value) =>
            {
                if (value != null)
                {
                    if (validData.ContainsKey(key)) { validData[key] = value; }
                    else { validData.Add(key, value); }
                }
            });
            data.Clear();
            validData.ExForEach_SC((key, value) =>
            {
                data.Add(key, value);
            });
        }
        /// <summary>Removes entries with null keys or values from a dictionary (UnityEngine.Object key, UnityEngine.Object value).</summary>
        public static void ExRemoveNullsOnIt_UU<T1, T2>(this IDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            var validData = new Dictionary<T1, T2>();
            data.ExForEach_UU((key, value) =>
            {
                if (key != null && value != null)
                {
                    if (validData.ContainsKey(key)) { validData[key] = value; }
                    else { validData.Add(key, value); }
                }
            });
            data.Clear();
            validData.ExForEach_UU((key, value) =>
            {
                data.Add(key, value);
            });
        }
    }
}