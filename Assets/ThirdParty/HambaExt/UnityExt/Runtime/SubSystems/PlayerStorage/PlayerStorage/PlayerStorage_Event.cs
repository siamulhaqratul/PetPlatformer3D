using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Event system for PlayerStorage<T> class.
    /// Provides three categories of events: Normal (general), RAM (memory-only), and Device (persistence-focused).
    /// Each category has Update events (fired on every assignment) and Change events (fired only when value differs).
    /// </summary>
    public partial class PlayerStorage<T> where T : struct
    {
        #region NormalEvents
        // General-purpose events that fire for most PlayerStorage operations
        // These are the primary events used by game logic systems
        
        /// <summary>
        /// Fired every time the Value property is assigned, regardless of whether the value changed.
        /// Use for operations that need to run on every assignment (UI refresh, validation, etc.)
        /// </summary>
        UnityEngine.Events.UnityEvent onUpdateData;
        public UnityEngine.Events.UnityEvent OnUpdateData
        {
            get
            {
                // Lazy initialization to avoid creating events that are never used
                if (onUpdateData == null) { onUpdateData = new UnityEngine.Events.UnityEvent(); }
                return onUpdateData;
            }
        }

        /// <summary>
        /// Fired only when the Value property is assigned and the new value differs from the old value.
        /// Use for operations that should only run when actual changes occur (save operations, notifications).
        /// Provides no parameters - use OnChangeData2 if you need old/new values.
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
        /// Fired only when the Value property changes, providing both old and new values.
        /// Use when you need to compare before/after values (animations, delta calculations, logging).
        /// Parameters: (T oldValue, T newValue)
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

        #region RAMEvents
        // Memory-only events that fire for RAMValue property operations
        // These events fire for temporary/transient data changes that don't persist to disk
        // Use for high-frequency updates, animations, or temporary UI states
        
        /// <summary>
        /// Fired every time the RAMValue property is assigned.
        /// Use for memory-only operations that don't need persistence (temporary animations, UI states).
        /// </summary>
        UnityEngine.Events.UnityEvent onUpdateRAMData;
        public UnityEngine.Events.UnityEvent OnUpdateRAMData
        {
            get
            {
                if (onUpdateRAMData == null) { onUpdateRAMData = new UnityEngine.Events.UnityEvent(); }
                return onUpdateRAMData;
            }
        }

        /// <summary>
        /// Fired only when the RAMValue property changes (new value differs from old).
        /// Use for memory-only change detection without persistence overhead.
        /// </summary>
        UnityEngine.Events.UnityEvent onChangeRAMData;
        public UnityEngine.Events.UnityEvent OnChangeRAMData
        {
            get
            {
                if (onChangeRAMData == null) { onChangeRAMData = new UnityEngine.Events.UnityEvent(); }
                return onChangeRAMData;
            }
        }

        /// <summary>
        /// Fired when RAMValue changes, providing old and new values.
        /// Use for memory-only operations that need before/after comparison.
        /// Parameters: (T oldValue, T newValue)
        /// </summary>
        UnityEngine.Events.UnityEvent<T, T> onChangeRAMData2;
        public UnityEngine.Events.UnityEvent<T, T> OnChangeRAMData2
        {
            get
            {
                if (onChangeRAMData2 == null) { onChangeRAMData2 = new UnityEngine.Events.UnityEvent<T, T>(); }
                return onChangeRAMData2;
            }
        }
        #endregion

        #region DeviceEvents
        // Persistence-focused events that fire when data is saved to disk
        // These events are triggered by operations that affect persistent storage
        // Use for save confirmations, backup operations, or persistence-related logging
        
        /// <summary>
        /// Fired every time a value assignment triggers a save operation to persistent storage.
        /// Use for save confirmations, backup operations, or storage-related housekeeping.
        /// </summary>
        UnityEngine.Events.UnityEvent onUpdateDeviceData;
        public UnityEngine.Events.UnityEvent OnUpdateDeviceData
        {
            get
            {
                if (onUpdateDeviceData == null) { onUpdateDeviceData = new UnityEngine.Events.UnityEvent(); }
                return onUpdateDeviceData;
            }
        }

        /// <summary>
        /// Fired only when a value change is persisted to disk.
        /// Use for operations that should only run when persistent data actually changes.
        /// </summary>
        UnityEngine.Events.UnityEvent onChangeDeviceData;
        public UnityEngine.Events.UnityEvent OnChangeDeviceData
        {
            get
            {
                if (onChangeDeviceData == null) { onChangeDeviceData = new UnityEngine.Events.UnityEvent(); }
                return onChangeDeviceData;
            }
        }

        /// <summary>
        /// Fired when a value change is persisted, providing old and new values.
        /// Use for logging persistent changes, analytics, or backup systems that need change deltas.
        /// Parameters: (T oldValue, T newValue)
        /// </summary>
        UnityEngine.Events.UnityEvent<T, T> onChangeDeviceData2;
        public UnityEngine.Events.UnityEvent<T, T> OnChangeDeviceData2
        {
            get
            {
                if (onChangeDeviceData2 == null) { onChangeDeviceData2 = new UnityEngine.Events.UnityEvent<T, T>(); }
                return onChangeDeviceData2;
            }
        }
        #endregion
        
        /*
         * EVENT USAGE GUIDE:
         * 
         * Use Normal Events when:
         * - General game logic needs to respond to data changes
         * - UI needs to update when values change
         * - You don't care about the specific type of operation
         * 
         * Use RAM Events when:
         * - Working with temporary/transient data
         * - High-frequency updates that shouldn't persist
         * - Animation values or temporary UI states
         * - Performance-critical code that avoids disk I/O
         * 
         * Use Device Events when:
         * - Implementing save confirmations or notifications
         * - Logging persistent data changes
         * - Backup or synchronization systems
         * - Analytics that track actual save operations
         * 
         * Event Timing:
         * - Update events fire on EVERY assignment
         * - Change events fire only when old != new
         * - Change2 events provide both old and new values
         * 
         * Performance Notes:
         * - Events are lazily initialized (created only when accessed)
         * - UnityEvent has some overhead - consider C# events for high-frequency scenarios
         * - RAM events don't trigger disk I/O operations
         */
    }
}