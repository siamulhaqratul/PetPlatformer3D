using UnityEngine;

namespace UnityExt.TaskManagement
{
    /// <summary>Represents a running coroutine handle that can be safely cancelled.</summary>
    [System.Serializable]
    public class TaskHandle
    {
        // =================== PRIVATE/PROTECTED FIELDS ====================

        private Coroutine _handle;
        private MonoBehaviour _runner;

        // =========================== PROPERTIES ==========================

        /// <summary>Returns true if the underlying coroutine reference is still valid.</summary>
        public bool IsAlive => _handle != null && _runner != null;
        /// <summary>Runner component that started the coroutine (may become null after kill).</summary>
        public MonoBehaviour Runner => _runner;

        // ========================= INITIALIZE METHODS ========================

        /// <summary>Creates a new task handle wrapping a coroutine reference and its runner.</summary>
        /// <param name="handle">Reference to the started coroutine.</param>
        /// <param name="runner">MonoBehaviour that started the coroutine.</param>
        internal TaskHandle(ref Coroutine handle, MonoBehaviour runner)
        {
            _handle = handle;
            _runner = runner;
        }

        // ========================= PUBLIC METHODS ========================

        /// <summary>Attempts to stop the coroutine if still running and clear references.</summary>
        public void TryKill()
        {
            if (_runner == null || !TaskManager.IsInstanceOk || _handle == null) return;
            _runner.StopCoroutine(_handle);
            _handle = null;
            _runner = null;
        }
    }

    /// <summary>Defines how a scheduled task behaves during scene transitions.</summary>
    public enum TaskRunMode
    {
        /// <summary>Coroutine is cleaned up on scene load.</summary>
        Default = 0,
        /// <summary>Coroutine continues running even while a scene load is in progress.</summary>
        RunEvenWhenSceneIsLoading = 1
    }
}