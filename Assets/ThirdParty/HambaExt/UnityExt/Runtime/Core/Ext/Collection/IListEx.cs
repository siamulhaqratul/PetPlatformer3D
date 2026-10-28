using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace UnityExt
{
    /// <summary>Extension methods for <see cref="IList{T}"/> providing safe iteration, filtering, and utility helpers.</summary>
    public static class IListEx
    {
        // ======================= STATIC FIELDS ==========================

        // Random number generator for shuffle operations.
        private static System.Random _random = new();

        // ========================= PUBLIC METHODS ========================

        // ------------------------- Map Helpers ---------------------------

        /// <summary>
        /// Iterates two lists in parallel (best effort) invoking the supplied delegate with element/value and their indices.
        /// </summary>
        /// <typeparam name="T1">Element type of the source list (UnityEngine.Object).</typeparam>
        /// <typeparam name="T2">Element type of the target list (UnityEngine.Object).</typeparam>
        /// <param name="listFrom">Source list to iterate.</param>
        /// <param name="listTo">Secondary list from which matching (or next available) element is chosen.</param>
        /// <param name="iterationCode">Delegate receiving (elemFrom, elemTo, indexFrom, indexTo).</param>
        public static void ExForEachMap<T1, T2>(this IList<T1> listFrom, IList<T2> listTo, OnDoAnything<T1, T2, int, int> iterationCode)
            where T1 : UnityEngine.Object where T2 : UnityEngine.Object
        {
            if (listFrom != null && listFrom.Count > 0)
            {
                int completedNum = -1;
                for (int i = 0; i < listFrom.Count; i++)
                {
                    var elem1 = listFrom[i];
                    if (elem1 == null) { continue; }

                    T2 elem2 = null;
                    int index2 = -1;
                    for (int j = 0; j < listTo.Count; j++)
                    {
                        var cm2 = listTo[j];
                        if (cm2 == null || j < completedNum) { continue; }
                        completedNum++;
                        elem2 = cm2;
                        index2 = j;
                        break;
                    }

                    iterationCode?.Invoke(elem1, elem2, i, index2);
                }
            }
        }
        
        // ========================= FOREACH (NO ID) =======================

        /// <summary>Iterates all elements of the list (value types) invoking the provided delegate.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Action delegate.</param>
        public static void ExForEach<T>(this IList<T> list, OnDoAnything<T> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates all elements of the list (value types) invoking the provided async delegate.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate to invoke per element.</param>
        public static async Task ExForEachAsync<T>(this IList<T> list, Func<T, Task> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates all non-null UnityEngine.Object elements invoking the delegate.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate invoked for each non-null element.</param>
        public static void ExForEachSafe<T>(this IList<T> list, OnDoAnything<T> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates all non-null UnityEngine.Object elements invoking the delegate.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate invoked for each non-null element.</param>
        public static async Task ExForEachAsyncSafe<T>(this IList<T> list, Func<T, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates all non-null reference type elements invoking the delegate.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate invoked for each non-null element.</param>
        public static void ExForEachSafeCustomClass<T>(this IList<T> list, OnDoAnything<T> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates all non-null reference type elements invoking the async delegate.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate invoked per element.</param>
        public static async Task ExForEachAsyncSafeCustomClass<T>(this IList<T> list, Func<T, Task> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates list without any validation invoking delegate (fast path).</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate invoked per element.</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, OnDoAnything<T> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates list without validation invoking delegate (fast path).</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate invoked per element.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, Func<T, Task> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> items without validation.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Maximum number of elements to process.</param>
        /// <param name="Code">Delegate invoked per element.</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> items without validation.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of elements to process.</param>
        /// <param name="Code">Async delegate invoked per element.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, int len, Func<T, Task> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        
        // ======================= FOREACH WITH ID =========================

        /// <summary>Iterates list (value types) passing element and index.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate receiving (value, index).</param>
        public static void ExForEach<T>(this IList<T> list, OnDoAnything<T, int> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates list (value types) passing element and index.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate receiving (value, index).</param>
        public static async Task ExForEachAsync<T>(this IList<T> list, Func<T, int, Task> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null Unity objects passing element and index.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate receiving (value, index).</param>
        public static void ExForEachSafe<T>(this IList<T> list, OnDoAnything<T, int> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity objects passing element and index.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate receiving (value, index).</param>
        public static async Task ExForEachAsyncSafe<T>(this IList<T> list, Func<T, int, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null reference type elements passing element and index.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate receiving (value, index).</param>
        public static void ExForEachSafeCustomClass<T>(this IList<T> list, OnDoAnything<T, int> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null reference type elements passing element and index.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate receiving (value, index).</param>
        public static async Task ExForEachAsyncSafeCustomClass<T>(this IList<T> list, Func<T, int, Task> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates list without validation passing element and index (fast path).</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate invoked per element (value, index).</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, OnDoAnything<T, int> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates list without validation passing element and index (fast path).</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate invoked per element.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, Func<T, int, Task> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> items without validation passing element and index.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items to process.</param>
        /// <param name="Code">Delegate invoked per element.</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T, int> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> items without validation passing element and index.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items to process.</param>
        /// <param name="Code">Async delegate invoked per element.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, int len, Func<T, int, Task> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        
        // ==================== FOREACH WITH BREAK (NO ID) ==================

        /// <summary>Iterates value types until break predicate returns true.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Predicate indicating early termination.</param>
        public static void ExForEach<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates value types until break predicate returns true.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Predicate controlling early termination.</param>
        public static async Task ExForEachAsync<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates non-null Unity objects until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachSafe<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity objects until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncSafe<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates non-null reference types until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachSafeCustomClass<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates non-null reference types until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncSafeCustomClass<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates list without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates list without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> elements without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> elements without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, int len, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        
        // ================= FOREACH WITH ID + BREAK ======================

        /// <summary>Iterates value types passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static void ExForEach<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates value types passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static async Task ExForEachAsync<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null Unity objects passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static void ExForEachSafe<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity objects passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static async Task ExForEachAsyncSafe<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null reference type elements passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachSafeCustomClass<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null reference type elements passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncSafeCustomClass<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates list without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates list without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> elements without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Delegate invoked per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEach_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> elements without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Async delegate invoked per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsync<T>(this IList<T> list, int len, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        
        // ===================== FOREACH REVERSE (NO ID) ===================

        /// <summary>Iterates value types in reverse order.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        public static void ExForEachReverse<T>(this IList<T> list, OnDoAnything<T> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates value types in reverse order.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        public static async Task ExForEachReverseAsync<T>(this IList<T> list, Func<T, Task> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates non-null Unity objects in reverse order.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        public static void ExForEachReverseSafe<T>(this IList<T> list, OnDoAnything<T> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity objects in reverse order.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        public static async Task ExForEachReverseAsyncSafe<T>(this IList<T> list, Func<T, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates non-null reference types in reverse order.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        public static void ExForEachReverseSafeCustomClass<T>(this IList<T> list, OnDoAnything<T> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates non-null reference types in reverse order.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        public static async Task ExForEachReverseAsyncSafeCustomClass<T>(this IList<T> list, Func<T, Task> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates list in reverse without validation.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, OnDoAnything<T> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates list in reverse without validation.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, Func<T, Task> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> items (by index range) in reverse without validation.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List containing data.</param>
        /// <param name="len">Number of items (from start) to consider.</param>
        /// <param name="Code">Delegate per element.</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> items in reverse without validation.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List containing data.</param>
        /// <param name="len">Number of items (from start) to consider.</param>
        /// <param name="Code">Async delegate per element.</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, int len, Func<T, Task> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        
        // ================= FOREACH REVERSE WITH ID =======================

        /// <summary>Iterates value types in reverse passing element and index.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        public static void ExForEachReverse<T>(this IList<T> list, OnDoAnything<T, int> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates value types in reverse passing element and index.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        public static async Task ExForEachReverseAsync<T>(this IList<T> list, Func<T, int, Task> Code) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null Unity objects in reverse passing element and index.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        public static void ExForEachReverseSafe<T>(this IList<T> list, OnDoAnything<T, int> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity objects in reverse passing element and index.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        public static async Task ExForEachReverseAsyncSafe<T>(this IList<T> list, Func<T, int, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null reference types in reverse passing element and index.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        public static void ExForEachReverseSafeCustomClass<T>(this IList<T> list, OnDoAnything<T, int> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null reference types in reverse passing element and index.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        public static async Task ExForEachReverseAsyncSafeCustomClass<T>(this IList<T> list, Func<T, int, Task> Code) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates list in reverse without validation passing element and index.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, OnDoAnything<T, int> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates list in reverse without validation passing element and index.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, Func<T, int, Task> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> items in reverse without validation passing element and index.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Delegate (value, index).</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T, int> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> items in reverse without validation passing element and index.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, int len, Func<T, int, Task> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        
        // ============== FOREACH REVERSE WITH BREAK (NO ID) ===============

        /// <summary>Iterates value types in reverse until predicate signals break.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachReverse<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates value types in reverse until predicate signals break.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachReverseAsync<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates non-null Unity objects in reverse until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachReverseSafe<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity objects in reverse until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachReverseAsyncSafe<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates non-null reference types in reverse until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachReverseSafeCustomClass<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates non-null reference types in reverse until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachReverseAsyncSafeCustomClass<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Iterates list in reverse without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates list in reverse without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> items in reverse without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> items in reverse without validation until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Async delegate per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, int len, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        
        // =========== FOREACH REVERSE WITH ID + BREAK =====================

        /// <summary>Iterates value types in reverse passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static void ExForEachReverse<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates value types in reverse passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static async Task ExForEachReverseAsync<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null Unity objects in reverse passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static void ExForEachReverseSafe<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null Unity objects in reverse passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static async Task ExForEachReverseAsyncSafe<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates non-null reference types in reverse passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static void ExForEachReverseSafeCustomClass<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates non-null reference types in reverse passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static async Task ExForEachReverseAsyncSafeCustomClass<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValid() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>Iterates list in reverse without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates list in reverse without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate (value, index).</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        /// <summary>Iterates first <paramref name="len"/> items in reverse without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachReverse_NoCheck<T>(this IList<T> list, int len, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>Asynchronously iterates first <paramref name="len"/> items in reverse without validation passing element and index until predicate signals break.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="len">Number of items considered.</param>
        /// <param name="Code">Async delegate (value, index).</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachReverse_NoCheckAsync<T>(this IList<T> list, int len, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        // ========================= VALIDATION & QUERY =====================

        /// <summary>Returns true if the list reference is not null and contains at least one element.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to validate.</param>
        public static bool ExIsValid<T>(this IList<T> list) { return list != null && list.Count > 0; }
        /// <summary>Checks whether list contains the specified Unity object using reference comparison.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="lst">List to search.</param>
        /// <param name="t">Object to locate.</param>
        public static bool ExContainsOptimized<T>(this IList<T> lst, T t) where T : UnityEngine.Object
        {
            var contains = false;
            if (lst != null)
            {
                var len = lst.Count;
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        var it = lst[i];
                        if (ReferenceEquals(it, t))
                        {
                            contains = true;
                            break;
                        }
                    }
                }
            }
            return contains;
        }
        /// <summary>Checks whether <paramref name="checkList"/> contains any element from <paramref name="hasAnyOfThis"/>.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="checkList">List to inspect.</param>
        /// <param name="hasAnyOfThis">List of candidate elements.</param>
        public static bool ExContainsAnyFrom<T>(this IList<T> checkList, IList<T> hasAnyOfThis) where T : class
        {
            var contains = false;
            if (checkList != null)
            {
                var len = checkList.Count;
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        var it = checkList[i];
                        if (hasAnyOfThis.Contains(it))
                        {
                            contains = true;
                            break;
                        }
                    }
                }
            }
            return contains;
        }
        /// <summary>Checks whether <paramref name="checkList"/> contains any element not present in <paramref name="hasAnyOfThis"/> (alien element).</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="checkList">Subject list to inspect.</param>
        /// <param name="hasAnyOfThis">Whitelist list.</param>
        public static bool ExContainsAnyThatIsNotFrom<T>(this IList<T> checkList, IList<T> hasAnyOfThis) where T : class
        {
            var alienFound = false;
            if (checkList != null)
            {
                var len = checkList.Count;
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        var it = checkList[i];
                        if (!hasAnyOfThis.Contains(it))
                        {
                            alienFound = true;
                            break;
                        }
                    }
                }
            }
            return alienFound;
        }
        
        // ====================== MUTATION / UTILITIES =====================

        /// <summary>Removes duplicate entries from the list preserving the last occurrence order.</summary>
        /// <typeparam name="T">Element type supporting hashing.</typeparam>
        /// <param name="list">List to modify in-place.</param>
        public static void ExRemoveDuplicates<T>(this IList<T> list)
        {
            HashSet<T> uniqueValues = new HashSet<T>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!uniqueValues.Add(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }
        /// <summary>Randomizes the order of elements in the list using Fisher-Yates shuffle.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to shuffle in-place.</param>
        public static void ExShuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        /// <summary>Counts null Unity object references within the list.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to scan.</param>
        public static int ExNullCount<T>(this IList<T> list) where T : UnityEngine.Object
        {
            var count = 0;
            list.ExForEachSafe((t) =>
            {
                if (t == null) { count++; }
            });
            return count;
        }
        /// <summary>Counts null references in a custom class list.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to scan.</param>
        public static int ExNullCountCustomClass<T>(this IList<T> list) where T : class
        {
            var count = 0;
            list.ExForEachSafeCustomClass((t) =>
            {
                if (t == null) { count++; }
            });
            return count;
        }
        /// <summary>Adds elements uniquely (Unity objects only) from another list.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Destination list.</param>
        /// <param name="toAdd">Source list.</param>
        public static void ExAddRangeUniquely<T>(this IList<T> list, IList<T> toAdd) where T : UnityEngine.Object
        {
            toAdd.ExForEachSafe((i) =>
            {
                if (list != null && !list.Contains(i))
                {
                    list.Add(i);
                }
            });
        }
        /// <summary>Adds a single reference type element if not already present.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Destination list.</param>
        /// <param name="toAdd">Element to add.</param>
        public static void ExAddUniquelyCustomClass<T>(this IList<T> list, T toAdd) where T : class
        {
            if (list != null && !list.Contains(toAdd))
            {
                list.Add(toAdd);
            }
        }
        /// <summary>Adds a range of reference type elements uniquely.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Destination list.</param>
        /// <param name="toAdd">Source elements.</param>
        public static void ExAddRangeUniquelyCustomClass<T>(this IList<T> list, IList<T> toAdd) where T : class
        {
            toAdd.ExForEachSafeCustomClass((i) =>
            {
                if (list != null && !list.Contains(i))
                {
                    list.Add(i);
                }
            });
        }
        /// <summary>Adds value type data elements uniquely.</summary>
        /// <typeparam name="T">Struct element type.</typeparam>
        /// <param name="list">Destination list.</param>
        /// <param name="toAdd">Source elements.</param>
        public static void ExAddDataRangeUniquely<T>(this IList<T> list, IList<T> toAdd) where T : struct
        {
            toAdd.ExForEach((i) =>
            {
                if (list != null && !list.Contains(i))
                {
                    list.Add(i);
                }
            });
        }
        /// <summary>Returns a random element from list and removes it (or default if invalid).</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="lst">List to mutate.</param>
        public static T ExGetAnyRandomAndDeleteFromList<T>(this IList<T> lst)
        {
            T result = default;
            if (lst.ExIsValid())
            {
                int toRemoveID = UnityEngine.Random.Range(0, lst.Count);
                result = lst[toRemoveID];
                lst.RemoveAt(toRemoveID);
            }
            return result;
        }
        /// <summary>Returns true if list contains any null reference (reference types only).</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to inspect.</param>
        public static bool ExHasAnyNull<T>(this IList<T> list) where T : class
        {
            return list.Contains(null);
        }
        /// <summary>Counts non-null Unity object references.</summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to inspect.</param>
        public static int ExNotNullCount<T>(this IList<T> list) where T : UnityEngine.Object
        {
            var count = 0;
            list.ExForEachSafe((t) =>
            {
                if (t != null) { count++; }
            });
            return count;
        }
        /// <summary>Counts non-null reference type elements.</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to inspect.</param>
        public static int ExNotNullCountCustomClass<T>(this IList<T> list) where T : class
        {
            var count = 0;
            list.ExForEachSafeCustomClass((t) =>
            {
                if (t != null) { count++; }
            });
            return count;
        }
        /// <summary>Removes null references from the list (reference types only).</summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to be cleaned in-place.</param>
        public static void ExRemoveNullsOnIt<T>(this IList<T> list) where T : class
        {
            var validList = new List<T>();
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item == null || item.Equals(null)) { continue; }
                    validList.Add(item);
                }
            }
            list.Clear();
            if (validList != null && validList.Count > 0)
            {
                for (int i = 0; i < validList.Count; i++)
                {
                    var l = validList[i];
                    list.Add(l);
                }
            }
        }
    }
}