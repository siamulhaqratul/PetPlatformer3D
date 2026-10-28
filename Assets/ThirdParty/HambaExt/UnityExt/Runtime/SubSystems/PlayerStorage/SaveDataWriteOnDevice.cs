
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityExt.TaskManagement;
using UnityExt.Tons;

namespace UnityExt.DataSaving
{
    /// <summary>
    /// Core save data writer component that handles automatic persistence to disk.
    /// Singleton MonoBehaviour that listens for various application events and triggers saves.
    /// Provides WebGL refresh detection, timer-based auto-saves, and error handling saves.
    /// </summary>
    public sealed class SaveDataWriteOnDevice : PersistentSingletonBehaviour<SaveDataWriteOnDevice>
    {
        #region Events and Debug Data
        /// <summary>Event fired when any save operation occurs (for refresh callbacks)</summary>
        UnityEvent onWrite = null;
        
        /// <summary>Event fired just before application quit saves (for cleanup logic)</summary>
        UnityEvent onJustBeforeQuitSave = null;
        
        /// <summary>Debug view of current save data structure for inspector visibility</summary>
        [SerializeField] MasterSaveJsonDefault debugSave = null;
        
        /// <summary>Updates debug data display in inspector for development monitoring</summary>
        internal void SetDebugData(MasterSaveJsonDefault debugData) { this.debugSave = debugData; }
        #endregion

        #region Task Management
        /// <summary>Task handle for scene load save operations to prevent overlapping saves</summary>
        TaskHandle handle = null;
        
        /// <summary>Timestamp of last save triggered by scene loading (for debugging timing issues)</summary>
        static double lastSceneLoadSaveTime = 0.0f;
        internal static double LastSceneLoadSaveTime { get { return lastSceneLoadSaveTime; } }
        #endregion

        #region Write Operation Tracking
        /// <summary>
        /// Counter for pending write operations (-1 = no pending writes, 0+ = operations count).
        /// Used by timer-based saving to determine if a save is needed.
        /// </summary>
        static int totalWriteCount = -1;
        
        /// <summary>Increments write counter when any PlayerStorage value changes</summary>
        internal static void OnAnyWriteOperation() { totalWriteCount++; }
        
        /// <summary>Resets write counter after successful save operation</summary>
        internal static void FlushWriteOperation() { totalWriteCount = -1; }
        
        /// <summary>
        /// Starts background timer-based auto-save system.
        /// Periodically checks for pending writes and saves if necessary.
        /// Runs continuously even during scene loading for data safety.
        /// </summary>
        static bool _isTimerOperationRunning = false;
        internal static void StartTimerBasedWriteOperation()
        {
            if (_isTimerOperationRunning) return;
            if (!Application.isPlaying) return;

            _isTimerOperationRunning = true;
            // Use TaskManager for persistent coroutine that survives scene changes
            TaskManager.StartTask(COR(), TaskRunMode.RunEvenWhenSceneIsLoading);
            IEnumerator COR()
            {
                var timer = 0.0f;
                while (true)
                {
                    timer += Time.deltaTime;
                    // Check if enough time has passed since last timer check
                    if (timer > UConfig.TimerIntervalForDataSaving)
                    {
                        timer = 0.0f;
                        // Only save if there are pending write operations
                        if (totalWriteCount >= 0)
                        {
                            PlayerStorageManager.SaveData?.WriteMasterSaveToDevice();
                        }
                    }
                    yield return null;
                }
            }
        }
        #endregion

        #region WebGL Refresh Detection
#if UNITY_WEBGL
        /// <summary>
        /// WebGL JavaScript interop to register for browser beforeunload event.
        /// Detects when user refreshes page or closes tab to trigger emergency save.
        /// </summary>
        [DllImport("__Internal")]
        private static extern void RegisterBeforeUnloadWebGL();
#endif

        void Start()
        {
#if UNITY_WEBGL
            // Register for WebGL page refresh/close detection
            // Only works in actual WebGL builds, not in editor
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                RegisterBeforeUnloadWebGL();
            }
#endif
        }

#if UNITY_WEBGL
        /// <summary>
        /// Called by WebGL JavaScript when page is about to be refreshed or closed.
        /// Provides last chance to save data before user loses session.
        /// Method name must match JavaScript callback registration.
        /// </summary>
        public void OnRefresh()
        {
            SaveDataWriteOnDevice.WriteToDevice();
        }
#endif
        #endregion

        #region Unity Event Handlers
        protected override void OnEnable()
        {
            base.OnEnable();
            // Subscribe to critical application events for automatic saving
            Application.logMessageReceived += HandleLog;       // Save on errors/exceptions
            Application.quitting += Application_quitting;      // Save before app closes
            Application.focusChanged += Application_focusChanged; // Save when focus lost
        }

        /// <summary>
        /// Handles scene loading events to trigger automatic saves.
        /// Prevents data loss during scene transitions.
        /// </summary>
        protected override void OnLoadUnityScene(Scene scene, LoadSceneMode mode)
        {
            base.OnLoadUnityScene(scene, mode);
            
            // Skip saving if disabled in configuration
            if (!UConfig.WriteSaveOnSceneLoad) { return; }
            
            // Skip saving for boot/initialization scenes to avoid startup overhead
            if (scene.buildIndex == 0 || scene.name == "Boot" || scene.name == "boot") { return; }

            // Cancel any existing save task to prevent overlapping operations
            if (handle != null) { handle.TryKill(); }
            
            // Start new save task that survives scene loading
            handle = TaskManager.StartTask(COR(), TaskRunMode.RunEvenWhenSceneIsLoading);
            IEnumerator COR()
            {
                // Wait one frame for scene to fully load before saving
                yield return null;
                PlayerStorageManager.SaveData?.WriteMasterSaveToDevice();
                lastSceneLoadSaveTime = Time.unscaledTimeAsDouble;
            }
        }

        /// <summary>
        /// Handles application focus change events.
        /// Saves data when app loses focus (user switches to another app).
        /// </summary>
        void Application_focusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveData();
            }
        }

        /// <summary>
        /// Handles application quit events.
        /// Ensures data is saved before application terminates.
        /// </summary>
        void Application_quitting()
        {
            SaveData(true); // Mark as app exit for special handling
        }

        private bool _isHandlingLog = false;

        /// <summary>
        /// Handles Unity log messages to trigger emergency saves.
        /// Automatically saves data when errors or exceptions occur to prevent data loss.
        /// </summary>
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (_isHandlingLog) return;

            if (type == LogType.Error || type == LogType.Exception)
            {
                _isHandlingLog = true;
                try
                {
                    // For exceptions, immediately write to disk (higher urgency)
                    if (type == LogType.Exception)
                    {
                        PlayerStorageManager.SaveData?.WriteMasterSaveToDevice();
                    }
                    else
                    {
                        // For errors, use normal save process
                        SaveData();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveDataWriteOnDevice] Failed to emergency save: {ex.Message}");
                }
                finally
                {
                    _isHandlingLog = false;
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Unsubscribe from events to prevent memory leaks
            Application.logMessageReceived -= HandleLog;
            Application.quitting -= Application_quitting;
            Application.focusChanged -= Application_focusChanged;
            // Final save attempt when component is destroyed
            //SaveData();
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Adds custom logic to be executed during save operations.
        /// Used by PlayerStorage instances to refresh their data before saves.
        /// Useful for ensuring all current values are properly serialized.
        /// </summary>
        internal void AddLogicOnRefresh(OnDoAnything logic)
        {
            if (onWrite == null) { onWrite = new UnityEvent(); }
            onWrite.AddListener(() => { logic?.Invoke(); });
        }

        /// <summary>
        /// Adds custom logic to be executed just before application quit saves.
        /// Provides opportunity for cleanup operations before final data persistence.
        /// Static method ensures it works even if no instance exists yet.
        /// </summary>
        public static void AddLogicOnJustBeforeSaveDuringAppQuit(OnDoAnything logic)
        {
            // Ensure instance exists for quit save registration
            if (SingletonBase<SaveDataWriteOnDevice>.Instance == null)
            {
                var g = new GameObject(UConfig.PlayerStorageGameObjectName);
                g.AddComponent<SaveDataWriteOnDevice>();
            }
            if (SingletonBase<SaveDataWriteOnDevice>.Instance.onJustBeforeQuitSave == null) { SingletonBase<SaveDataWriteOnDevice>.Instance.onJustBeforeQuitSave = new UnityEvent(); }
            SingletonBase<SaveDataWriteOnDevice>.Instance.onJustBeforeQuitSave.AddListener(() => { logic?.Invoke(); });
        }

        private float _lastTimeDataSaved;
        /// <summary>
        /// Core save operation that handles all persistence logic.
        /// Fires appropriate events and delegates to storage manager for actual I/O.
        /// </summary>
        /// <param name="isAppExit">True if this save is triggered by application exit</param>
        void SaveData(bool isAppExit = false)
        {
            // Fire pre-quit events for cleanup if this is an app exit save
            if (isAppExit)
            {
                if (onJustBeforeQuitSave == null) { onJustBeforeQuitSave = new UnityEvent(); }
                try
                {
                    onJustBeforeQuitSave?.Invoke();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveDataWriteOnDevice] Error in onJustBeforeQuitSave: {ex.Message}");
                }
            }
            
            // Fire refresh events to update all storage values
            if (onWrite == null) { onWrite = new UnityEvent(); }
            try
            {
                onWrite?.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveDataWriteOnDevice] Error in onWrite refresh: {ex.Message}");
            }
            
            // Delegate to storage manager for actual file I/O
            PlayerStorageManager.SaveData?.WriteMasterSaveToDevice();
        }

        /// <summary>
        /// Public static method to trigger save operation from anywhere.
        /// Provides simple interface for manual save requests.
        /// </summary>
        public static void WriteToDevice()
        {
            SingletonBase<SaveDataWriteOnDevice>.Instance.SaveData();
        }
        #endregion

        /*
         * AUTOMATIC SAVE TRIGGERS:
         * 1. Scene loading (after new scene loads)
         * 2. Application focus lost (user switches apps)
         * 3. Application quitting (before app closes)
         * 4. Errors/Exceptions (emergency saves)
         * 5. Component disable (cleanup saves)
         * 6. Timer-based (periodic auto-saves)
         * 7. WebGL refresh/close (browser events)
         * 
         * PERFORMANCE CONSIDERATIONS:
         * - Uses TaskManager for scene-persistent operations
         * - Prevents overlapping save operations
         * - Only timer-saves when there are pending writes
         * - Lazy event initialization to reduce memory overhead
         * 
         * PLATFORM-SPECIFIC FEATURES:
         * - WebGL: Browser refresh/close detection via JavaScript interop
         * - All platforms: Standard Unity application events
         * 
         * THREAD SAFETY:
         * - All operations run on main thread (Unity requirement)
         * - Storage manager handles threading internally for file I/O
         */
    }
}