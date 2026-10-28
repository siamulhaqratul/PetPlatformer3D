using UnityEngine;

namespace UnityExt.TaskManagement
{
    /// <summary>Persistent task runner used for coroutines that must survive scene transitions.</summary>
    internal sealed class PersistentTaskRunner : MonoBehaviour
    {
        // ======================= UNITY CALLBACKS =========================

        private void Awake()
        {
            // Ensure this runner (and its GameObject) persists across scene loads.
            if (transform.parent == null) // Prevent duplicating DontDestroyOnLoad for nested objects.
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}