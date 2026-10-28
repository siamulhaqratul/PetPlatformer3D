using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityExt.Debugging;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Utility class providing helper functions for the player storage system.
    /// Contains methods for ID optimization, validation, data conversion, and deep copying.
    /// </summary>
    internal static class PlayerStorageSaveUtil 
    {
        /// <summary>
        /// Optimizes string identifiers using various hashing algorithms to reduce save file size.
        /// Trade-off: Smaller file size vs. collision risk (data loss potential).
        /// </summary>
        /// <param name="input">Original identifier string to optimize</param>
        /// <returns>Optimized identifier or original string if no optimization configured</returns>
        internal static string MakeIDSmaller(string input)
        {
            string result = input;

            // SHA256: Most secure but slower, 64-character hex output
            if (UConfig.UIDOptimization == UIDOptimizationMethod.SHA256)
            {
                //https://stackoverflow.com/questions/63615950/generate-unique-id-from-string-in-c-sharp
                // Using the SHA256 algorithm for the hash.
                // NOTE: You can replace it with any other algorithm (e.g. MD5) if you need.
                using (var hashAlgorithm = SHA256.Create())
                {
                    // Convert the input string to a byte array and compute the hash.
                    byte[] data = hashAlgorithm.ComputeHash(EncodingUtil.GetBytes(input));

                    // Create a new Stringbuilder to collect the bytes
                    // and create a string.
                    var sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data
                    // and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    // Return the hexadecimal string.
                    result = sBuilder.ToString();
                }
            }
            // MD5: Faster but higher collision risk, shorter output
            else if (UConfig.UIDOptimization == UIDOptimizationMethod.MD5)
            {
                //https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
                // Use input string to calculate MD5 hash
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert to Base64 for shorter representation (.NET 5+)
                    result = Convert.ToBase64String(hashBytes); // .NET 5 +

                    // Convert the byte array to hexadecimal string prior to .NET 5
                    // StringBuilder sb = new System.Text.StringBuilder();
                    // for (int i = 0; i < hashBytes.Length; i++)
                    // {
                    //     sb.Append(hashBytes[i].ToString("X2"));
                    // }
                    // return sb.ToString();
                }
            }
            // Custom: SHA1-based XOR method, moderate collision risk
            else if (UConfig.UIDOptimization == UIDOptimizationMethod.Custom)
            {
                if (input.IsStringValid())
                {
                    //https://stackoverflow.com/questions/63615950/generate-unique-id-from-string-in-c-sharp
                    //Unicode Encode Covering all characterset
                    var hash = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                    byte[] byteContents = EncodingUtil.GetBytes(input);
                    byte[] hashText = hash.ComputeHash(byteContents);
                    
                    // Extract three 32-bit segments from the 160-bit SHA1 hash
                    uint hashCodeStart = BitConverter.ToUInt32(hashText, 0);
                    uint hashCodeMedium = BitConverter.ToUInt32(hashText, 8);
                    uint hashCodeEnd = BitConverter.ToUInt32(hashText, 16);
                    
                    // XOR the segments together for final hash
                    var hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
                    
                    // Invert to reduce predictability and convert to string
                    result = (uint.MaxValue - hashCode) + "";
                }
            }
            // None: No optimization, return original string
            return result;
        }
        
        /// <summary>
        /// Validates ProcessDesc structure to ensure all required fields are properly set.
        /// Prevents save data corruption by catching invalid operations early.
        /// </summary>
        /// <param name="desc">Process descriptor to validate</param>
        /// <param name="dataType">Type of data being processed for validation rules</param>
        /// <returns>True if descriptor is valid for the intended operation</returns>
        internal static bool IsProcessDescValid(this ProcessDesc desc, System.Type dataType)
        {
            // For non-string types, default value must be valid
            bool defaultValueValid = true;
            if (dataType != typeof(string))
            {
                defaultValueValid = desc.defaultValue.IsStringValid();
            }

            // Key must always be valid (non-null, non-empty)
            var keyValid = desc.key.IsStringValid();
            
            // Partition number must be non-negative
            var partitionValid = desc.partition >= 0;
            var shouldSave = desc.shouldSave;

            // For save operations on non-string types, save data must be valid
            bool isSaveDataStrValid = true;
            if (dataType != typeof(string))
            {
                isSaveDataStrValid = desc.saveData.IsStringValid();
            }

            // Validation logic: key and partition always required
            // For save operations, save data must be valid
            // For load operations, save data validation is skipped
            bool result = false;
            result = keyValid && partitionValid &&
                ((shouldSave && isSaveDataStrValid) || (!shouldSave));

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            // Detailed error logging for development debugging
            if (!result)
            {
                GLog.Throw("save data process invalid-details-- 'defaultValue str': " + desc.defaultValue + Environment.NewLine +
                    "'defaultValueValid: '" + defaultValueValid + Environment.NewLine + 
                    " 'keyValid': " + keyValid + Environment.NewLine +
                    " 'partitionValid': " + partitionValid + Environment.NewLine +
                    " 'shouldSave': " + shouldSave + Environment.NewLine +
                    " 'isSaveDataStrValid': " + isSaveDataStrValid);
            }
#endif
            return result;
        }

        /// <summary>
        /// Creates a new default master save structure with game metadata.
        /// Used when no existing save data is found or system initialization.
        /// </summary>
        /// <typeparam name="T">Master save type (must inherit from MasterSaveJsonDefault)</typeparam>
        /// <returns>New master save instance with game name and bundle ID populated</returns>
        internal static T GetDefaultMasterSave<T>() where T : MasterSaveJsonDefault
        {
            var sv = new MasterSaveJsonDefault()
            {
                // Populate with current application metadata
                gameBundleID = Application.identifier,
                gameName = Application.productName,
            };
            return (T)(object)sv;
        }

        /// <summary>
        /// Creates a deep copy of master save data for thread-safe operations.
        /// Essential for threaded save operations to prevent data corruption.
        /// </summary>
        /// <typeparam name="T">Master save type</typeparam>
        /// <param name="input">Original master save to copy</param>
        /// <returns>Complete deep copy with all partitions and entries duplicated</returns>
        internal static T CopyMasterSave<T>(T input) where T : MasterSaveJsonDefault
        {
            // Copy basic metadata
            var sv = new MasterSaveJsonDefault()
            {
                gameBundleID = input.gameBundleID,
                gameName = input.gameName,
            };
            
            // Deep copy all partitions if they exist
            var partLen = input.partitions.ExIsValid() ? input.partitions.Length : 0;
            if (partLen > 0)
            {
                sv.partitions = new DataEntryPartition[partLen];
                for (int i = 0; i < partLen; i++)
                {
                    var inputPartition = input.partitions[i];
                    var copiedPartition = new DataEntryPartition { partition = inputPartition.partition };
                    
                    // Deep copy all entries within this partition
                    var pEntriesLen = inputPartition.entries.ExIsValid() ? inputPartition.entries.Length : 0;
                    if (pEntriesLen > 0)
                    {
                        copiedPartition.entries = new DataEntry[pEntriesLen];
                        for (int j = 0; j < pEntriesLen; j++)
                        {
                            // Create new DataEntry instances to avoid reference sharing
                            copiedPartition.entries[j] = new DataEntry { 
                                key = inputPartition.entries[j].key, 
                                value = inputPartition.entries[j].value 
                            };
                        }
                    }
                    sv.partitions[i] = copiedPartition;
                }
            }
            return (T)(object)sv;
        }

        /// <summary>
        /// Safely deserializes JSON string to specified type with error handling.
        /// Prevents crashes from corrupted save data by graceful error handling.
        /// </summary>
        /// <typeparam name="T">Target type for deserialization</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>Deserialized object or default value if deserialization fails</returns>
        internal static T GetDataFromJson<T>(string json)
        {
            T result = default;
            try
            {
                // Use Unity's JsonUtility for deserialization
                result = JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception ex)
            {
                // Log warning but don't crash - return default value instead
                GLog.PrintWarning("Could not get data of type " + typeof(T) + " from json string. " +
                         "Exception is thrown! Err message: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Safely serializes object to JSON string with error handling.
        /// Uses configuration settings for pretty printing in development builds.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="objectToBeJsonized">Object to convert to JSON</param>
        /// <returns>JSON string or default value if serialization fails</returns>
        internal static string GetDataAsJsonString<T>(T objectToBeJsonized)
        {
            string result = default;
            try
            {
                // Use Unity's JsonUtility with optional pretty printing for debugging
                result = JsonUtility.ToJson(objectToBeJsonized, UConfig.UsePrettyPrintForSaveDataJson);
            }
            catch (System.Exception ex)
            {
                // Log warning but don't crash - return default value instead
                GLog.PrintWarning("Could not convert data of type " + typeof(T) + " to json string. " +
                        "Exception is thrown! Err message: " + ex.Message);
            }
            return result;
        }
    }
}