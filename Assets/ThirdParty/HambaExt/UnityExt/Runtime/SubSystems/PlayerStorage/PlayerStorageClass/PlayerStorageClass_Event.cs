using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Event system for PlayerStorageClass<T> - reference type variant of PlayerStorage.
    /// Provides a simplified event model compared to the struct-based version.
    /// Only includes normal events, omitting RAM and Device event categories for simplicity.
    /// </summary>
    public partial class PlayerStorageClass<T> where T : class
    {
        #region NormalEvents
        // Simplified event model for reference types
        // Unlike PlayerStorage<T> (struct), this only provides general-purpose events
        // No separate RAM/Device events since reference type persistence is less performance-critical
        
        /// <summary>
        /// Fired every time the Value property is assigned, regardless of whether the reference changed.
        /// Use for operations that need to run on every assignment (UI refresh, validation, etc.)
        /// Note: For reference types, this fires even when assigning the same object reference.
        /// </summary>
        UnityEngine.Events.UnityEvent onUpdateData;
        public UnityEngine.Events.UnityEvent OnUpdateData
        {
            get
            {
                // Lazy initialization to avoid creating events that are never used
                // Important for memory efficiency when dealing with many storage instances
                if (onUpdateData == null) { onUpdateData = new UnityEngine.Events.UnityEvent(); }
                return onUpdateData;
            }
        }

        /// <summary>
        /// Fired only when the Value property is assigned and the new reference differs from the old reference.
        /// Uses reference equality comparison (ReferenceEquals) unless the class overrides Equals().
        /// Use for operations that should only run when the object reference actually changes.
        /// Provides no parameters - use OnChangeData2 if you need old/new reference access.
        /// </summary>
        UnityEngine.Events.UnityEvent onChangeData;
        public UnityEngine.Events.UnityEvent OnChangeData
        {
            get
            {
                if (onChangeData == null) { onChangeData = new UnityEngine.Events.UnityEvent(); }
                return onChangeData;
            }
        }

        /// <summary>
        /// Fired only when the Value property changes, providing both old and new object references.
        /// Use when you need to compare before/after references or perform cleanup on old objects.
        /// Parameters: (T oldReference, T newReference)
        /// 
        /// IMPORTANT CONSIDERATIONS FOR REFERENCE TYPES:
        /// - Parameters provide object references, not copies
        /// - Both references may be null - ensure handlers can deal with null values
        /// - If the same object is modified externally, this won't fire (reference stays same)
        /// - Useful for resource cleanup, analytics, or cross-referencing operations
        /// </summary>
        UnityEngine.Events.UnityEvent<T, T> onChangeData2;
        public UnityEngine.Events.UnityEvent<T, T> OnChangeData2
        {
            get
            {
                if (onChangeData2 == null) { onChangeData2 = new UnityEngine.Events.UnityEvent<T, T>(); }
                return onChangeData2;
            }
        }
        #endregion

        /*
         * SIMPLIFIED EVENT MODEL RATIONALE:
         * 
         * Why only Normal events (no RAM/Device events)?
         * 1. Reference types are typically used for complex objects that don't change frequently
         * 2. The overhead of separate event categories isn't justified for reference type use cases
         * 3. Reference type serialization is inherently slower, making fine-grained event control less critical
         * 4. Simplifies the API while maintaining core functionality
         * 
         * REFERENCE TYPE EVENT CONSIDERATIONS:
         * 
         * 1. NULL HANDLING:
         *    - Event handlers must be prepared to receive null references
         *    - Both old and new values in OnChangeData2 can be null
         *    - Default values for reference types are typically null
         * 
         * 2. REFERENCE VS VALUE EQUALITY:
         *    - Events trigger based on reference equality by default
         *    - If a class overrides Equals(), that will be used instead
         *    - Modifying an object's properties won't trigger events (same reference)
         * 
         * 3. MEMORY MANAGEMENT:
         *    - Event handlers may hold references to objects, affecting garbage collection
         *    - Be careful not to create reference cycles between events and stored objects
         *    - Consider weak references for long-lived event subscriptions
         * 
         * 4. PERFORMANCE IMPLICATIONS:
         *    - UnityEvent<T,T> has boxing overhead for reference types in some Unity versions
         *    - Consider C# events for high-frequency reference type changes
         *    - Reference comparison is typically fast, but custom Equals() may be expensive
         * 
         * USAGE PATTERNS:
         * - OnUpdateData: UI refresh, validation, logging every assignment
         * - OnChangeData: Save operations, notifications, simple change reactions
         * - OnChangeData2: Resource cleanup, analytics with before/after context, migration logic
         */
    }
}