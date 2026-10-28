using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityExt.TaskManagement;

namespace UnityExt
{
    /// <summary>
    /// Miscellaneous extension methods for RaycastHit, string, coroutine, UI, and material utilities.
    /// </summary>
    public static class OtherEx
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Resets the first <paramref name="len"/> elements of a RaycastHit array to default.
        /// </summary>
        /// <param name="hits">The RaycastHit array to reset.</param>
        /// <param name="len">Number of elements to reset.</param>
        public static void ResetHits(this RaycastHit[] hits, int len)
        {
            for (int i = 0; i < len; i++)
            {
                hits[i] = default;
            }
        }

        /// <summary>
        /// Checks if any RaycastHit in the array (up to <paramref name="len"/>) contains a collider with a component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Component type to search for (MonoBehaviour).</typeparam>
        /// <param name="hits">The RaycastHit array.</param>
        /// <param name="len">Number of elements to check.</param>
        /// <param name="col">Outputs the found collider if successful; otherwise null.</param>
        /// <returns>True if a matching collider/component is found; otherwise false.</returns>
        internal static bool HasFoundTagType<T>(this RaycastHit[] hits, int len, out Collider col) where T : MonoBehaviour
        {
            bool result = false;
            col = null;
            for (int i = 0; i < len; i++)
            {
                var hit = hits[i];
                if (ReferenceEquals(hit.collider, null)) { continue; }
                var scr = hit.collider.GetComponent<T>();
                if (ReferenceEquals(scr, null)) { continue; }
                col = hit.collider;
                result = true;
                break;
            }
            return result;
        }

        /// <summary>
        /// Checks if any RaycastHit in the array (up to <paramref name="len"/>) contains the specified collider and a component of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Component type to search for (MonoBehaviour).</typeparam>
        /// <param name="hits">The RaycastHit array.</param>
        /// <param name="len">Number of elements to check.</param>
        /// <param name="collider">The collider to match.</param>
        /// <returns>True if a matching collider/component is found; otherwise false.</returns>
        internal static bool HasFoundTagTypeAndCollider<T>(this RaycastHit[] hits, int len, Collider collider) where T : MonoBehaviour
        {
            bool result = false;
            for (int i = 0; i < len; i++)
            {
                var hit = hits[i];
                if (ReferenceEquals(hit.collider, null)) { continue; }
                if (!ReferenceEquals(hit.collider, collider)) { continue; }
                var scr = hit.collider.GetComponent<T>();
                if (ReferenceEquals(scr, null)) { continue; }
                result = true;
                break;
            }
            return result;
        }

        /// <summary>
        /// Returns true if the string is not null, empty, or whitespace.
        /// </summary>
        /// <param name="str">The string to validate.</param>
        /// <returns>True if valid; otherwise false.</returns>
        internal static bool IsStringValid(this string str)
        {
            return !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Attempts to kill all running TaskHandles in the list.
        /// </summary>
        /// <param name="handles">List of TaskHandles to kill.</param>
        /// <param name="killer">Optional MonoBehaviour to use for context.</param>
        /// <param name="optionalReason">Optional reason for killing tasks.</param>
        public static void KillRunningTasks(this List<TaskHandle> handles, MonoBehaviour killer = null, string optionalReason = "")
        {
            handles.ExForEachSafeCustomClass((h) =>
            {
                h.TryKill();
            });
        }

        /// <summary>
        /// Stops all running coroutines in the list using the provided MonoBehaviour.
        /// </summary>
        /// <param name="handles">List of Coroutine handles.</param>
        /// <param name="killer">MonoBehaviour to call StopCoroutine on.</param>
        public static void KillRunningTasks(this List<Coroutine> handles, MonoBehaviour killer)
        {
            handles.ExForEachSafeCustomClass((h) =>
            {
                killer.StopCoroutine(h);
            });
        }

        /// <summary>
        /// Returns the name of a Unity Object, or "NULL" if the object is null.
        /// </summary>
        /// <param name="obj">The Unity Object.</param>
        /// <returns>The object's name or "NULL".</returns>
        public static string ExGetObjectName(this Object obj)
        {
            return obj == null ? "NULL" : obj.name;
        }

        /// <summary>
        /// Sets a Rigidbody to Unity's default values.
        /// </summary>
        /// <param name="rigidbody">The Rigidbody to reset.</param>
        public static void ExSetDefault(this Rigidbody rigidbody)
        {
            rigidbody.mass = 1.0f;
            rigidbody.drag = 0.0f;
            rigidbody.angularDrag = 0.05f;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;
            rigidbody.velocity = Vector3.zero;
        }

        /// <summary>
        /// Casts a ray down from above the target position and returns the hit point (offset), or the original position if no hit.
        /// </summary>
        /// <param name="targetPosition">The position to check from.</param>
        /// <param name="mask">LayerMask to use for the raycast.</param>
        /// <returns>The platform position or original if not found.</returns>
        internal static Vector3 ExGetPositionInPlatform(this Vector3 targetPosition, LayerMask mask)
        {
            RaycastHit hit;
            var success = Physics.Raycast(targetPosition + Vector3.up * 20f, Vector3.down, out hit, Mathf.Infinity,
                mask, QueryTriggerInteraction.Ignore);
            if (!success) { return targetPosition; }
            return hit.point + Vector3.down * 0.12999f;
        }

        /// <summary>
        /// Extracts all unique implementations of <typeparamref name="T"/> from the children of the provided GameObject roots.
        /// </summary>
        /// <typeparam name="T">Component type to extract.</typeparam>
        /// <param name="depRoots">List of GameObject roots to search.</param>
        /// <param name="dependencies">List to populate with found implementations.</param>
        internal static void ExtractImplementationsFrom<T>(this List<GameObject> depRoots, in List<T> dependencies) where T : class
        {
            dependencies.Clear();
            if (depRoots != null && depRoots.Count > 0)
            {
                for (int i = 0; i < depRoots.Count; i++)
                {
                    var root = depRoots[i];
                    if (root == null) { continue; }
                    var handlers = root.GetComponentsInChildren<T>(true);
                    if (handlers != null && handlers.Length > 0)
                    {
                        for (int j = 0; j < handlers.Length; j++)
                        {
                            var hnd = handlers[j];
                            if (hnd == null) { continue; }
                            if (!dependencies.Contains(hnd))
                            {
                                dependencies.Add(hnd);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the first implementation of <typeparamref name="T"/> from the children of the provided GameObject roots.
        /// </summary>
        /// <typeparam name="T">Component type to extract.</typeparam>
        /// <param name="depRoots">List of GameObject roots to search.</param>
        /// <param name="dependency">Reference to set to the found implementation.</param>
        internal static void ExtractImplementationsFrom<T>(this List<GameObject> depRoots, ref T dependency) where T : class
        {
            if (depRoots != null && depRoots.Count > 0)
            {
                for (int i = 0; i < depRoots.Count; i++)
                {
                    var root = depRoots[i];
                    if (root == null) { continue; }
                    var handle = root.GetComponentInChildren<T>(true);
                    if (handle != null)
                    {
                        dependency = handle;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Stops and nulls a single coroutine handle.
        /// </summary>
        /// <param name="runner">MonoBehaviour to call StopCoroutine on.</param>
        /// <param name="handle">Reference to the Coroutine to stop and null.</param>
        public static void ExFlushCoroutine(this MonoBehaviour runner, ref Coroutine handle)
        {
            if (handle != null) { runner.StopCoroutine(handle); }
            handle = null;
        }

        /// <summary>
        /// Stops and nulls all coroutine handles in a list.
        /// </summary>
        /// <param name="runner">MonoBehaviour to call StopCoroutine on.</param>
        /// <param name="handles">Reference to the list of Coroutine handles.</param>
        public static void ExFlushCoroutine(this MonoBehaviour runner, ref List<Coroutine> handles)
        {
            if (handles != null && handles.Count > 0)
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    var handle = handles[i];
                    if (handle != null) { runner.StopCoroutine(handle); }
                    handles[i] = null;
                }
            }
        }

        /// <summary>
        /// Binds a button's onClick event to the provided logic, removing all previous listeners.
        /// </summary>
        /// <param name="button">The Button to bind.</param>
        /// <param name="logic">Delegate to invoke on click.</param>
        public static void ExFreshBind(this Button button, OnDoAnything logic)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { logic?.Invoke(); });
        }

        /// <summary>
        /// Sets the shared material for all MeshRenderer and SkinnedMeshRenderer components in the children of the Transform.
        /// </summary>
        /// <param name="tr">The root Transform to search from.</param>
        /// <param name="material">The Material to assign.</param>
        public static void ExSetSharedMaterial(this Transform tr, Material material)
        {
            var meshRNs = tr.GetComponentsInChildren<MeshRenderer>(true);
            meshRNs.ExForEachSafe((m) =>
            {
                m.sharedMaterial = material;
            });
            var skinnedMeshRNs = tr.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            skinnedMeshRNs.ExForEachSafe((m) =>
            {
                m.sharedMaterial = material;
            });
        }
    }
}