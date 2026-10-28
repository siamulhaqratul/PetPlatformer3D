using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Accessor properties and methods for PlayerStorage<T> class.
    /// Provides different ways to access and modify stored data with various event triggering behaviors.
    /// </summary>
    public partial class PlayerStorage<T> where T : struct
    {
        /// <summary>
        /// Main property for accessing stored data with full event system support.
        /// Triggers all events (RAM, Device, and general) and persists changes to disk.
        /// Use this for most game logic where you need complete event notification.
        /// </summary>
        public T Value
        {
            get
            {
                // Return the cached in-memory value for performance
                return cachedValue;
            }
            set
            {
                // Store previous value for change comparison
                var prevValue = vl;
                var nowValue = value;
                
                // Update both internal storage variables
                vl = value;
                cachedValue = value;

#if UNITY_EDITOR
                // Special handling for editor mode (when game is not playing)
                if (!Application.isPlaying)
                {
                    // Immediately save to disk in editor for inspector changes
                    var str = GetString(cachedValue);
                    PlayerStorageManager.SaveData.Save(identifier, str, defaultValData, partition, typeof(T));
                    PlayerStorageManager.SaveData.WriteMasterSaveToDevice();
                    return;
                }
#endif
                // Trigger update events (fired on every assignment)
                OnUpdateData?.Invoke();
                OnUpdateRAMData?.Invoke();
                OnUpdateDeviceData?.Invoke();
                
                // Only trigger change events if value actually changed
                if (Equals(prevValue, nowValue) == false)
                {
                    // Fire all change events with no parameters
                    OnChangeData?.Invoke();
                    OnChangeRAMData?.Invoke();
                    OnChangeDeviceData?.Invoke();
                    
                    // Fire change events with before/after values
                    OnChangeData2?.Invoke(prevValue, nowValue);
                    OnChangeRAMData2?.Invoke(prevValue, nowValue);
                    OnChangeDeviceData2?.Invoke(prevValue, nowValue);
                    
                    // Save the new value to the storage system
                    var str = GetString(cachedValue);
                    PlayerStorageManager.SaveData.Save(identifier, str, defaultValData, partition, typeof(T));
                    
                    // Optionally write to disk immediately based on configuration
                    if (UConfig.WriteSaveWhenEverDataChanged)
                    {
                        PlayerStorageManager.SaveData.WriteMasterSaveToDevice();
                    }
                    
                    // Notify the save system that write operation occurred
                    SaveDataWriteOnDevice.OnAnyWriteOperation();
                }
            }
        }

        /// <summary>
        /// Manually triggers all update and change events without modifying the value.
        /// Useful for forcing event notifications when external systems need to be notified.
        /// Also ensures the current value is saved to the storage system.
        /// </summary>
        public void SyncValues()
        {
            // Fire all update events
            OnUpdateData?.Invoke();
            OnUpdateRAMData?.Invoke();
            OnUpdateDeviceData?.Invoke();

            // Fire all change events (even though value didn't actually change)
            OnChangeData?.Invoke();
            OnChangeRAMData?.Invoke();
            OnChangeDeviceData?.Invoke();
            
            // Save current value to storage system
            var str = GetString(cachedValue);
            PlayerStorageManager.SaveData.Save(identifier, str, defaultValData, partition, typeof(T));
            
            // Write to disk if configured for immediate saves
            if (UConfig.WriteSaveWhenEverDataChanged)
            {
                PlayerStorageManager.SaveData.WriteMasterSaveToDevice();
            }
        }

        /// <summary>
        /// RAM-only property that updates the cached value but doesn't persist to disk.
        /// Only triggers RAM-related events. Use for temporary values or high-frequency updates
        /// that you don't want to save immediately (e.g., animation values, temporary UI state).
        /// </summary>
        public T RAMValue
        {
            get
            {
                return cachedValue;
            }
            set
            {
                var prevValue = vl;
                var nowValue = value;
                
                // Update cached values
                vl = value;
                cachedValue = value;
                
                // Only trigger RAM-specific events
                OnUpdateRAMData?.Invoke();
                if (Equals(prevValue, nowValue) == false)
                {
                    OnChangeRAMData?.Invoke();
                    OnChangeRAMData2?.Invoke(prevValue, nowValue);
                }
                // Note: No disk save operation or device events
            }
        }

        /// <summary>
        /// Silent RAM-only property that updates the value without triggering any events.
        /// Use for internal operations, bulk updates, or when you need to set values
        /// without side effects. No events are fired and no disk save occurs.
        /// </summary>
        public T RAMValue_WithoutEvents
        {
            get
            {
                return cachedValue;
            }
            set
            {
                var prevValue = vl;
                var nowValue = value;
                
                // Update values silently - no events triggered
                vl = value;
                cachedValue = value;
                // Note: No events fired, no disk save
            }
        }

        /// <summary>
        /// Device-focused property that saves to disk but only triggers device-related events.
        /// Useful when you want persistence without triggering general game logic events.
        /// Only fires device events, not general or RAM events.
        /// </summary>
        public T Value_EventsAreOnlyDevice
        {
            get
            {
                return cachedValue;
            }
            set
            {
                var prevValue = vl;
                var nowValue = value;
                
                // Update cached values
                vl = value;
                cachedValue = value;

#if UNITY_EDITOR
                // Handle editor mode saves
                if (!Application.isPlaying)
                {
                    var str = GetString(cachedValue);
                    PlayerStorageManager.SaveData.Save(identifier, str, defaultValData, partition, typeof(T));
                    PlayerStorageManager.SaveData.WriteMasterSaveToDevice();
                    return;
                }
#endif
                // Only trigger device-related events
                OnUpdateDeviceData?.Invoke();
                if (Equals(prevValue, nowValue) == false)
                {
                    OnChangeDeviceData?.Invoke();
                    OnChangeDeviceData2?.Invoke(prevValue, nowValue);
                    
                    // Save to storage system
                    var str = GetString(cachedValue);
                    PlayerStorageManager.SaveData.Save(identifier, str, defaultValData, partition, typeof(T));
                    
                    // Write to disk if configured
                    if (UConfig.WriteSaveWhenEverDataChanged)
                    {
                        PlayerStorageManager.SaveData.WriteMasterSaveToDevice();
                    }
                    // Note: No SaveDataWriteOnDevice.OnAnyWriteOperation() call for device-only events
                }
            }
        }
    }
}