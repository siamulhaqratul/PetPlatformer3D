using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Read-only wrapper for PlayerStorage<T> that prevents accidental modifications.
    /// Provides a safe interface for systems that only need to read data and react to changes.
    /// Useful for implementing observer patterns and preventing unauthorized data mutations.
    /// </summary>
    /// <typeparam name="T">Must be a struct type, same constraint as PlayerStorage<T></typeparam>
    public class PlayerStorageReader<T> where T : struct
    {
        /// <summary>
        /// Internal reference to the actual PlayerStorage instance.
        /// Private to prevent external access to write operations.
        /// </summary>
        PlayerStorage<T> handle = null;

        /// <summary>
        /// Creates a new read-only wrapper by creating a new PlayerStorage instance internally.
        /// Use this constructor when you want to create a reader for new data.
        /// The underlying PlayerStorage will handle all persistence operations.
        /// </summary>
        /// <param name="identifier">Unique key for the data storage</param>
        /// <param name="defaultValue">Default value if no saved data exists</param>
        public PlayerStorageReader(string identifier, T defaultValue)
        {
            // Create new PlayerStorage instance with full read/write capabilities
            // but only expose read operations through this wrapper
            handle = new PlayerStorage<T>(identifier, defaultValue);
        }

        /// <summary>
        /// Creates a read-only wrapper around an existing PlayerStorage instance.
        /// Use this constructor when you want to provide read-only access to existing storage.
        /// Allows sharing the same data across multiple systems with controlled access.
        /// </summary>
        /// <param name="handle">Existing PlayerStorage instance to wrap</param>
        public PlayerStorageReader(PlayerStorage<T> handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// Read-only access to the stored value.
        /// Provides the same performance as direct PlayerStorage access (uses cached value).
        /// No write operations are exposed, preventing accidental data modification.
        /// </summary>
        public T Value { get { return handle.Value; } }

        /// <summary>
        /// Subscribes to update events that fire on every value assignment.
        /// Provides a simplified callback interface that automatically passes the current value.
        /// Use for operations that need to run on every assignment regardless of value change.
        /// </summary>
        /// <param name="logic">Callback function that receives the current value</param>
        public void AddLogicOnUpdate(OnDoAnything<T> logic)
        {
            // Wrap the generic OnDoAnything delegate to match UnityEvent expectations
            // Automatically provides current value to the callback for convenience
            handle.OnUpdateData.AddListener(() => { logic?.Invoke(handle.Value); });
        }

        /// <summary>
        /// Subscribes to change events that fire only when the value actually changes.
        /// Provides a simplified callback interface that automatically passes the new value.
        /// Use for operations that should only run when the stored value changes.
        /// </summary>
        /// <param name="logic">Callback function that receives the new value</param>
        public void AddLogicOnChange(OnDoAnything<T> logic)
        {
            // Convert parameterless change event into value-providing callback
            // Null-safe invocation prevents crashes from unassigned delegates
            handle.OnChangeData.AddListener(() => { logic?.Invoke(handle.Value); });
        }

        /// <summary>
        /// Subscribes to change events with access to both old and new values.
        /// Provides before/after comparison capability for delta calculations or logging.
        /// Use when you need to compare previous and current values during changes.
        /// </summary>
        /// <param name="logic">Callback function that receives (oldValue, newValue)</param>
        public void AddLogicOnChange2(OnDoAnything<T,T> logic)
        {
            // Direct mapping from UnityEvent<T,T> to OnDoAnything<T,T>
            // Maintains parameter order: previous value first, new value second
            handle.OnChangeData2.AddListener((previous, now) => { logic?.Invoke(previous, now); });
        }

        /*
         * DESIGN PATTERNS AND USE CASES:
         * 
         * 1. READ-ONLY ACCESS CONTROL:
         *    - Prevents accidental writes in read-only systems
         *    - Enables safe data sharing between systems
         *    - Enforces separation of concerns (readers vs writers)
         * 
         * 2. OBSERVER PATTERN IMPLEMENTATION:
         *    - UI systems that display data but shouldn't modify it
         *    - Analytics systems that track changes
         *    - Validation systems that react to data changes
         * 
         * 3. SIMPLIFIED EVENT SUBSCRIPTION:
         *    - Automatic value passing eliminates boilerplate code
         *    - Type-safe delegate usage instead of UnityEvent reflection
         *    - Consistent null-safe invocation patterns
         * 
         * 4. PERFORMANCE CHARACTERISTICS:
         *    - No additional overhead for value access (direct passthrough)
         *    - Event subscription adds minimal lambda wrapper overhead
         *    - Same caching benefits as underlying PlayerStorage
         * 
         * MISSING FUNCTIONALITY (Intentionally Omitted):
         * - No write access to Value property
         * - No access to RAMValue or other write variants
         * - No direct access to underlying PlayerStorage handle
         * - No save/clear operations
         * 
         * POTENTIAL EXTENSIONS:
         * - PlayerStorageWriter<T> class for write-only access
         * - Generic IPlayerStorageReader<T> interface for testing
         * - Validation hooks for read operations
         * - Read-only access to metadata (DataKey, DefaultData)
         */
    }

    // Note: The code appears to reference OnDoAnything<T> delegate types
    // These should be defined elsewhere in the codebase as:
    // public delegate void OnDoAnything<T>(T value);
    // public delegate void OnDoAnything<T, U>(T first, U second);
}