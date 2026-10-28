using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt.Debugging;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Helper methods and type conversion system for PlayerStorageClass<T> - reference type variant.
    /// Handles serialization/deserialization for reference types using JSON or direct string conversion.
    /// Much simpler than the struct version since it only needs to handle strings and JSON-serializable classes.
    /// </summary>
    public partial class PlayerStorageClass<T> where T : class
    {
        /// <summary>
        /// Centralized error handling for type conversion failures during reference type operations.
        /// In development builds, creates exceptions for debugging (note: not thrown, just created).
        /// In production builds, logs errors without crashing to maintain game stability.
        /// </summary>
        /// <param name="customMsg">Additional context about the conversion failure</param>
        void GetValueFromStringError(string customMsg)
        {
            var txt = "Could not parse data! additional info msg: " + customMsg;
#if DEVELOPMENT_BUILD
            // Create exception for debugging (appears to be missing 'throw' keyword)
            // This should probably be: throw new System.Exception(txt);
            new System.Exception(txt);
#else
            // Log error in production to avoid crashes but provide diagnostics
            GLog.PrintError(txt);
#endif
        }

        /// <summary>
        /// Converts string representation back to reference type instance.
        /// Handles two main cases: strings (direct conversion) and classes (JSON deserialization).
        /// Used during load operations to deserialize saved reference type data.
        /// </summary>
        /// <param name="str">String representation of the reference type data</param>
        /// <returns>Converted reference type instance or null if conversion fails</returns>
        T GetVal(string str)
        {
            T result = default; // For reference types, default is null
            
            // Special case: String type - direct conversion without JSON parsing
            if (typeof(T) == typeof(string))
            {
                // Direct cast from string to T (which is string in this case)
                result = (T)(object)str;
            }
            // General case: Reference types that need JSON deserialization
            else if (typeof(T).IsClass)
            {
                // Use Unity's JsonUtility to deserialize the JSON string back to object
                // Will return null if JSON is invalid or doesn't match the class structure
                result = JsonUtility.FromJson<T>(str);
            }
            else
            {
                // This should never happen due to the 'where T : class' constraint
                // But kept for defensive programming
                GetValueFromStringError("Invalid or unsupported type! " + typeof(T));
            }
            return result;
        }

        /// <summary>
        /// Converts reference type instance to string representation for storage.
        /// Handles two main cases: strings (direct conversion) and classes (JSON serialization).
        /// Used during save operations to serialize reference type data for persistence.
        /// </summary>
        /// <param name="data">Reference type instance to convert to string (can be null)</param>
        /// <returns>String representation suitable for storage</returns>
        string GetString(T data)
        {
            string result = "";
            
            // Special case: String type - direct conversion to avoid double JSON encoding
            if (typeof(T) == typeof(string))
            {
                // Handle null strings gracefully by converting to empty string
                result = (string)(object)data + "";
            }
            // General case: Reference types that need JSON serialization
            else if(typeof(T).IsClass)
            {
                // Use Unity's JsonUtility to serialize the object to JSON
                // Uses configuration setting for pretty printing (helpful for debugging)
                // Will handle null objects by returning "null" or appropriate JSON representation
                result = JsonUtility.ToJson((object)data, UConfig.UsePrettyPrintForSaveDataJson);
            }
            else
            {
                // This should never happen due to the 'where T : class' constraint
                // But kept for defensive programming
                GetValueFromStringError("Invalid or unsupported type!");
            }
            return result;
        }

        /*
         * REFERENCE TYPE SERIALIZATION CONSIDERATIONS:
         * 
         * 1. JSON LIMITATIONS:
         *    - Unity's JsonUtility has limitations compared to full JSON libraries
         *    - Doesn't serialize private fields by default (use [SerializeField])
         *    - Doesn't handle circular references (will cause infinite recursion)
         *    - Polymorphism isn't preserved (derived types may lose information)
         *    - Dictionary and complex collections aren't supported directly
         * 
         * 2. NULL HANDLING:
         *    - Reference types can be null, and this is handled gracefully
         *    - JsonUtility.ToJson(null) typically returns "null" string
         *    - JsonUtility.FromJson<T>("null") typically returns null
         *    - String concatenation with null produces "null" string
         * 
         * 3. PERFORMANCE IMPLICATIONS:
         *    - JSON serialization is slower than primitive type conversion
         *    - Large objects or deep hierarchies can impact performance
         *    - Consider caching serialized strings for frequently accessed objects
         * 
         * 4. TYPE SAFETY:
         *    - Much simpler than struct version due to 'where T : class' constraint
         *    - Only two main cases to handle: string and other classes
         *    - JsonUtility provides type safety during deserialization
         * 
         * 5. POTENTIAL IMPROVEMENTS:
         *    - Could add support for ISerializationCallbackReceiver
         *    - Could implement custom serialization for unsupported types
         *    - Could add validation for JSON schema compatibility
         *    - Error handling could be more robust (catch JSON exceptions)
         * 
         * SUPPORTED TYPES:
         * - string: Direct conversion, no JSON processing
         * - Any class marked with [System.Serializable]: JSON serialization via JsonUtility
         * - Classes with public fields or [SerializeField] private fields
         * 
         * UNSUPPORTED TYPES:
         * - Classes with circular references
         * - Generic classes (limitations of JsonUtility)
         * - Classes requiring custom serialization logic
         */
    }
}