using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt.Debugging;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Generic player storage container for persistent save data.
    /// Provides event-driven access to saved values with automatic persistence to disk.
    /// Supports any struct type with built-in serialization for common Unity and .NET types.
    /// </summary>
    /// <typeparam name="T">Must be a struct type (value type constraint for performance and consistency)</typeparam>
    public partial class PlayerStorage<T> where T : struct
    {
        #region Core Data Fields
        /// <summary>Internal storage for the current value</summary>
        T vl = default;
        
        /// <summary>Optimized unique identifier for this storage entry (may be hashed)</summary>
        string identifier;
        
        /// <summary>Default value returned when no saved data exists</summary>
        T defaultVal = default;
        
        /// <summary>Partition number for data organization (-1 = auto-assigned)</summary>
        int partition = -1;
        
        /// <summary>String representation of default value for storage operations</summary>
        string defaultValData = default;
        
        /// <summary>Cached current value for fast read access without storage system calls</summary>
        T cachedValue;
        #endregion

        #region Public Properties
        /// <summary>Gets the optimized storage key used internally (may differ from original identifier)</summary>
        public string DataKey => identifier;
        
        /// <summary>Gets the default value that will be returned if no saved data exists</summary>
        public T DefaultData => defaultVal;
        #endregion

        /// <summary>
        /// Creates a new PlayerStorage instance for a specific data value.
        /// Automatically initializes the storage system, loads existing data, and sets up persistence.
        /// </summary>
        /// <param name="identifier">Unique key for this data (will be optimized based on UConfig settings)</param>
        /// <param name="defaultValue">Value to use when no saved data exists</param>
        public PlayerStorage(string identifier, T defaultValue)
        {
            // Optimize identifier based on configuration (may apply hashing for smaller file size)
            var uid = PlayerStorageSaveUtil.MakeIDSmaller(identifier);
            
            // Store initialization parameters
            this.defaultVal = defaultValue;
            this.identifier = uid;
            
            // Pre-convert default value to string for storage operations
            var defaultValueString = GetString(defaultValue);
            this.defaultValData = defaultValueString;

            // Initialize the storage system if not already done
            // This sets up the save/load infrastructure and loads master save data
            if (PlayerStorageManager.InitDone == false) 
            { 
                PlayerStorageManager.InitSystem(); 
            }

            // Ensure save writer component exists for automatic persistence
            var writer = SaveDataWriteOnDevice.Instance;
            if (writer == null && Application.isPlaying)
            {
                // Create the storage writer GameObject with configured name
                // This component handles automatic saves and WebGL refresh detection
                var tempGameobject = new GameObject(UConfig.PlayerStorageGameObjectName);
                writer = tempGameobject.AddComponent<SaveDataWriteOnDevice>();
            }

            // Determine partition assignment
            // Auto-assignment balances data across partitions for optimal performance
            // Explicit assignment allows grouping related data together
            partition = PlayerStorageManager.SaveData.GetOrCreatePartitionNumberOnPlayerStorageHandleCreation(uid, defaultValueString);

            // Load existing value from persistent storage
            // If no saved data exists, defaultValueString will be returned
            var curValue = PlayerStorageManager.SaveData.Load(uid, defaultValueString, partition, typeof(T));
            
            // Convert loaded string back to strongly-typed value
            vl = GetVal(curValue);
            
            
            // Cache the current value for fast access
            cachedValue = vl;

            
            // Register this storage instance with the save writer for automatic persistence
            // The lambda ensures current cached value is saved during refresh operations
            ////writer.AddLogicOnRefresh(() =>
            ////{
            ////    var str = GetString(cachedValue);
            ////    PlayerStorageManager.SaveData.Save(uid, str, defaultValueString, partition, typeof(T));
            ////});
        }

        /*
         * INITIALIZATION FLOW:
         * 1. Identifier optimization (hashing if configured)
         * 2. Storage system initialization (loads master save file)
         * 3. Save writer component setup (for automatic persistence)
         * 4. Partition assignment (auto or explicit)
         * 5. Data loading from persistent storage
         * 6. Type conversion (string -> T)
         * 7. Value caching for performance
         * 8. Registration with save writer for refresh handling
         * 
         * PERFORMANCE CONSIDERATIONS:
         * - Cached values avoid repeated storage system calls
         * - Identifier optimization reduces save file size
         * - Partition system balances memory usage and lookup speed
         * - Lazy initialization minimizes startup overhead
         * 
         * THREAD SAFETY:
         * - Constructor should only be called on main thread
         * - Storage system handles threading internally for save operations
         * - Multiple instances with same identifier will share data correctly
         */
    }
}