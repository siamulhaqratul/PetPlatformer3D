using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityExt.Debugging;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Descriptor structure for processing save data operations.
    /// Contains all necessary information for loading/saving a specific data entry.
    /// </summary>
    public struct ProcessDesc
    {
        /// <summary>Partition number where the data belongs</summary>
        public int partition;
        /// <summary>Unique key identifier for the data entry</summary>
        public string key;
        /// <summary>Default value to use if data doesn't exist</summary>
        public string defaultValue;
        /// <summary>Data to save (used only during save operations)</summary>
        public string saveData;
        /// <summary>Whether this operation is a save (true) or load (false)</summary>
        public bool shouldSave;
    }

    /// <summary>
    /// Core save data module that handles all in-memory save data operations.
    /// Manages loading, saving, and manipulation of the master save data structure.
    /// </summary>
    public class PlayerStorageSaveModule
    {
        /// <summary>In-memory representation of the entire save data structure</summary>
        MasterSaveJsonDefault masterSave = null;
        
        /// <summary>
        /// Loads existing save data from persistent storage into memory.
        /// Handles different backends (PlayerPrefs vs File) and supports encryption/compression.
        /// </summary>
        internal void LoadMasterSaveToMemory()
        {
            // Handle PlayerPrefs backend
            if (UConfig.SaveDataBackend == SaveDataWriteBackend.PlayerPref)
            {
                // Create unique key using product name and identifier to avoid conflicts
                var key = UConfig.MasterDataKeyForPlayerPref + "_PROD_NAME_" + Application.productName + "_IDFR_" + Application.identifier;
                var text = PlayerPrefs.GetString(key, "");
                
                if (text.IsStringValid())
                {
                    // Decrypt if encryption is enabled
                    if (UConfig.useEncryption)
                    {
                        text = CryptoUtil.DecryptStringAESForSaveData(text);
                    }
                    
                    // Parse JSON data or create default if invalid
                    if (text.IsStringValid())
                    {
                        masterSave = PlayerStorageSaveUtil.GetDataFromJson<MasterSaveJsonDefault>(text);
                    }
                    else
                    {
                        // Create empty default save structure
                        masterSave = PlayerStorageSaveUtil.GetDefaultMasterSave<MasterSaveJsonDefault>();
                        masterSave.partitions = null;
                    }
                }
                else
                {
                    // No existing data found, create new default
                    masterSave = PlayerStorageSaveUtil.GetDefaultMasterSave<MasterSaveJsonDefault>();
                    masterSave.partitions = null;
                }
            }
            // Handle file-based backend
            else if(UConfig.SaveDataBackend == SaveDataWriteBackend.Default)
            {
                var fPath = Path.Combine(Application.persistentDataPath, UConfig.PlayerStorageFileName);
                
                if (File.Exists(fPath))
                {
                    string text = "";
                    
                    // Handle compressed data
                    if (UConfig.UseDataCompression)
                    {
                        var result = File.ReadAllBytes(fPath);
                        text = CompressionUtil.DecompressByte(result);
                        
                        // Decrypt after decompression
                        if (UConfig.useEncryption)
                        {
                            text = CryptoUtil.DecryptStringAESForSaveData(text);
                        }
                    }
                    else
                    {
                        // Read as plain text and decrypt if needed
                        text = File.ReadAllText(fPath);
                        if (UConfig.useEncryption)
                        {
                            text = CryptoUtil.DecryptStringAESForSaveData(text);
                        }
                    }
                    
                    // Parse the loaded JSON data
                    masterSave = PlayerStorageSaveUtil.GetDataFromJson<MasterSaveJsonDefault>(text);
                }
                else
                {
                    // No save file exists, create default structure
                    masterSave = PlayerStorageSaveUtil.GetDefaultMasterSave<MasterSaveJsonDefault>();
                    masterSave.partitions = null;
                }
            }   
        }
        
        /// <summary>
        /// Writes the current in-memory save data to persistent storage.
        /// Supports threading, encryption, compression, and multiple backends.
        /// </summary>
        public void WriteMasterSaveToDevice()
        {
            // Ensure any pending write operations are completed
            SaveDataWriteOnDevice.FlushWriteOperation();
            
            // Initialize master save if null
            if (masterSave == null)
            {
                masterSave = PlayerStorageSaveUtil.GetDefaultMasterSave<MasterSaveJsonDefault>();
                masterSave.partitions = null;
            }
            
            string text = "";
            
            // Handle threaded saving for better performance
            if (UConfig.UseThreadingWhenDataSaving)
            {
                // PlayerPrefs is not thread-safe, so prevent threading with that backend
                if (UConfig.SaveDataBackend == SaveDataWriteBackend.PlayerPref)
                {
                    GLog.PrintError("PlayerPref API is not thread safe. Thus please change the saving backend if you want to use multithreading for save data!");
                    return;
                }

                // Copy data and config for thread safety
                var copiedMasterSave = PlayerStorageSaveUtil.CopyMasterSave(masterSave);
                var useEncryption = UConfig.useEncryption;
                var useCompression = UConfig.UseDataCompression;
                var fileName = UConfig.PlayerStorageFileName;
                string sharedSecret = UConfig.encryptionSharedSecretForSaveData;
                string salt = UConfig.encryptionSaltForSaveData;
                var encoding = UConfig.Encoding;
                var fpath = Path.Combine(Application.persistentDataPath, fileName);
                
                // Create and start background save thread
                System.Threading.Thread saveThread = new System.Threading.Thread(() =>
                {
                    // Convert data to JSON
                    text = PlayerStorageSaveUtil.GetDataAsJsonString(copiedMasterSave);
                    
                    // Apply encryption if enabled
                    if (useEncryption)
                    {
                        text = CryptoUtil.EncryptStringAESForSaveData(text, sharedSecret, salt);
                    }
                    
                    // Handle compression and file writing
                    if (useCompression)
                    {
                        byte[] dataToSave = CompressionUtil.CompressText(text, encoding);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        try
                        {
                            File.WriteAllBytes(fpath, dataToSave);
                            Debug.Log("Threaded Game saved successfully.");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("Threaded Save failed: " + e.Message);
                        }
#else
                        File.WriteAllBytes(fpath, dataToSave);
#endif
                    }
                    else
                    {
                        // Save as plain text
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        try
                        {
                            File.WriteAllText(fpath, text);
                            Debug.Log("Threaded Game saved successfully.");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("Threaded Save failed: " + e.Message);
                        }
#else
                        File.WriteAllText(fpath, text);
#endif
                    }
                });
                
                saveThread.Start();
            }
            else
            {
                // Synchronous saving on main thread
                text = PlayerStorageSaveUtil.GetDataAsJsonString(masterSave);
                
                // Apply encryption
                if (UConfig.useEncryption)
                {
                    text = CryptoUtil.EncryptStringAESForSaveData(text);
                }
                
                // Save to appropriate backend
                if (UConfig.SaveDataBackend == SaveDataWriteBackend.PlayerPref)
                {
                    var key = UConfig.MasterDataKeyForPlayerPref + "_PROD_NAME_" + Application.productName + "_IDFR_" + Application.identifier;
                    PlayerPrefs.SetString(key, text);
                }
                else if(UConfig.SaveDataBackend == SaveDataWriteBackend.Default)
                {
                    var fpath = Path.Combine(Application.persistentDataPath, UConfig.PlayerStorageFileName);
                    
                    if (UConfig.UseDataCompression)
                    {
                        // Save compressed binary data
                        byte[] dataToSave = CompressionUtil.CompressText(text);
                        File.WriteAllBytes(fpath, dataToSave);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        Debug.Log("Game saved successfully.");
#endif
                    }
                    else
                    {
                        // Save as plain text file
                        File.WriteAllText(fpath, text);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        Debug.Log("Game saved successfully.");
#endif
                    }
                }
            }
        }
        
        /// <summary>
        /// Compares two strings based on configuration settings.
        /// Handles case sensitivity and culture-specific comparisons.
        /// </summary>
        /// <param name="value1">First string to compare</param>
        /// <param name="value2">Second string to compare</param>
        /// <returns>True if strings match according to configuration</returns>
        bool CheckEqual(string value1, string value2)
        {
            return UConfig.IdentifierAlwaysEnglishAndIgnoresCasing ? value1.Equals(value2, StringComparison.OrdinalIgnoreCase) : 
                value1.Equals(value2);
        }
        
        /// <summary>
        /// Removes a specific data entry from all partitions.
        /// Searches through all partitions and removes entries with matching keys.
        /// </summary>
        /// <param name="key">Key of the data entry to remove</param>
        internal void Clear(string key)
        {
            if (key.IsStringValid() && masterSave != null && masterSave.partitions != null && masterSave.partitions.Length > 0)
            {
                // Iterate through all partitions
                for (int i = 0; i < masterSave.partitions.Length; i++)
                {
                    var data = masterSave.partitions[i];
                    if (data == null || data.entries == null || data.entries.Length == 0) { continue; }
                    
                    // Filter out entries with matching keys
                    var entryResult = new List<DataEntry>();
                    for (int j = 0; j < data.entries.Length; j++)
                    {
                        var entry = data.entries[j];
                        if (entry == null || !entry.key.IsStringValid()) { continue; }
                        
                        // Keep entries that don't match the key to clear
                        if (!CheckEqual(entry.key, key))
                        {
                            entryResult.Add(entry);
                        }
                    }
                    
                    // Update partition with filtered entries
                    masterSave.partitions[i].entries = entryResult.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Clears all save data by resetting to default empty state.
        /// </summary>
        internal void ClearAll()
        {
            masterSave = PlayerStorageSaveUtil.GetDefaultMasterSave<MasterSaveJsonDefault>();
            masterSave.partitions = null;
        }
        
        /// <summary>
        /// Finds existing partition for a key or creates a new one.
        /// Manages partition assignment and ensures optimal data distribution.
        /// </summary>
        /// <param name="key">Data key to find or create partition for</param>
        /// <param name="defaultValue">Default value for new entries</param>
        /// <returns>Partition number where the key is stored</returns>
        internal int GetOrCreatePartitionNumberOnPlayerStorageHandleCreation(string key, string defaultValue)
        {
            int result = -1;
            
            // Initialize master save if needed
            if (masterSave == null)
            {
                masterSave = PlayerStorageSaveUtil.GetDefaultMasterSave<MasterSaveJsonDefault>();
                masterSave.partitions = null;
            }

            // Create first partition if none exist
            if (masterSave.partitions == null || masterSave.partitions.Length == 0)
            {
                masterSave.partitions = new DataEntryPartition[1];
                var newPartition = new DataEntryPartition();
                {
                    newPartition.partition = 0;
                    newPartition.entries = new DataEntry[1];
                    newPartition.entries[0] = new DataEntry { key = key, value = defaultValue };
                }
                masterSave.partitions[0] = newPartition;
            }

            // Validate and clean up save file if configured
            if (UConfig.ValidateEntireSaveFileUponEveryCreation)
            {
                List<DataEntryPartition> parts = new List<DataEntryPartition>();
                for (int i = 0; i < masterSave.partitions.Length; i++)
                {
                    var p = masterSave.partitions[i];
                    if (p == null || p.partition < 0 || p.entries == null || p.entries.Length == 0) { continue; }
                    
                    // Clean up invalid entries
                    List<DataEntry> entries = new List<DataEntry>();
                    for (int j = 0; j < p.entries.Length; j++)
                    {
                        var entry = p.entries[j];
                        if (entry == null || !entry.key.IsStringValid()) { continue; }
                        entries.Add(entry);
                    }
                    
                    // Update partition with valid entries only
                    if (entries.Count > 0) { p.entries = entries.ToArray(); }
                    else { p.entries = null; }
                    parts.Add(p);
                }
                masterSave.partitions = parts.ToArray();
            }

            // Variables for partition management logic
            int elligiblePartitionIndex = -1;
            int elligiblePartitionNumber = -1;
            int currentPartitionNumLargest = -1;
            bool hasThereAnySpaceInExistingPartition = false;
            bool partitionFound = false;
            
            // Search for existing key and find available partitions
            for (int i = 0; i < masterSave.partitions.Length; i++)
            {
                if (partitionFound) { break; }
                var data = masterSave.partitions[i];
                if (data == null || data.entries == null || data.entries.Length == 0) { continue; }
                
                // Check if partition has space for new entries
                if (data.entries.Length < UConfig.SaveDataPartitionElementNumber)
                {
                    hasThereAnySpaceInExistingPartition = true;
                    elligiblePartitionNumber = data.partition;
                    elligiblePartitionIndex = i;
                }
                
                // Track highest partition number for creating new partitions
                if (currentPartitionNumLargest < data.partition) { currentPartitionNumLargest = data.partition; }

                // Search for existing key in this partition
                for (int j = 0; j < data.entries.Length; j++)
                {
                    var entry = data.entries[j];
                    if (entry == null || !entry.key.IsStringValid()) { continue; }
                    
                    if (CheckEqual(entry.key, key)) 
                    {
                        partitionFound = true;
                        result = data.partition;
                        break;
                    }
                }
            }

            // Handle case where key doesn't exist yet
            if (!partitionFound)
            {
                if (hasThereAnySpaceInExistingPartition)
                {
                    // Add to existing partition with available space
                    var entries = masterSave.partitions[elligiblePartitionIndex].entries;
                    var entriesTmp = new List<DataEntry>();
                    entriesTmp.AddRange(entries);
                    entriesTmp.Add(new DataEntry { key = key, value = defaultValue });
                    masterSave.partitions[elligiblePartitionIndex].entries = entriesTmp.ToArray();
                    result = elligiblePartitionNumber;
                }
                else
                {
                    // Create new partition for this entry
                    var parts = new List<DataEntryPartition>();
                    parts.AddRange(masterSave.partitions);
                    var newPartition = new DataEntryPartition();
                    {
                        newPartition.partition = currentPartitionNumLargest + 1;
                        newPartition.entries = new DataEntry[1];
                        newPartition.entries[0] = new DataEntry { key = key, value = defaultValue };
                    }
                    parts.Add(newPartition);
                    masterSave.partitions = parts.ToArray();
                    result = newPartition.partition;
                }
            }
            return result;
        }
        
        /// <summary>
        /// Core method for processing load/save operations on partitioned data.
        /// Handles finding, creating, and updating data entries within partitions.
        /// </summary>
        /// <param name="desc">Process descriptor with operation details</param>
        /// <param name="selEntry">Output: Found or created data entry</param>
        /// <param name="dataType">Type information for validation</param>
        /// <returns>True if partition structure was modified</returns>
        void ProcessPartitions(ProcessDesc desc, ref DataEntry selEntry, System.Type dataType)
        {
            // Extract process parameters
            int partition = desc.partition;
            string key = desc.key;
            string defaultValue = desc.defaultValue;
            bool shouldSave = desc.shouldSave;
            string saveData = desc.saveData;

            selEntry = null;
            
            // Validate input in development builds
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (!desc.IsProcessDescValid(dataType))
            {
                GLog.Throw("UnityExt PlayerStorage Serious Issue! The input desc data is invalid!!!!");
                return;
            }
#endif
            
            // Find target partition and separate others
            DataEntryPartition selPartition = null;
            List<DataEntryPartition> otherPartitions = new List<DataEntryPartition>();
            
            if (masterSave != null && masterSave.partitions != null && masterSave.partitions.Length > 0)
            {
                for (int i = 0; i < masterSave.partitions.Length; i++)
                {
                    var data = masterSave.partitions[i];
                    if (data == null) { continue; }
                    
                    if (data.partition == partition)
                    {
                        selPartition = data;
                    }
                    else
                    {
                        otherPartitions.Add(data);
                    }
                }

                // Search for existing entry in target partition
                if (selPartition != null && selPartition.entries != null && selPartition.entries.Length > 0)
                {
                    for (int i = 0; i < selPartition.entries.Length; i++)
                    {
                        var data = selPartition.entries[i];
                        if (data == null || !CheckEqual(data.key, key)) { continue; }
                        
                        selEntry = data;
                        
                        // Update value if this is a save operation
                        if (shouldSave)
                        {
                            selPartition.entries[i].value = saveData;
                        }
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Loads a data entry value by key from specified partition.
        /// Returns default value if entry doesn't exist.
        /// </summary>
        /// <param name="key">Unique identifier for the data</param>
        /// <param name="defaultValue">Value to return if key not found</param>
        /// <param name="partition">Partition number to search in</param>
        /// <param name="dataType">Type information for validation</param>
        /// <returns>Stored value or default value</returns>
        internal string Load(string key, string defaultValue, int partition, System.Type dataType)
        {
            DataEntry selEntry = null;
            var desc = new ProcessDesc
            {
                defaultValue = defaultValue,
                key = key,
                partition = partition,
                saveData = "",
                shouldSave = false // This is a load operation
            };
            ProcessPartitions(desc, ref selEntry, dataType);
            return selEntry.value;
        }
        
        /// <summary>
        /// Saves a data entry value by key to specified partition.
        /// Creates entry if it doesn't exist, updates if it does.
        /// </summary>
        /// <param name="key">Unique identifier for the data</param>
        /// <param name="data">Value to save</param>
        /// <param name="defaultValue">Default value for new entries</param>
        /// <param name="partition">Partition number to save in</param>
        /// <param name="dataType">Type information for validation</param>
        internal void Save(string key, string data, string defaultValue, int partition, System.Type dataType)
        {
            DataEntry entry = null;
            var desc = new ProcessDesc
            {
                defaultValue = defaultValue,
                key = key,
                partition = partition,
                saveData = data,
                shouldSave = true // This is a save operation
            };
            ProcessPartitions(desc, ref entry, dataType);
        }
    }
}