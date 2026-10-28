using UnityEngine;

namespace UnityExt.TaskManagement
{
    /// <summary>Runtime task runner used for coroutines that should be cleaned up on scene load.</summary>
    internal sealed class TaskRunner : MonoBehaviour
    {
        // (Intentionally empty) Acts as a tag component to host scene-bound coroutines.
    }
}