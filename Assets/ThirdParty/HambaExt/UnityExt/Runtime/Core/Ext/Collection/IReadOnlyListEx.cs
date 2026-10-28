using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace UnityExt
{
    /// <summary>
    /// Extension helpers for <see cref="IReadOnlyList{T}"/> adding rich, allocation-free iteration patterns
    /// (forward / reverse, with index, breakable, async variants) plus validation / containment / null analysis utilities.
    /// </summary>
    public static class IReadOnlyListEx
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Iterates two Unity object read-only lists in parallel (mapping one to another) invoking <paramref name="iterationCode"/> with both elements and their indices.
        /// </summary>
        /// <typeparam name="T1">UnityEngine.Object element type of the source list.</typeparam>
        /// <typeparam name="T2">UnityEngine.Object element type of the target list to map onto.</typeparam>
        /// <param name="listFrom">Primary list to iterate.</param>
        /// <param name="listTo">Secondary list from which the next non-null element (beyond last consumed index) is paired.</param>
        /// <param name="iterationCode">Delegate invoked with (elemFrom, elemToOrNull, indexFrom, indexToOrNegativeOne).</param>
        /// <remarks>
        /// For every non-null element in <paramref name="listFrom"/>, the method searches the next not-yet-used non-null element in <paramref name="listTo"/>.
        /// If no remaining non-null element exists in <paramref name="listTo"/>, <c>null</c> and <c>-1</c> are passed for the paired element and index.
        /// </remarks>
        public static void ExForEachMapRO<T1, T2>(this IReadOnlyList<T1> listFrom, IReadOnlyList<T2> listTo, OnDoAnything<T1, T2, int, int> iterationCode)
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

        #region Foreach
        /// <summary>
        /// Iterates all elements (struct) invoking <paramref name="Code"/> for each element.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per element.</param>
        public static void ExForEachRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async variant of <see cref="ExForEachRO{T}(IReadOnlyList{T}, OnDoAnything{T})"/> awaiting a task per element.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        public static async Task ExForEachAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Iterates all non-null UnityEngine.Object elements invoking <paramref name="Code"/>.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        public static void ExForEachSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async safe iteration over non-null UnityEngine.Object elements.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        public static async Task ExForEachAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Safe iteration for reference-type custom class elements skipping nulls.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        public static void ExForEachSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async safe iteration for custom reference-type elements skipping nulls.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        public static async Task ExForEachAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Iterates list without any validation or null checks (fast path). Caller must ensure list validity.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">Read-only list (assumed non-null).</param>
        /// <param name="Code">Delegate executed per element (not null-checked).</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>
        /// Async variant of <see cref="ExForEach_NoCheckRO{T}(IReadOnlyList{T}, OnDoAnything{T})"/>.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">Read-only list (assumed non-null).</param>
        /// <param name="Code">Async delegate executed per element.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        /// <summary>
        /// Fast iteration over first <paramref name="len"/> elements without validation.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List (assumed non-null, large enough).</param>
        /// <param name="len">Number of elements to iterate (caller responsibility).</param>
        /// <param name="Code">Delegate executed per element.</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>
        /// Async fast iteration over first <paramref name="len"/> elements without validation.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List (assumed non-null, large enough).</param>
        /// <param name="len">Number of elements to iterate.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, Task> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        #endregion

        #region Foreach With ID
        /// <summary>
        /// Iterates value-type elements providing index to <paramref name="Code"/>.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate receiving element and index.</param>
        public static void ExForEachRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async variant providing element and index to <paramref name="Code"/>.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate receiving element and index.</param>
        public static async Task ExForEachAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Safe iteration for Unity objects providing index.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed for each non-null element and its index.</param>
        public static void ExForEachSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async safe iteration for Unity objects providing index.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        public static async Task ExForEachAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Safe iteration for custom reference types with index skipping nulls.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate receiving non-null element and its index.</param>
        public static void ExForEachSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async safe iteration for custom reference types with index.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate receiving non-null element and index.</param>
        public static async Task ExForEachAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast iteration (no validation) providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List (assumed valid).</param>
        /// <param name="Code">Delegate executed per element and its index.</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast iteration (no validation) providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List (assumed valid).</param>
        /// <param name="Code">Async delegate executed per element and index.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast iteration over first <paramref name="len"/> elements (no validation) providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List (assumed valid and length &gt;= len).</param>
        /// <param name="len">Number of elements to iterate.</param>
        /// <param name="Code">Delegate executed per element.</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T, int> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast iteration over first <paramref name="len"/> elements (no validation) providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List (assumed valid).</param>
        /// <param name="len">Number of elements to iterate.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, int, Task> Code)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        #endregion

        #region Foreach with break
        /// <summary>
        /// Iterates value-type elements until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate executed per element until break.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async breakable iteration for value-type elements.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate executed per element until break.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Breakable safe iteration for Unity objects skipping nulls.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate executed for each non-null element until predicate breaks.</param>
        /// <param name="whenBreak">Predicate evaluated after null skip deciding early termination.</param>
        public static void ExForEachSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async breakable safe iteration for Unity objects skipping nulls.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate executed per non-null element until break.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Breakable safe iteration for custom reference types skipping nulls.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async breakable safe iteration for custom reference types skipping nulls.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Fast breakable iteration (no validation) over elements.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Delegate executed per element until break.</param>
        /// <param name="whenBreak">Predicate deciding early termination.</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>
        /// Async fast breakable iteration (no validation).
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Async delegate executed per element until break.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        /// <summary>
        /// Fast breakable iteration over first <paramref name="len"/> elements.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to process.</param>
        /// <param name="Code">Delegate executed per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>
        /// Async fast breakable iteration over first <paramref name="len"/> elements.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="len">Number of elements to process.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        #endregion

        #region Foreach With ID
        /// <summary>
        /// Breakable iteration over value-type elements providing index.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate executed per element until break.</param>
        /// <param name="whenBreak">Predicate receiving element and index; when true ends iteration.</param>
        public static void ExForEachRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async breakable iteration over value-type elements with index.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate executed per element until break.</param>
        /// <param name="whenBreak">Break predicate accepting element and index.</param>
        public static async Task ExForEachAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Breakable safe iteration for Unity objects with index skipping nulls.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate executed per non-null element until break.</param>
        /// <param name="whenBreak">Break predicate receiving element and index.</param>
        public static void ExForEachSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async breakable safe iteration for Unity objects with index.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Breakable safe iteration for custom reference types with index skipping nulls.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEachSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async breakable safe iteration for custom reference types with index.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to iterate.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEachAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast breakable iteration (no validation) providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Delegate executed per element until break.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast breakable iteration (no validation) providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Async delegate executed per element until break.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast breakable iteration over first <paramref name="len"/> elements providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements processed.</param>
        /// <param name="Code">Delegate executed per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static void ExForEach_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast breakable iteration over first <paramref name="len"/> elements providing index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="len">Number of elements.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        /// <param name="whenBreak">Break predicate.</param>
        public static async Task ExForEach_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = 0; i < len; i++)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        #endregion

        #region Foreach Reverse
        /// <summary>Iterates all elements (struct) in reverse order, invoking <paramref name="Code"/> for each element.</summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per element.</param>
        public static void ExForEachReverseRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code?.Invoke(l);
            }
        }
        /// <summary>Async reverse iteration over all elements (struct), invoking <paramref name="Code"/> for each element.</summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        public static async Task ExForEachReverseAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Iterates all non-null UnityEngine.Object elements in reverse order, invoking <paramref name="Code"/> for each element.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        public static void ExForEachReverseSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null UnityEngine.Object elements, invoking <paramref name="Code"/> for each element.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        public static async Task ExForEachReverseAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Iterates all non-null custom reference-type elements in reverse order, invoking <paramref name="Code"/> for each element.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        public static void ExForEachReverseSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null custom reference-type elements, invoking <paramref name="Code"/> for each element.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        public static async Task ExForEachReverseAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>Fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Delegate executed per element.</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>Async fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element.</summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        /// <summary>
        /// Fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Delegate executed per element.</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l);
            }
        }
        /// <summary>
        /// Async fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, Task> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l);
            }
        }
        #endregion

        #region Foreach Reverse With ID
        /// <summary>
        /// Iterates all elements (struct) in reverse order, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per element and its index.</param>
        public static void ExForEachReverseRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async reverse iteration over all elements (struct), invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per element and its index.</param>
        public static async Task ExForEachReverseAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Iterates all non-null UnityEngine.Object elements in reverse order, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element and its index.</param>
        public static void ExForEachReverseSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null UnityEngine.Object elements, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element and its index.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Iterates all non-null custom reference-type elements in reverse order, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element and its index.</param>
        public static void ExForEachReverseSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null custom reference-type elements, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element and its index.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Delegate executed per element and its index.</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Async delegate executed per element and its index.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Delegate executed per element and its index.</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T, int> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element and its index.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Async delegate executed per element and its index.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, int, Task> Code)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                await Code.Invoke(l, i);
            }
        }
        #endregion

        #region Foreach Reverse with break
        /// <summary>
        /// Iterates all elements (struct) in reverse order, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverseRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async reverse iteration over all elements (struct), invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Reverse iteration over all non-null UnityEngine.Object elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverseSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null UnityEngine.Object elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Reverse iteration over all non-null custom reference-type elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverseSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null custom reference-type elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l)) { break; }
                await Code?.Invoke(l);
            }
        }
        /// <summary>
        /// Fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Delegate executed per element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>
        /// Async fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        /// <summary>
        /// Fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Delegate executed per element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                Code.Invoke(l);
            }
        }
        /// <summary>
        /// Async fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Async delegate executed per element.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, Task> Code, WhenToDoFunc<T> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l)) { break; }
                await Code.Invoke(l);
            }
        }
        #endregion

        #region Foreach Reverse With ID
        /// <summary>
        /// Iterates all elements (struct) in reverse order, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverseRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async reverse iteration over all elements (struct), invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Value type element.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : struct
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Reverse iteration over all non-null UnityEngine.Object elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverseSafeRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null UnityEngine.Object elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncSafeRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : UnityEngine.Object
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Reverse iteration over all non-null custom reference-type elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Delegate executed per non-null element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverseSafeCustomClassRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async reverse iteration over all non-null custom reference-type elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">Read-only list.</param>
        /// <param name="Code">Async delegate executed per non-null element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverseAsyncSafeCustomClassRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak) where T : class
        {
            if (list.ExIsValidRO() == false) { return; }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (l == null) { continue; }
                if (whenBreak.Invoke(l, i)) { break; }
                await Code?.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Delegate executed per element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast reverse iteration (no validation) over all elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid.</param>
        /// <param name="Code">Async delegate executed per element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Delegate executed per element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        public static void ExForEachReverse_NoCheckRO<T>(this IReadOnlyList<T> list, int len, OnDoAnything<T, int> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                Code.Invoke(l, i);
            }
        }
        /// <summary>
        /// Async fast reverse iteration (no validation) over the first <paramref name="len"/> elements, invoking <paramref name="Code"/> for each element and its index until <paramref name="whenBreak"/> returns true.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List assumed valid and length &gt;= len.</param>
        /// <param name="len">Number of elements to iterate in reverse.</param>
        /// <param name="Code">Async delegate executed per element and its index.</param>
        /// <param name="whenBreak">Predicate which when true stops iteration (element that triggered break is not processed if the delegate invocation precedes check).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ExForEachReverse_NoCheckAsyncRO<T>(this IReadOnlyList<T> list, int len, Func<T, int, Task> Code, WhenToDoFunc<T, int> whenBreak)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                var l = list[i];
                if (whenBreak.Invoke(l, i)) { break; }
                await Code.Invoke(l, i);
            }
        }
        #endregion

        /// <summary>
        /// Returns true if the list is non-null and has at least one element.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to validate.</param>
        public static bool ExIsValidRO<T>(this IReadOnlyList<T> list) { return list != null && list.Count > 0; }
        /// <summary>
        /// Determines if the list contains the reference-equal element <paramref name="t"/> (fast path using <see cref="ReferenceEquals(object, object)"/>).
        /// </summary>
        /// <typeparam name="T">Reference type element.</typeparam>
        /// <param name="lst">List to search.</param>
        /// <param name="t">Element to find (reference comparison).</param>
        /// <returns>True if found by reference; otherwise false.</returns>
        public static bool ExContainsOptimizedRO<T>(this IReadOnlyList<T> lst, T t) where T : class
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
        /// <summary>
        /// Determines if the list contains <paramref name="t"/> using standard equality (==).
        /// </summary>
        /// <typeparam name="T">Reference type element.</typeparam>
        /// <param name="lst">List to search.</param>
        /// <param name="t">Element to compare via operator ==.</param>
        /// <returns>True if an equal element exists; otherwise false.</returns>
        public static bool ExContainsRO<T>(this IReadOnlyList<T> lst, T t) where T : class
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
                        if (it == t)
                        {
                            contains = true;
                            break;
                        }
                    }
                }
            }
            return contains;
        }
        /// <summary>
        /// Counts null Unity object references (UnityEngine.Object handling) within the list.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to inspect.</param>
        /// <returns>Number of null (destroyed/missing) references.</returns>
        public static int ExNullCountRO<T>(this IReadOnlyList<T> list) where T : UnityEngine.Object
        {
            var count = 0;
            list.ExForEachSafeRO((t) =>
            {
                if (t == null) { count++; }
            });
            return count;
        }
        /// <summary>
        /// Counts null entries for custom reference-type elements.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to inspect.</param>
        /// <returns>Null entry count.</returns>
        public static int ExNullCountCustomClassRO<T>(this IReadOnlyList<T> list) where T : class
        {
            var count = 0;
            list.ExForEachSafeCustomClassRO((t) =>
            {
                if (t == null) { count++; }
            });
            return count;
        }
        /// <summary>
        /// Returns true if the list contains any null reference.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to search.</param>
        public static bool ExHasAnyNullRO<T>(this IReadOnlyList<T> list) where T : class
        {
            return list.ExContainsRO(null);
        }
        /// <summary>
        /// Counts non-null Unity object references.
        /// </summary>
        /// <typeparam name="T">Unity object type.</typeparam>
        /// <param name="list">List to inspect.</param>
        /// <returns>Number of non-null entries.</returns>
        public static int ExNotNullCountRO<T>(this IReadOnlyList<T> list) where T : UnityEngine.Object
        {
            var count = 0;
            list.ExForEachSafeRO((t) =>
            {
                if (t != null) { count++; }
            });
            return count;
        }
        /// <summary>
        /// Counts non-null custom reference-type elements.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">List to inspect.</param>
        /// <returns>Number of non-null entries.</returns>
        public static int ExNotNullCountCustomClassRO<T>(this IReadOnlyList<T> list) where T : class
        {
            var count = 0;
            list.ExForEachSafeCustomClassRO((t) =>
            {
                if (t != null) { count++; }
            });
            return count;
        }
        /// <summary>
        /// Returns true if any element of <paramref name="checkList"/> is contained in <paramref name="hasAnyOfThis"/>.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="checkList">Primary list to scan.</param>
        /// <param name="hasAnyOfThis">Lookup list whose membership is tested.</param>
        /// <returns>True if at least one shared element exists.</returns>
        public static bool ExContainsAnyFromRO<T>(this IReadOnlyList<T> checkList, IList<T> hasAnyOfThis) where T : class
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
        /// <summary>
        /// Returns true if <paramref name="checkList"/> contains an element NOT present in <paramref name="hasAnyOfThis"/>.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="checkList">List whose elements are validated against the allowed set.</param>
        /// <param name="hasAnyOfThis">Allowed elements list.</param>
        /// <returns>True if an alien element exists; otherwise false.</returns>
        public static bool ExContainsAnyThatIsNotFromRO<T>(this IReadOnlyList<T> checkList, IList<T> hasAnyOfThis) where T : class
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

        public static List<T> ExShallowCopyData<T>(this IReadOnlyList<T> list) where T : struct
        {
            var result = new List<T>();
            list.ExForEach_NoCheckRO((i) => { result.Add(i); });
            return result;
        }

    }
}