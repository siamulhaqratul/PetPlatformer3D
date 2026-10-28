using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Accessor properties for PlayerStorageClass<T> - the reference type variant of PlayerStorage.
    /// Unlike the struct-based PlayerStorage<T>, this handles reference types (classes) with null support.
    /// Provides similar event-driven access patterns but with reference type semantics.
    /// </summary>
    public partial class PlayerStorageClass<T> where T : class
    {
        /// <summary>
        /// Main property for accessing stored reference type data with full event system support.
        /// Handles null values properly and provides the same event notifications as struct variant.
        /// Use this for most game logic where you need complete event notification for reference types.
        /// </summary>
        public T Value
        {
            get
            {
                // Return the cached reference for performance
                // Note: For reference types, this returns the actual object reference
                return cachedValue;
            }
            set
            {
                // Store previous reference for change comparison
                var prevValue = vl;
                var nowValue = value;
                
                // Update both internal storage variables
                // For reference types, this copies the reference, not the object
                vl = value;
                cachedValue = value;

#if UNITY_EDITOR
                // Special handling for editor mode (when game is not playing)
                if (!Application.isPlaying)
                {
                    // Immediately save to disk in editor for inspector changes
                    // Handles null values through the serialization system
                    var str = GetString(cachedValue);
                    PlayerStorageManager.SaveData.Save(identifier, str, defaultValData, partition, typeof(T));
                    PlayerStorageManager.SaveData.WriteMasterSaveToDevice();
                    return;
                }
#endif
                // Always trigger update events (fired on every assignment, even if same reference)
                OnUpdateData?.Invoke();
                
                // Reference equality check - only trigger change events if references differ
                // Note: Uses Equals() which for reference types typically checks reference equality
                // unless the class overrides Equals() for value-based comparison
                if (Equals(prevValue, nowValue) == false)
                {
                    // Fire change events with no parameters
                    OnChangeData?.Invoke();
                    
                    // Fire change events with before/after references
                    OnChangeData2?.Invoke(prevValue, nowValue);
                    
                    // Serialize the new reference and save to storage system
                    // For reference types, this serializes the object state, not just the reference
                    var str = GetString(cachedValue);
                    PlayerStorageManager.SaveData.Save(identifier, str, defaultValData, partition, typeof(T));
                    
                    // Optionally write to disk immediately based on configuration
                    if (UConfig.WriteSaveWhenEverDataChanged)
                    {
                        PlayerStorageManager.SaveData.WriteMasterSaveToDevice();
                    }
                    
                    // Notify the save system that a write operation occurred
                    SaveDataWriteOnDevice.OnAnyWriteOperation();
                }
            }
        }

        /*
         * KEY DIFFERENCES FROM STRUCT-BASED PlayerStorage<T>:
         * 
         * 1. REFERENCE SEMANTICS:
         *    - Stores object references, not copies
         *    - Null values are supported and handled properly
         *    - Equality comparison uses reference equality by default
         * 
         * 2. SERIALIZATION CHALLENGES:
         *    - Reference types require more complex serialization
         *    - Circular references could cause issues
         *    - Polymorphism may not be preserved during serialization
         * 
         * 3. PERFORMANCE IMPLICATIONS:
         *    - No boxing/unboxing overhead (unlike structs)
         *    - Potential garbage collection impact from object allocation
         *    - Reference comparison is typically faster than value comparison
         * 
         * 4. MEMORY CONSIDERATIONS:
         *    - Multiple PlayerStorageClass instances can reference the same object
         *    - Changes to the referenced object affect all instances sharing it
         *    - Object lifetime managed by garbage collector
         * 
         * 5. NULL HANDLING:
         *    - Default values can be null
         *    - Serialization system must handle null gracefully
         *    - Event handlers should expect null values
         * 
         * USAGE RECOMMENDATIONS:
         * - Use for complex game data objects (player stats, inventory items)
         * - Ensure referenced classes are properly serializable
         * - Be cautious with shared object references
         * - Consider implementing custom Equals() for value-based comparison if needed
         */
    }
}