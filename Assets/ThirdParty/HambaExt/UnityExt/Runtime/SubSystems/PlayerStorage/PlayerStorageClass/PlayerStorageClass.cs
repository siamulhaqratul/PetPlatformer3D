using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Generic player storage container for persistent reference type data.
    /// Similar to PlayerStorage<T> but specifically designed for reference types (classes).
    /// Provides event-driven access to saved reference values with automatic persistence to disk.
    /// Supports strings and any JSON-serializable class types.
    /// </summary>
    /// <typeparam name="T">Must be a reference type (class constraint for proper null handling)</typeparam>
    public partial class PlayerStorageClass<T> where T : class
    {
        /// <summary>
        /// Creates a new PlayerStorageClass instance for a specific reference type data value.
        /// Automatically initializes the storage system, loads existing data, and sets up persistence.
        /// Handles null values properly throughout the entire lifecycle.
        /// </summary>
        /// <param name="identifier">Unique key for this data (will be optimized based on UConfig settings)</param>
        /// <param name="defaultValue">Value to use when no saved data exists (can be null for reference types)</param>
        /// <param name="optionalExplicitPartition">Specific partition to use (-1 for auto-assignment)</param>
        public PlayerStorageClass(string identifier, T defaultValue, int optionalExplicitPartition = -1)
        {
            // Apply identifier optimization (hashing) to reduce save file size
            // Same optimization strategy as struct-based PlayerStorage
            var uid = PlayerStorageSaveUtil.MakeIDSmaller(identifier);
            
            // Store initialization parameters
            this.defaultVal = defaultValue; // Can be null for reference types
            this.identifier = uid;
            
            // Pre-serialize default value for storage operations
            // Handles null values through JSON serialization or direct string conversion
            var defaultValueString = GetString(defaultValue);
            this.defaultValData = defaultValueString;
            
            // Initialize the storage system if not already done
            // Loads master save data and sets up the persistence infrastructure
            if (PlayerStorageManager.InitDone == false) { PlayerStorageManager.InitSystem(); }
            
            // Ensure save writer component exists for automatic persistence
            var writer = SaveDataWriteOnDevice.Instance;
            if (writer == null && Application.isPlaying)
            {
                // Create storage writer GameObject with hardcoded name
                // This component handles automatic saves, timers, and WebGL refresh detection
                var g = new GameObject("__STORAGE__WRITER__");
                writer = g.AddComponent<SaveDataWriteOnDevice>();
            }

            // Determine partition assignment strategy
            // Auto-assignment distributes data across partitions for balanced performance
            // Explicit assignment allows grouping related reference type data together
            partition = optionalExplicitPartition == -1 ? 
                PlayerStorageManager.SaveData.GetOrCreatePartitionNumberOnPlayerStorageHandleCreation(uid, defaultValueString) : 
                optionalExplicitPartition;
            
            // Load existing value from persistent storage
            // Returns defaultValueString if no saved data exists
            var curValue = PlayerStorageManager.SaveData.Load(uid, defaultValueString, partition, typeof(T));
            
            // Deserialize loaded string back to reference type
            // Uses JSON deserialization for classes, direct conversion for strings
            vl = GetVal(curValue);
            
            // Ensure loaded value is saved back to storage system
            // Handles cases where save format changed or data needs normalization
            PlayerStorageManager.SaveData.Save(uid, curValue, defaultValueString, partition, typeof(T));
            
            // Cache current value for fast property access
            cachedValue = vl;

#if UNITY_EDITOR
            // Skip runtime-only setup when not in play mode
            if (!Application.isPlaying) { return; }
#endif

            // Register with save writer for WebGL refresh detection and auto-saves
            // Lambda captures current state to ensure fresh serialization during save operations
            if (writer != null)
            {
                writer.AddLogicOnRefresh(() =>
                {
                    var str = GetString(cachedValue);
                    PlayerStorageManager.SaveData?.Save(uid, str, defaultValueString, partition, typeof(T));
                });
            }
        }

        #region Core Data Fields
        /// <summary>Partition number for data organization and performance optimization</summary>
        int partition = -1;
        
        /// <summary>Internal storage for the current reference value (can be null)</summary>
        T vl = default; // For reference types, default is null
        
        /// <summary>Optimized unique identifier for this storage entry (may be hashed)</summary>
        string identifier;
        
        /// <summary>Gets the optimized storage key used internally (may differ from original identifier)</summary>
        public string DataKey => identifier;
        
        /// <summary>Default value returned when no saved data exists (can be null for reference types)</summary>
        T defaultVal = default; // For reference types, default is null
        
        /// <summary>Gets the default value that will be returned if no saved data exists</summary>
        public T DefaultData => defaultVal;

        /// <summary>String representation of default value for storage operations</summary>
        string defaultValData = default;
        
        /// <summary>Cached current value for fast read access without storage system calls</summary>
        T cachedValue;
        #endregion

        /*
         * KEY DIFFERENCES FROM PlayerStorage<T> (struct version):
         * 
         * 1. REFERENCE TYPE SUPPORT:
         *    - Can store null values properly
         *    - Uses reference equality for change detection
         *    - Supports any JSON-serializable class
         * 
         * 2. SERIALIZATION STRATEGY:
         *    - Strings: Direct conversion (no JSON overhead)
         *    - Classes: JSON serialization via Unity's JsonUtility
         *    - Null handling built into serialization process
         * 
         * 3. PERFORMANCE CHARACTERISTICS:
         *    - No boxing/unboxing overhead
         *    - JSON serialization slower than primitive conversion
         *    - Reference comparison typically faster than value comparison
         * 
         * 4. MEMORY MANAGEMENT:
         *    - Objects managed by garbage collector
         *    - Multiple instances can reference same object
         *    - Changes to referenced objects don't trigger save automatically
         * 
         * 5. EVENT MODEL:
         *    - Simplified event system (no RAM/Device separation)
         *    - Events trigger on reference changes, not object content changes
         * 
         * USAGE SCENARIOS:
         * - Player profile data (names, settings, preferences)
         * - Complex game state objects (inventory, quest data)
         * - Serializable configuration objects
         * - Any reference type that needs persistence
         * 
         * LIMITATIONS:
         * - Unity JsonUtility limitations apply
         * - Circular references not supported
         * - Polymorphism may not be preserved
         * - Large objects impact serialization performance
         */
    }
}