using System.Collections;
using UnityEngine;
using UnityExt.Debugging;
using UnityExt.Tons;

namespace UnityExt.TaskManagement
{
    /// <summary>Central manager for launching and controlling coroutine-based tasks with configurable lifecycle behavior.</summary>
    public class TaskManager : PersistentSingletonBehaviourProtected<TaskManager>
    {
        // =================== PRIVATE/PROTECTED FIELDS ====================

        private TaskRunner _runner;
        private PersistentTaskRunner _persistRunner;

        // =========================== PROPERTIES ==========================

        /// <summary>Returns true if the singleton instance is currently valid.</summary>
        internal static bool IsInstanceOk => Instance != null;

        // ========================= STATIC METHODS ========================

        /// <summary>Stops all non-persistent tasks when a scene load occurs.</summary>
        /// <param name="appExit">Indicates whether this stop is triggered by an application exit (currently unused).</param>
        internal static void KillRunningTasksOnSceneLoad(bool appExit = false)
        {
            if (Instance == null) return;
            var normalRunner = Instance.GetComponent<TaskRunner>();
            if (normalRunner == null) return;
            normalRunner.StopAllCoroutines();
        }

        /// <summary>Starts a coroutine task with the specified run mode.</summary>
        /// <param name="work">Coroutine enumerator to execute.</param>
        /// <param name="mode">Determines whether the task survives scene loads.</param>
        /// <returns>A <see cref="TaskHandle"/> that can be used to cancel the task, or null if launch failed.</returns>
        public static TaskHandle StartTask(IEnumerator work, TaskRunMode mode = TaskRunMode.Default)
        {
            if (work == null) return null;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GLog.Throw("Task Manager can only be used in playmode/runtime! Use 'EditorCoroutine' for editor scripts.");
            }
#endif
            var manager = EnsureInstance();
            manager.EnsureRunners();

            TaskHandle result;
            if (mode == TaskRunMode.RunEvenWhenSceneIsLoading)
            {
                var handle = manager._persistRunner.StartCoroutine(work);
                result = new TaskHandle(ref handle, manager._persistRunner);
            }
            else
            {
                var handle = manager._runner.StartCoroutine(work);
                result = new TaskHandle(ref handle, manager._runner);
            }
            return result;
        }

        public static void DelayAction(float seconds, System.Action action)
        {
            var enumerator = DelayActionEnumerator(seconds, action);
            StartTask(enumerator);

            IEnumerator DelayActionEnumerator(float secs, System.Action act)
            {
                yield return new WaitForSeconds(secs);
                act?.Invoke();
            }
        }



        // ========================= PRIVATE METHODS =======================

        /// <summary>Ensures the singleton instance exists, creating one if necessary.</summary>
        /// <returns>The valid <see cref="TaskManager"/> instance.</returns>
        private static TaskManager EnsureInstance()
        {
            var inst = Instance;
            if (inst != null) return inst;
            var gobj = new GameObject("__TASK_MANAGER_GEN_");
            return gobj.AddComponent<TaskManager>();
        }



        /// <summary>Ensures runner components are present and cached.</summary>
        private void EnsureRunners()
        {
            if (_runner == null)
            {
                _runner = GetComponent<TaskRunner>() ?? gameObject.AddComponent<TaskRunner>();
            }
            if (_persistRunner == null)
            {
                _persistRunner = GetComponent<PersistentTaskRunner>() ?? gameObject.AddComponent<PersistentTaskRunner>();
            }
        }
    }
}