using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace UnityExt
{
    /// <summary>Extension helpers for <see cref="IReadOnlyDictionary{TKey, TValue}"/> enabling safe iteration across different key/value Unity + struct + class combinations and providing null counting utilities.</summary>
    public static class IReadOnlyDictionaryEx
    {
        // ========================= PUBLIC STATIC METHODS ========================
        #region Struct Vs Struct
        /// <summary>Iterates all key/value pairs (both struct) invoking the supplied delegate.</summary>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Struct value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_SS<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : struct
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates all key/value pairs (both struct) invoking the supplied delegate.</summary>
        /// <remarks>Returns immediately (fire-and-forget) because it uses <c>async void</c>. Consider providing a <c>Task</c>-returning overload for awaited flows.</remarks>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Struct value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_SS<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : struct
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        #endregion

        #region Struct Vs Class
        /// <summary>Iterates struct key to non-null class value pairs invoking delegate.</summary>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_SC<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : class
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates struct key to non-null class value pairs invoking delegate.</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_SC<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : class
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        #endregion

        #region UnityObject vs Class
        /// <summary>Iterates non-null Unity key / non-null class value pairs invoking delegate.</summary>
        /// <typeparam name="T1">Unity object key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_UC<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates non-null Unity key / non-null class value pairs using ReferenceEquals checks (micro-optimized).</summary>
        /// <typeparam name="T1">Unity object key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_UC_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity key / non-null class value pairs invoking delegate.</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Unity object key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_UC<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity key / non-null class value pairs using ReferenceEquals checks.</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Unity object key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_UC_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : class
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        #endregion

        #region Struct Vs UnityObject
        #region NonAsync
        /// <summary>Iterates non-null Unity key / non-null Unity value pairs invoking delegate.</summary>
        /// <typeparam name="T1">Unity object key type.</typeparam>
        /// <typeparam name="T2">Unity object value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_UU<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object 
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates non-null Unity key/value pairs (ReferenceEquals optimization).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_UU_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates non-null Unity key to struct value pairs invoking delegate.</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Struct value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_US<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates non-null Unity key to struct value pairs (ReferenceEquals optimization).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Struct value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_US_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates struct key to non-null Unity value pairs invoking delegate.</summary>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_SU<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Iterates struct key to non-null Unity value pairs (ReferenceEquals optimization).</summary>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Delegate invoked with (key, value).</param>
        public static void ExForEachRO_SU_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, OnDoAnything<T1, T2> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Value, null)) { continue; }
                Code?.Invoke(a.Key, a.Value);
            }
        }
        #endregion

        #region Async
        /// <summary>Asynchronously iterates non-null Unity key/value pairs invoking delegate.</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_UU<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null || a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity key/value pairs (ReferenceEquals optimization).</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_UU_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object
            where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null) || ReferenceEquals(a.Value, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity key to struct value pairs invoking delegate.</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Struct value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_US<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Key == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity key to struct value pairs (ReferenceEquals optimization).</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Struct value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_US_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : UnityEngine.Object where T2 : struct
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Key, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates struct key to non-null Unity value pairs invoking delegate.</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_SU<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (a.Value == null) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        /// <summary>Asynchronously iterates struct key to non-null Unity value pairs (ReferenceEquals optimization).</summary>
        /// <remarks>Fire-and-forget (<c>async void</c>).</remarks>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to iterate.</param>
        /// <param name="Code">Async delegate invoked with (key, value).</param>
        public static async void ExForEachRO_SU_Optimized<T1, T2>(this IReadOnlyDictionary<T1, T2> data, Func<T1, T2, Task> Code) where T1 : struct where T2 : UnityEngine.Object
        {
            if (data.ExIsValidRO() == false) { return; }
            foreach (var a in data)
            {
                if (ReferenceEquals(a.Value, null)) { continue; }
                await Code?.Invoke(a.Key, a.Value);
            }
        }
        #endregion
        #endregion

        /// <summary>Returns true if the read-only dictionary reference is not null and contains at least one entry.</summary>
        /// <typeparam name="T1">Key type.</typeparam>
        /// <typeparam name="T2">Value type.</typeparam>
        /// <param name="data">Dictionary to validate.</param>
        public static bool ExIsValidRO<T1, T2>(this IReadOnlyDictionary<T1, T2> data) { return data != null && data.Count > 0; }

        #region Get NullCount
        #region Unity vs Unity
        /// <summary>Counts entries whose value is null (Unity key/value dictionary).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExNullValueCountRO_UU<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEachRO_UU((key, value) =>
            {
                if (value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts entries whose key is null (Unity key/value dictionary).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExNullKeyCountRO_UU<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEachRO_UU((key, value) =>
            {
                if (key == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts entries where either key or value is null (Unity key/value dictionary).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExAnyNullCountRO_UU<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEachRO_UU((key, value) =>
            {
                if (key == null || value == null) { count++; }
            });
            return count;
        }
        #endregion
        #region Unity vs Class
        /// <summary>Counts entries whose value is null (Unity key / class value dictionary).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExNullValueCountRO_UC<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : class
        {
            var count = 0;
            data.ExForEachRO_UC((key, value) =>
            {
                if (value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts entries whose key is null (Unity key / class value dictionary).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExNullKeyCountRO_UC<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : class
        {
            var count = 0;
            data.ExForEachRO_UC((key, value) =>
            {
                if (key == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts entries where either key or value is null (Unity key / class value dictionary).</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Class value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExAnyNullCountRO_UC<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : class
        {
            var count = 0;
            data.ExForEachRO_UC((key, value) =>
            {
                if (key == null || value == null) { count++; }
            });
            return count;
        }
        #endregion

        /// <summary>Counts entries whose value is null (struct key / Unity value dictionary).</summary>
        /// <typeparam name="T1">Struct key type.</typeparam>
        /// <typeparam name="T2">Unity value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExNullValueCountRO_SU<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : struct where T2 : UnityEngine.Object
        {
            var count = 0;
            data.ExForEachRO_SU((key, value) =>
            {
                if (value == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts entries whose key is null (Unity key / struct value dictionary). Struct key itself cannot be null; checks key for Unity part.</summary>
        /// <typeparam name="T1">Unity key type.</typeparam>
        /// <typeparam name="T2">Struct value type.</typeparam>
        /// <param name="data">Dictionary to scan.</param>
        public static int ExAnyNullCountRO_US<T1, T2>(this IReadOnlyDictionary<T1, T2> data) where T1 : UnityEngine.Object where T2 : struct
        {
            var count = 0;
            data.ExForEachRO_US((key, value) =>
            {
                if (key == null) { count++; }
            });
            return count;
        }
        #endregion
    }
}