using System.IO;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Root container for player save data in JSON format.
    /// Contains game metadata and partitioned data entries.
    /// </summary>
    [System.Serializable]
    internal class MasterSaveJsonDefault
    {
        [SerializeField] internal string gameName = "";
        [SerializeField] internal string gameBundleID = "";
        /// <summary>
        /// Array of data partitions to organize save data into logical groups
        /// </summary>
        [SerializeField] internal DataEntryPartition[] partitions = null;
    }

    /// <summary>
    /// Represents a logical partition of save data entries.
    /// Used to group related data together for better organization.
    /// </summary>
    [System.Serializable]
    internal class DataEntryPartition
    {
        /// <summary>
        /// Unique identifier for this partition
        /// </summary>
        [SerializeField] internal int partition;
        /// <summary>
        /// All key-value data entries belonging to this partition
        /// </summary>
        [SerializeField] internal DataEntry[] entries = null;
    }

    /// <summary>
    /// Individual save data entry containing a key-value pair
    /// </summary>
    [System.Serializable]
    internal class DataEntry
    {
        /// <summary>
        /// Unique identifier for this data entry
        /// </summary>
        [SerializeField] internal string key;
        /// <summary>
        /// Serialized data value (stored as string)
        /// </summary>
        [SerializeField] internal string value;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only utilities for managing player storage during development.
    /// Provides debugging and testing functionality.
    /// </summary>
    public static class StorageManEditorExternalCall
    {
        /// <summary>
        /// Deletes the player save file from persistent data path.
        /// Used for testing and debugging purposes.
        /// </summary>
        public static void Clear()
        {
            var fpath = Path.Combine(Application.persistentDataPath, UConfig.PlayerStorageFileName);
            if (File.Exists(fpath))
            {
                File.Delete(fpath);
            }
            PlayerStorageManager.ResetMemoryState();
            if (Application.isPlaying)
            {
                PlayerStorageManager.InitSystem();
            }
        }

        /// <summary>
        /// Retrieves information about the current save file for editor display.
        /// </summary>
        /// <param name="saveFile">Output: Full path to the save file</param>
        /// <param name="saveSize">Output: Size of save file in KB</param>
        public static void GetCurrentSaveDetails(ref string saveFile, ref long saveSize)
        {
            PlayerStorageManager.GetSaveDataDetails(ref saveFile, ref saveSize);
        }
    }
#endif

    /// <summary>
    /// Core manager for the player storage system.
    /// Handles initialization, save data access, and file operations.
    /// </summary>
    public static class PlayerStorageManager
    {
        // System initialization state
        static bool initDone = false;
        // Main save data module instance
        static PlayerStorageSaveModule save = null;

        internal static void ResetMemoryState()
        {
            initDone = false;
            save = null;
        }

        /// <summary>
        /// Returns true if the storage system has been initialized
        /// </summary>
        internal static bool InitDone => initDone;

        /// <summary>
        /// Provides access to the main save data module
        /// </summary>
        internal static PlayerStorageSaveModule SaveData
        {
            get
            {
                if (save == null)
                {
                    InitSystem();
                }
                return save;
            }
        }

        public static PlayerStorageSaveModule SaveDataCaution(string code)
        {
            if (code == "UseWithCaution")
            {
                return SaveData;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Initializes the player storage system.
        /// Creates save module, loads existing data, and optionally starts timer-based saving.
        /// </summary>
        internal static void InitSystem()
        {
            // Create new save module instance
            save = new PlayerStorageSaveModule();
            // Load any existing save data from disk into memory
            save.LoadMasterSaveToMemory();
            initDone = true;

            // Start automatic saving if configured
            if (UConfig.UseTimerBasedDataSaving)
            {
                SaveDataWriteOnDevice.StartTimerBasedWriteOperation();
            }
        }

        /// <summary>
        /// Gets information about the current save file for debugging/monitoring.
        /// </summary>
        /// <param name="saveFile">Output: Full path to the save file</param>
        /// <param name="saveSize">Output: File size in KB (0 if file doesn't exist)</param>
        internal static void GetSaveDataDetails(ref string saveFile, ref long saveSize)
        {
            // Construct full path to save file
            saveFile = Path.Combine(Application.persistentDataPath, UConfig.PlayerStorageFileName);

            // Check if save file exists
            if (!File.Exists(saveFile))
            {
                saveSize = 0;
                return;
            }

            // Get file size in KB
            var info = new FileInfo(saveFile);
            saveSize = info == null ? 0 : (long)((float)info.Length / 1000.0f);
        }
    }
}