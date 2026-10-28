using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Static class containing string constants for UI labels, menu items, and categorization.
    /// Used throughout the UnityExt framework for consistent naming conventions.
    /// </summary>
    public static class ULabel
    {
        // =========================== CONSTANTS ===========================
        // Core framework identification
        public const string LibName = "UnityExt";

        // Feature category labels for editor organization
        public const string Debug = "Debug";
        public const string Gizmo = "Gizmo";
        public const string Basic = "Basic";
        public const string Test = "Test";
        public const string Advanced = "Advanced";
        public const string Dependency = "Dependency";

        // Unity Editor menu structure constants
        public const string CreateAssetLabel = "Assets/Create/Kaiyum/UnityExt/";
        public const string DoOperationLabel = "Assets/UnityExt/";
        public const string CreateSO = "Kaiyum/UnityExt/";
        public const string AuthorName = "Kaiyum";
        public const string MenuItemExt = "/UnityExt";

        // Specialized system labels
        public const string UGameplayFramework = "UGameplayFramework";
        public const string UI = "UI";
        public const string Story = "Story";
        public const string Rendering = "Rendering";
        public const string FTUE = "FTUE"; // First Time User Experience
        public const string Emoji = "Emoji";
        public const string Animation = "Animation";
        public const string Sound = "Sound";
        public const string Deploy = "Deploy";
        public const string Layer2_ArcadeIdle = "Layer2/ArcadeIdle/";
    }

    /// <summary>
    /// Optimization methods for unique identifier generation in save data.
    /// Each method has different collision risks and performance characteristics.
    /// </summary>
    public enum UIDOptimizationMethod
    {
        SHA256 = 0, // Cryptographically secure, lowest collision risk, larger hash size
        MD5 = 1,    // Fast hashing, moderate collision risk, smaller hash size
        Custom = 2, // User-defined method, unknown collision risk, not recommended
        None = 3    // No optimization, uses original identifiers, largest file size
    }

    /// <summary>
    /// Available backends for save data storage.
    /// Each has different limitations and use cases.
    /// </summary>
    public enum SaveDataWriteBackend
    {
        PlayerPref = 0, // Unity's PlayerPrefs system, limited to ~1MB, platform-dependent storage
        Default = 1     // File-based storage, unlimited size, custom encryption/compression support
    }

    /// <summary>
    /// Text encoding types supported for save data encryption and general text processing.
    /// Different encodings have varying character support and file sizes.
    /// </summary>
    public enum GameTextEncodingType
    {
        ASCII = 0,      // 7-bit encoding, English characters only, smallest size
        UTF_7 = 1,      // Variable-length encoding, limited Unicode support
        UTF_8 = 2,      // Variable-length encoding, full Unicode support, most common
        UTF_32 = 3,     // Fixed-length encoding, full Unicode support, largest size
        UNICODE = 4,    // UTF-16 encoding, good Unicode support, moderate size
        BIG_ENDIAN = 5  // UTF-16 Big Endian, byte order consideration for cross-platform
    }

    /// <summary>
    /// Central configuration class for the UnityExt framework.
    /// Contains compile-time constants that control system behavior.
    /// WARNING: Many save data settings should NOT be changed after game release!
    /// </summary>
    public static class UConfig
    {
        // =========================== CONSTANTS ===========================
        #region Script Execution Order Constants
        // Unity script execution order values (range: -32768 to 32767)
        // Lower values execute earlier in the frame

        /// <summary>Core application systems execute first</summary>
        internal const int AppExecutionOrder = -31900;

        /// <summary>UI Canvas systems execute after core app</summary>
        internal const int AGameCanvasExecutionOrder = -31800;
        /// <summary>UI components execute after canvas setup</summary>
        internal const int UIComponentExecutionOrder = -31700;

        /// <summary>World/scene management systems</summary>
        internal const int AWorldExecutionOrder = -31600;
        /// <summary>Game actors/entities execute after world setup</summary>
        internal const int AnActorExecutionOrder = -31500;
        /// <summary>Actor components execute last in the chain</summary>
        internal const int AComponentExecutionOrder = -31400;
        #endregion

        #region Save Data Configuration
        // CRITICAL: These settings must NOT be changed after game release!
        // Changing them will cause existing player save data to be lost or corrupted!

        /// <summary>
        /// File name for save data stored on device.
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// </summary>
        internal const string PlayerStorageFileName = "PlayerData.Kai";

        /// <summary>
        /// Internal key for PlayerPrefs backend storage.
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// </summary>
        internal const string MasterDataKeyForPlayerPref = "_PLAYER_SAVE_DATA_AS_JSON_STRING_";

        /// <summary>
        /// Storage backend selection: PlayerPrefs vs File system.
        /// PlayerPrefs limited to ~1MB, File system supports larger data.
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// </summary>
        internal const SaveDataWriteBackend SaveDataBackend = SaveDataWriteBackend.Default;

        /// <summary>
        /// Enable data compression to reduce save file size.
        /// Only works with Default (file-based) backend, not PlayerPrefs.
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// </summary>
        internal const bool UseDataCompression = true;

        /// <summary>
        /// Optimization for English-only identifiers (case insensitive).
        /// Improves performance for key comparisons in save system.
        /// CRITICAL: Do NOT change after release - may corrupt save data!
        /// </summary>
        internal const bool IdentifierAlwaysEnglishAndIgnoresCasing = true;

        /// <summary>
        /// Maximum entries per partition in save data structure.
        /// Power-of-two values recommended for memory alignment.
        /// CRITICAL: Do NOT change after release - may corrupt save data!
        /// </summary>
        internal const int SaveDataPartitionElementNumber = 512;

        /// <summary>
        /// Identifier optimization method for save data keys.
        /// Reduces file size but introduces collision risks.
        /// SHA256: Safest, MD5: Moderate risk, Custom: High risk, None: No optimization
        /// </summary>
        internal const UIDOptimizationMethod UIDOptimization = UIDOptimizationMethod.None;

        /// <summary>
        /// Enable AES encryption for save data security.
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// Note: Encryption reduces compression effectiveness.
        /// </summary>
        internal const bool useEncryption = false;

        /// <summary>
        /// Text encoding for encrypted save data.
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// </summary>
        internal const GameTextEncodingType Encoding = GameTextEncodingType.UTF_32;

        /// <summary>
        /// Encryption shared secret key (only used if encryption enabled).
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// </summary>
        internal const string encryptionSharedSecretForSaveData = "o7x8y6";

        /// <summary>
        /// Encryption salt value (only used if encryption enabled).
        /// CRITICAL: Do NOT change after release - will lose all player data!
        /// </summary>
        internal const string encryptionSaltForSaveData = "o6806642kbM7c5";

        /// <summary>
        /// Validate save data integrity during handle creation.
        /// Ensures no null/invalid entries but may impact performance.
        /// </summary>
        internal const bool ValidateEntireSaveFileUponEveryCreation = false;

        /// <summary>
        /// GameObject name for save system management.
        /// If renamed, update corresponding WebGL plugin "RefreshDetection".
        /// </summary>
        internal const string PlayerStorageGameObjectName = "__STORAGE__WRITER__";

        /// <summary>
        /// Auto-save on scene transitions.
        /// May cause frame rate hitches during scene loading.
        /// </summary>
        internal const bool WriteSaveOnSceneLoad = true;

        /// <summary>
        /// Auto-save on any data change.
        /// Recommended OFF for arcade/idle games with frequent data updates.
        /// </summary>
        internal const bool WriteSaveWhenEverDataChanged = false;

        /// <summary>
        /// Pretty-print JSON save files for debugging.
        /// Increases file size - use only for development debugging.
        /// </summary>
        internal const bool UsePrettyPrintForSaveDataJson = false;

        /// <summary>Background threading for save operations to prevent main thread blocking</summary>
        internal const bool UseThreadingWhenDataSaving = true;

        /// <summary>Periodic auto-save system</summary>
        internal const bool UseTimerBasedDataSaving = true;

        /// <summary>Auto-save interval in seconds</summary>
        internal const float TimerIntervalForDataSaving = 0.5f;
        #endregion

        #region Gameplay Framework Configuration

        /// <summary>
        /// Enable custom tick system for World/Actor/Component updates.
        /// Alternative to Unity's built-in Update() system for better control.
        /// </summary>
        internal const bool UseCustomTickSystemForGameplayFramework = true;

        /// <summary>
        /// Cooldown time for TriggerVolumeComponent to prevent spam triggering.
        /// Time in seconds between trigger activations.
        /// </summary>
        internal const float VolumeCooldownTime = 0.2f;

        /// <summary>
        /// Enable parameter system for Actors/Components with save data support.
        /// Allows runtime configuration that persists between sessions.
        /// </summary>
        internal const bool UseSaveableParameterSystem = false;
        #endregion

        #region General Framework Settings

        /// <summary>
        /// Safe mode for custom tick system - skips missing script references.
        /// Recommended OFF for performance, ON for development safety.
        /// Only relevant when using custom tick system.
        /// </summary>
        internal const bool EnableSafeModeForTickSystem = true;

        /// <summary>UI panel animation offset time for smooth transitions</summary>
        internal const float PanelOpenCloseOffset = 0.8f;

        /// <summary>Default duration for UI tween animations</summary>
        public const float DefaultTweenTime = 0.4f;

        /// <summary>Encryption key for non-save-data operations (general purpose)</summary>
        internal const string encryptionKEYForOtherEncryption = "";

        /// <summary>Initialization vector for non-save-data encryption (general purpose)</summary>
        internal const string encryptionIVForOtherEncryption = "";
        #endregion
    }
}