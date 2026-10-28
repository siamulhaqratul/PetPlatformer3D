using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityExt
{
    /// <summary>
    /// Provides extension methods for collections such as arrays and lists, 
    /// including utilities for null checks, shallow and deep copying, 
    /// and safe enumeration for UnityEngine.Object and custom class types.
    /// </summary>
    public static class ExCollection
    {
        // ========================= STATIC METHODS ========================

        /// <summary>
        /// Determines whether the array contains any null elements.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="array">The array to check.</param>
        /// <returns>True if any element is null; otherwise, false.</returns>
        public static bool ExHasAnyNull<T>(this T[] array) where T : class
        {
            return array.ExContains(null);
        }

        /// <summary>
        /// Counts the number of non-null elements in a UnityEngine.Object array.
        /// </summary>
        /// <typeparam name="T">Type derived from UnityEngine.Object.</typeparam>
        /// <param name="array">The array to count non-null elements in.</param>
        /// <returns>The count of non-null elements.</returns>
        public static int ExNotNullCount<T>(this T[] array) where T : UnityEngine.Object
        {
            var count = 0;
            array.ExForEachSafe((t) =>
            {
                if (t != null) { count++; }
            });
            return count;
        }

        /// <summary>
        /// Counts the number of non-null elements in a custom class array.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="array">The array to count non-null elements in.</param>
        /// <returns>The count of non-null elements.</returns>
        public static int ExNotNullCountCustomClass<T>(this T[] array) where T : class
        {
            var count = 0;
            array.ExForEachSafeCustomClass((t) =>
            {
                if (t != null) { count++; }
            });
            return count;
        }

        /// <summary>
        /// Counts the number of null elements in a UnityEngine.Object array.
        /// </summary>
        /// <typeparam name="T">Type derived from UnityEngine.Object.</typeparam>
        /// <param name="array">The array to count null elements in.</param>
        /// <returns>The count of null elements.</returns>
        public static int ExNullCount<T>(this T[] array) where T : UnityEngine.Object
        {
            var count = 0;
            array.ExForEachSafe((t) =>
            {
                if (t == null) { count++; }
            });
            return count;
        }

        /// <summary>
        /// Counts the number of null elements in a custom class array.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="array">The array to count null elements in.</param>
        /// <returns>The count of null elements.</returns>
        public static int ExNullCountCustomClass<T>(this T[] array) where T : class
        {
            var count = 0;
            array.ExForEachSafeCustomClass((t) =>
            {
                if (t == null) { count++; }
            });
            return count;
        }

        /// <summary>
        /// Creates a new list of the specified count, filled with default values.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">The source list (not used for content).</param>
        /// <param name="count">The number of elements in the new list.</param>
        /// <returns>A new list with the specified count of default values.</returns>
        public static List<T> ExGetListWithCount<T>(this List<T> list, int count)
        {
            var result = new List<T>();
            for (int i = 0; i < count; i++)
            {
                result.Add(default);
            }
            return result;
        }

        /// <summary>
        /// Creates a shallow copy of a list of UnityEngine.Object.
        /// </summary>
        /// <typeparam name="T">Type derived from UnityEngine.Object.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <returns>A new list containing the same references as the original.</returns>
        public static List<T> ExShallowCopy<T>(this List<T> list) where T : UnityEngine.Object
        {
            var result = new List<T>();
            list.ExForEachSafe((i) => { result.Add(i); });
            return result;
        }

        public static List<T> ExShallowCopyAndShuffle<T>(this List<T> list) where T : UnityEngine.Object
        {
            var result = new List<T>();
            list.ExForEachSafe((i) => { result.Add(i); });
            result.ExShuffle();
            return result;
        }

        /// <summary>
        /// Creates a shallow copy of a list of custom class objects.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <returns>A new list containing the same references as the original.</returns>
        public static List<T> ExShallowCopyCustomClass<T>(this List<T> list) where T : class
        {
            var result = new List<T>();
            list.ExForEachSafeCustomClass((i) => { result.Add(i); });
            return result;
        }

        /// <summary>
        /// Creates a shallow copy of a list of value types.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <returns>A new list containing the same values as the original.</returns>
        public static List<T> ExShallowCopyData<T>(this List<T> list) where T : struct
        {
            var result = new List<T>();
            list.ExForEach((i) => { result.Add(i); });
            return result;
        }

        /// <summary>
        /// Creates a deep copy of a list of UnityEngine.Object that implements ICloneable.
        /// </summary>
        /// <typeparam name="T">Type derived from UnityEngine.Object and ICloneable.</typeparam>
        /// <param name="list">The list to deep copy.</param>
        /// <returns>A new list with cloned elements.</returns>
        public static List<T> ExDeepCopy<T>(this List<T> list) where T : UnityEngine.Object, ICloneable
        {
            var result = new List<T>();
            list.ExForEachSafe((i) => { result.Add(i.Clone() as T); });
            return result;
        }

        /// <summary>
        /// Creates a deep copy of a list of custom class objects that implement ICloneable.
        /// </summary>
        /// <typeparam name="T">Reference type implementing ICloneable.</typeparam>
        /// <param name="list">The list to deep copy.</param>
        /// <returns>A new list with cloned elements.</returns>
        public static List<T> ExDeepCopyCustomClass<T>(this List<T> list) where T : class, ICloneable
        {
            var result = new List<T>();
            list.ExForEachSafeCustomClass((i) => { result.Add(i.Clone() as T); });
            return result;
        }

        /// <summary>
        /// Creates a shallow copy of an array of UnityEngine.Object.
        /// </summary>
        /// <typeparam name="T">Type derived from UnityEngine.Object.</typeparam>
        /// <param name="list">The array to copy.</param>
        /// <returns>A new array containing the same references as the original.</returns>
        public static T[] ExShallowCopy<T>(this T[] list) where T : UnityEngine.Object
        {
            var result = new T[list.Length];
            list.ExForEachSafe((i, index) => { result[index] = i; });
            return result;
        }

        /// <summary>
        /// Creates a shallow copy of an array of custom class objects.
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">The array to copy.</param>
        /// <returns>A new array containing the same references as the original.</returns>
        public static T[] ExShallowCopyCustomClass<T>(this T[] list) where T : class
        {
            var result = new T[list.Length];
            list.ExForEachSafeCustomClass((i, index) => { result[index] = i; });
            return result;
        }

        /// <summary>
        /// Creates a shallow copy of an array of value types.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="list">The array to copy.</param>
        /// <returns>A new array containing the same values as the original.</returns>
        public static T[] ExShallowCopyData<T>(this T[] list) where T : struct
        {
            var result = new T[list.Length];
            list.ExForEach((i, index) => { result[index] = i; });
            return result;
        }

        /// <summary>
        /// Creates a deep copy of an array of UnityEngine.Object that implements ICloneable.
        /// </summary>
        /// <typeparam name="T">Type derived from UnityEngine.Object and ICloneable.</typeparam>
        /// <param name="list">The array to deep copy.</param>
        /// <returns>A new array with cloned elements.</returns>
        public static T[] ExDeepCopy<T>(this T[] list) where T : UnityEngine.Object, ICloneable
        {
            var result = new T[list.Length];
            list.ExForEachSafe((i, index) => { result[index] = i.Clone() as T; });
            return result;
        }

        /// <summary>
        /// Creates a deep copy of an array of custom class objects that implement ICloneable.
        /// </summary>
        /// <typeparam name="T">Reference type implementing ICloneable.</typeparam>
        /// <param name="list">The array to deep copy.</param>
        /// <returns>A new array with cloned elements.</returns>
        public static T[] ExDeepCopyCustomClass<T>(this T[] list) where T : class, ICloneable
        {
            var result = new T[list.Length];
            list.ExForEachSafeCustomClass((i, index) => { result[index] = i.Clone() as T; });
            return result;
        }

        /// <summary>
        /// Determines whether the array is not null and contains at least one element.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="array">The array to check.</param>
        /// <returns>True if the array is not null and not empty; otherwise, false.</returns>
        public static bool ExIsValid<T>(this T[] array)
        {
            return array != null && array.Length > 0;
        }

        /// <summary>
        /// Returns a new list with all null elements removed (for custom class types).
        /// </summary>
        /// <typeparam name="T">Reference type.</typeparam>
        /// <param name="list">The list to filter.</param>
        /// <returns>A new list without null elements.</returns>
        public static List<T> ExRemoveNulls<T>(this List<T> list) where T : class
        {
            var result = new List<T>();
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item == null || item.Equals(null)) { continue; }
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a new list with all null elements removed (for UnityEngine.Object types).
        /// </summary>
        /// <typeparam name="T">Type derived from UnityEngine.Object.</typeparam>
        /// <param name="list">The list to filter.</param>
        /// <returns>A new list without null elements.</returns>
        public static List<T> ExRemoveNullsUnityObjects<T>(this List<T> list) where T : UnityEngine.Object
        {
            var result = new List<T>();
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var l = list[i];
                    if (l == null) { continue; }
                    result.Add(l);
                }
            }
            return result;
        }


        public static bool ExContains<T>(this T[] array, T item) where T : class
        {
            bool contains = false;
            if (array != null && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    var it = array[i];
                    if (it == item)
                    {
                        contains = true;
                        break;
                    }
                }
            }
            return contains;
        }

    }
}