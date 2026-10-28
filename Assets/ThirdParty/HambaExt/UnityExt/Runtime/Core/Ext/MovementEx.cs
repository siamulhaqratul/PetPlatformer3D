using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Extension methods for moving and rotating a Transform towards targets with speed control.
    /// </summary>
    public static class ExMovement
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Moves a Transform towards a target position at a specified speed, returning true if the target is reached.
        /// </summary>
        /// <param name="transform">The Transform to move.</param>
        /// <param name="targetPosition">The target position to move towards.</param>
        /// <param name="speed">The speed at which to move.</param>
        /// <returns>True if the Transform has reached the target position; otherwise, false.</returns>
        public static bool ExMoveTowards(this Transform transform, Vector3 targetPosition, float speed)
        {
            if (transform == null) { return false; }
            var currentPosition = transform.position;
            if (currentPosition == targetPosition) { return true; }
            var step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, step);
            return transform.position == targetPosition;
        }

        /// <summary>
        /// Rotates a Transform towards a target rotation at a specified speed, returning true if the target is reached.
        /// </summary>
        /// <param name="transform">The Transform to rotate.</param>
        /// <param name="targetRotation">The target rotation to rotate towards.</param>
        /// <param name="speed">The speed at which to rotate.</param>
        /// <returns>True if the Transform has reached the target rotation; otherwise, false.</returns>
        public static bool ExRotateTowards(this Transform transform, Quaternion targetRotation, float speed)
        {
            if (transform == null) { return false; }
            var currentRotation = transform.rotation;
            if (currentRotation == targetRotation) { return true; }
            var step = speed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, step);
            return transform.rotation == targetRotation;
        }

        /// <summary>
        /// Rotates a Transform to face a target position at a specified speed, returning true if the target is reached.
        /// </summary>
        /// <param name="transform">The Transform to rotate.</param>
        /// <param name="targetPosition">The target position to face.</param>
        /// <param name="speed">The speed at which to rotate.</param>
        /// <returns>True if the Transform has reached the target position; otherwise, false.</returns>
        public static bool ExLookAt(this Transform transform, Vector3 targetPosition, float speed)
        {
            if (transform == null) { return false; }
            var targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            return transform.ExRotateTowards(targetRotation, speed);
        }

        /// <summary>
        /// Moves a Transform towards a target position and rotates it to face that position at specified speeds, returning true if the target is reached.
        /// </summary>
        /// <param name="transform">The Transform to move and rotate.</param>
        /// <param name="targetPosition">The target position to move towards and face.</param>
        /// <param name="moveSpeed">The speed at which to move.</param>
        /// <param name="rotateSpeed">The speed at which to rotate.</param>
        /// <returns>True if the Transform has reached the target position; otherwise, false.</returns>
        public static bool ExMoveAndLookAt(this Transform transform, Vector3 targetPosition, float moveSpeed, float rotateSpeed)
        {
            if (transform == null) { return false; }
            var reachedPosition = transform.ExMoveTowards(targetPosition, moveSpeed);
            var reachedRotation = transform.ExLookAt(targetPosition, rotateSpeed);
            return reachedPosition && reachedRotation;
        }
    }
}