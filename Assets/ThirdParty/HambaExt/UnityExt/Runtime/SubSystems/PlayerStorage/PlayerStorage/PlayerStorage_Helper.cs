using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt.Debugging;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Delegate for converting string data back to strongly-typed values during load operations.
    /// Used by the type conversion system to deserialize saved data.
    /// </summary>
    /// <typeparam name="T">Target type to convert to</typeparam>
    /// <param name="input">String representation of the data</param>
    /// <returns>Converted value of type T</returns>
    internal delegate T PSGetter<T>(string input);

    /// <summary>
    /// Delegate for converting strongly-typed values to string during save operations.
    /// Used by the type conversion system to serialize data for storage.
    /// </summary>
    /// <typeparam name="T">Source type to convert from</typeparam>
    /// <param name="input">Value to convert to string</param>
    /// <returns>String representation of the value</returns>
    internal delegate string PSSetter<T>(T input);

    /// <summary>
    /// Helper methods and type conversion system for PlayerStorage<T>.
    /// Provides serialization/deserialization for supported Unity and .NET types.
    /// </summary>
    public partial class PlayerStorage<T> where T : struct
    {
        /// <summary>
        /// Dictionary mapping supported types to their string-to-value conversion functions.
        /// Each entry defines how to deserialize a specific type from stored string data.
        /// Supports primitive types, Unity math types, and enums.
        /// </summary>
        readonly static IReadOnlyDictionary<System.Type, PSGetter<T>> GetValueDefines = new Dictionary<Type, PSGetter<T>>
        {
            // Primitive numeric types - direct parsing with error handling
            {
                typeof(int),
                (str)=>
                {
                    T result = default;
                    int val = 0;
                    var success = int.TryParse(str, out val);
                    if (success == false)
                    {
                        GetValueFromStringError("int->value:" + str);
                    }
                    result = (T)(object)val;
                    return result;
                }
            },

            {
                typeof(long),
                (str)=>
                {
                    T result = default;
                    long val = 0;
                    var success = long.TryParse(str, out val);
                    if (success == false)
                    {
                        GetValueFromStringError("long->value:" + str);
                    }
                    result = (T)(object)val;
                    return result;
                }
            },

            // DateTime stored as ticks (long) for precision and platform consistency
            {
                typeof(DateTime),
                (str)=>
                {
                    T result = default;
                    long val = 0;
                    var success = long.TryParse(str, out val);
                    if (success == false)
                    {
                        GetValueFromStringError("datetime tick long->value:" + str);
                    }
                    var valdt = new DateTime(val);
                    result = (T)(object)valdt;
                    return result;
                }
            },

            // Floating-point types - may have precision limitations
            {
                typeof(float),
                (str)=>
                {
                    T result = default;
                    float val = 0;
                    var success = float.TryParse(str, out val);
                    if (success == false)
                    {
                        GetValueFromStringError("float->value:" + str);
                    }
                    result = (T)(object)val;
                    return result;
                }
            },

            {
                typeof(double),
                (str)=>
                {
                    T result = default;
                    double val = 0;
                    var success = double.TryParse(str, out val);
                    if (success == false)
                    {
                        GetValueFromStringError("double->value:" + str);
                    }
                    result = (T)(object)val;
                    return result;
                }
            },

            // Boolean type - supports "True"/"False" string parsing
            {
                typeof(bool),
                (str)=>
                {
                    T result = default;
                    bool val = false;
                    var success = bool.TryParse(str, out val);
                    if (success == false)
                    {
                        GetValueFromStringError("bool->value:" + str);
                    }
                    result = (T)(object)val;
                    return result;
                }
            },

            // Unity math types - serialized as JSON through wrapper classes
            // Wrappers are needed because Unity's JsonUtility can't serialize these types directly
            
            {
                typeof(Ray),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Ray_Wrapper();
                    wrapper = JsonUtility.FromJson<Ray_Wrapper>(str);
                    result = (T)(object)(new Ray(wrapper.rayOrigin, wrapper.rayDirection));
                    return result;
                }
            },

            {
                typeof(Ray2D),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Ray2D_Wrapper();
                    wrapper = JsonUtility.FromJson<Ray2D_Wrapper>(str);
                    result = (T)(object)(new Ray2D(wrapper.rayOrigin, wrapper.rayDirection));
                    return result;
                }
            },

            {
                typeof(Bounds),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Bounds_Wrapper();
                    wrapper = JsonUtility.FromJson<Bounds_Wrapper>(str);
                    result = (T)(object)(new Bounds(wrapper.center, wrapper.size));
                    return result;
                }
            },

            {
                typeof(BoundsInt),
                (str)=>
                {
                    T result = default;
                    var wrapper = new BoundsInt_Wrapper();
                    wrapper = JsonUtility.FromJson<BoundsInt_Wrapper>(str);
                    result = (T)(object)(new BoundsInt(wrapper.position, wrapper.size));
                    return result;
                }
            },

            {
                typeof(Rect),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Rect_Wrapper();
                    wrapper = JsonUtility.FromJson<Rect_Wrapper>(str);
                    result = (T)(object)(new Rect(wrapper.position, wrapper.size));
                    return result;
                }
            },

            {
                typeof(RectInt),
                (str)=>
                {
                    T result = default;
                    var wrapper = new RectInt_Wrapper();
                    wrapper = JsonUtility.FromJson<RectInt_Wrapper>(str);
                    result = (T)(object)(new RectInt(wrapper.position, wrapper.size));
                    return result;
                }
            },

            // Vector types - common in game development for positions, directions, etc.
            {
                typeof(Vector2Int),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Vector2Int_Wrapper();
                    wrapper = JsonUtility.FromJson<Vector2Int_Wrapper>(str);
                    result = (T)(object)(wrapper.data);
                    return result;
                }
            },

            {
                typeof(Vector3Int),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Vector3Int_Wrapper();
                    wrapper = JsonUtility.FromJson<Vector3Int_Wrapper>(str);
                    result = (T)(object)(wrapper.data);
                    return result;
                }
            },

            {
                typeof(Vector2),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Vector2_Wrapper();
                    wrapper = JsonUtility.FromJson<Vector2_Wrapper>(str);
                    result = (T)(object)(wrapper.data);
                    return result;
                }
            },

            {
                typeof(Vector3),
                (str)=>
                {
                    T result = default;
                    var wrapper = new Vector3_Wrapper();
                    wrapper = JsonUtility.FromJson<Vector3_Wrapper>(str);
                    result = (T)(object)(wrapper.data);
                    return result;
                }
            },

            {
                typeof(TimeSpan),
                (str)=>
                {
                    T result = default;
                    long val = 0;
                    var success = long.TryParse(str, out val);
                    if (success == false)
                    {
                        GetValueFromStringError("timespan->value:" + str);
                    }
                    result = (T)(object)(new TimeSpan(val));
                    return result;
                }
            }
        };

        /// <summary>
        /// Dictionary mapping supported types to their value-to-string conversion functions.
        /// Each entry defines how to serialize a specific type to string for storage.
        /// Must correspond exactly with GetValueDefines for proper round-trip conversion.
        /// </summary>
        readonly static IReadOnlyDictionary<System.Type, PSSetter<T>> SetValueDefines = new Dictionary<Type, PSSetter<T>>
        {
            // Primitive types - simple string concatenation (leverages .NET's ToString())
            {
                typeof(int),
                (data)=>
                {
                    return (int)(object)data + "";
                }
            },

            {
                typeof(long),
                (data)=>
                {
                    return (long)(object)data + "";
                }
            },

            // DateTime serialized as Ticks for maximum precision and cross-platform compatibility
            {
                typeof(DateTime),
                (data)=>
                {
                    string result = "";
                    var dateTime = (DateTime)(object)data;
                    result = dateTime.Ticks + "";
                    return result;
                }
            },

            {
                typeof(float),
                (data)=>
                {
                    return (float)(object)data + "";
                }
            },

            {
                typeof(double),
                (data)=>
                {
                    return (double)(object)data + "";
                }
            },

            {
                typeof(bool),
                (data)=>
                {
                    return (bool)(object)data + "";
                }
            },

            // Unity math types - converted to wrapper classes and serialized as JSON
            // JSON provides structured storage for multi-component types
            {
                typeof(Ray),
                (data)=>
                {
                    string result = "";
                    var convData = (Ray)(object)data;
                    var wrapper = new Ray_Wrapper() { rayOrigin = convData.origin, rayDirection = convData.direction };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(Ray2D),
                (data)=>
                {
                    string result = "";
                    var convData = (Ray2D)(object)data;
                    var wrapper = new Ray2D_Wrapper() { rayOrigin = convData.origin, rayDirection = convData.direction };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(Bounds),
                (data)=>
                {
                    string result = "";
                    var convData = (Bounds)(object)data;
                    var wrapper = new Bounds_Wrapper() { center = convData.center, size = convData.size };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(BoundsInt),
                (data)=>
                {
                    string result = "";
                    var convData = (BoundsInt)(object)data;
                    var wrapper = new BoundsInt_Wrapper() { position = convData.position, size = convData.size };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(Rect),
                (data)=>
                {
                    string result = "";
                    var convData = (Rect)(object)data;
                    var wrapper = new Rect_Wrapper() { position = convData.position, size = convData.size };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(RectInt),
                (data)=>
                {
                    string result = "";
                    var convData = (RectInt)(object)data;
                    var wrapper = new RectInt_Wrapper() { position = convData.position, size = convData.size };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(Vector2Int),
                (data)=>
                {
                    string result = "";
                    var convData = (Vector2Int)(object)data;
                    var wrapper = new Vector2Int_Wrapper() { data = convData };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(Vector3Int),
                (data)=>
                {
                    string result = "";
                    var convData = (Vector3Int)(object)data;
                    var wrapper = new Vector3Int_Wrapper() { data = convData };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(Vector2),
                (data)=>
                {
                    string result = "";
                    var convData = (Vector2)(object)data;
                    var wrapper = new Vector2_Wrapper() { data = convData };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },

            {
                typeof(Vector3),
                (data)=>
                {
                    string result = "";
                    var convData = (Vector3)(object)data;
                    var wrapper = new Vector3_Wrapper() { data = convData };
                    result = JsonUtility.ToJson(wrapper, UConfig.UsePrettyPrintForSaveDataJson);
                    return result;
                }
            },
            {
                typeof(TimeSpan),
                (data)=>
                {
                    string result = "";
                    var convData = (TimeSpan)(object)data;
                    result = convData.Ticks + "";
                    return result;
                }
            }
        };

        /// <summary>
        /// Centralized error handling for type conversion failures.
        /// In development builds, throws exceptions for debugging.
        /// In production builds, logs errors without crashing.
        /// </summary>
        /// <param name="customMsg">Additional context about the conversion failure</param>
        static void GetValueFromStringError(string customMsg)
        {
            var txt = "Could not parse data! additional info msg: " + customMsg;
#if DEVELOPMENT_BUILD
            // Throw exception in development for immediate debugging feedback
            throw new Exception(txt);
#else
            // Log error in production to avoid crashes but still provide diagnostics
            GLog.PrintError(txt);
#endif
        }

        /// <summary>
        /// Converts string representation back to strongly-typed value.
        /// Handles both enum types and registered value types.
        /// Used during load operations to deserialize saved data.
        /// </summary>
        /// <param name="str">String representation of the value</param>
        /// <returns>Converted value of type T</returns>
        T GetVal(string str)
        {
            T result = default;

            // Special handling for enum types using built-in Enum.TryParse
            if (typeof(T).IsEnum)
            {
                T val = default;
                var success = Enum.TryParse<T>(str, out val);
                if (success == false)
                {
                    GetValueFromStringError("Enum->value:" + str);
                }
                result = (T)(object)val;
            }
            else
            {
                // Use registered conversion function for non-enum types
                var typeT = typeof(T);
                if (GetValueDefines.ContainsKey(typeT))
                {
                    result = GetValueDefines[typeT].Invoke(str);
                }
                else
                {
                    GetValueFromStringError("Invalid or unsupported type! " + typeof(T));
                }
            }
            return result;
        }

        /// <summary>
        /// Converts strongly-typed value to string representation for storage.
        /// Handles both enum types and registered value types.
        /// Used during save operations to serialize data.
        /// </summary>
        /// <param name="data">Value to convert to string</param>
        /// <returns>String representation suitable for storage</returns>
        string GetString(T data)
        {
            string result = "";

            // Enum types can be converted directly to string (uses enum name)
            if (typeof(T).IsEnum)
            {
                result = (T)(object)data + "";
            }
            else
            {
                // Use registered conversion function for non-enum types
                var typeT = typeof(T);
                if (SetValueDefines.ContainsKey(typeT))
                {
                    result = SetValueDefines[typeT].Invoke(data);
                }
                else
                {
                    GetValueFromStringError("Invalid or unsupported type!");
                }
            }
            return result;
        }

        /*
         * SUPPORTED TYPES:
         * - Primitive types: int, long, float, double, bool
         * - Special types: DateTime (stored as ticks)
         * - Unity math types: Vector2/3, Vector2Int/3Int, Ray/Ray2D, Bounds/BoundsInt, Rect/RectInt
         * - Enum types: Any enum (stored as string name)
         * 
         * ADDING NEW TYPES:
         * 1. Add entry to GetValueDefines with deserialization logic
         * 2. Add corresponding entry to SetValueDefines with serialization logic
         * 3. Ensure round-trip compatibility (serialize -> deserialize -> same value)
         * 4. Consider precision loss for floating-point types
         * 5. For complex types, create wrapper classes for JSON serialization
         * 
         * PERFORMANCE NOTES:
         * - Dictionary lookups are O(1) but have overhead
         * - JSON serialization is slower than primitive conversion
         * - Enum conversion uses reflection internally
         * - Consider caching for frequently accessed types
         */
    }
}