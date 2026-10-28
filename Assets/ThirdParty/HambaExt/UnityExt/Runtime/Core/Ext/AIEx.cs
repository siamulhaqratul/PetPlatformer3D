using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UnityExt
{
    /// <summary>
    /// Extension methods for Unity AI/NavMesh helpers, providing random and sampled positions on the NavMesh for Transforms.
    /// </summary>
    public static class ExAI
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Returns a random valid position on the NavMesh within a given radius of the Transform's position.
        /// </summary>
        /// <param name="transform">The Transform whose position is the center of the search area.</param>
        /// <param name="radius">The radius within which to search for a valid NavMesh position.</param>
        /// <returns>A valid NavMesh position within the radius, or Vector3.zero if none found.</returns>
        public static Vector3 ExRandomNavmeshLocation(this Transform transform, float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += transform.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

        /// <summary>
        /// Samples a position on the NavMesh in a given direction from the agent's position.
        /// </summary>
        /// <param name="agent">The Transform to sample from.</param>
        /// <param name="direction">The direction vector relative to the agent's position.</param>
        /// <returns>A valid NavMesh position in the given direction, or a fallback position if none found.</returns>
        public static Vector3 ExSamplePositionOnNavMesh(this Transform agent, Vector3 direction)
        {
            Vector3 sourcePosition = direction;
            sourcePosition += agent.transform.position;
            NavMeshHit hit;
            Vector3 finalPosition = agent.position + direction.normalized * 2f;
            if (NavMesh.SamplePosition(sourcePosition, out hit, 10f, NavMesh.AllAreas))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }
    }
}