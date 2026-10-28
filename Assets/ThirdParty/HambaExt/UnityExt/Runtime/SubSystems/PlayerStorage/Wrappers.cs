using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.DataSaving
{
    /*
     * WRAPPER CLASSES FOR UNITY MATH TYPES
     * 
     * PURPOSE:
     * Unity's JsonUtility cannot directly serialize certain built-in value types like Ray, Bounds, etc.
     * These wrapper classes provide a serializable container for these types by breaking them down
     * into their component parts that JsonUtility can handle (Vector2, Vector3, etc.).
     * 
     * USAGE PATTERN:
     * 1. To Save: UnityType -> WrapperClass -> JSON string
     * 2. To Load: JSON string -> WrapperClass -> UnityType
     * 
     * PERFORMANCE NOTES:
     * - These are temporary objects created only during serialization/deserialization
     * - Minimal memory overhead due to simple structure
     * - JsonUtility serialization is faster than custom string parsing
     * 
     * DESIGN DECISIONS:
     * - All fields are internal for package-level access
     * - SerializeField ensures JSON serialization works properly
     * - Simple structure with no methods to minimize overhead
     * - Consistent naming pattern: TypeName_Wrapper
     */

    /// <summary>
    /// Serializable wrapper for Unity's Ray struct.
    /// Ray cannot be directly serialized by JsonUtility, so we break it into its Vector3 components.
    /// Usage: Ray(origin, direction) <-> Ray_Wrapper{rayOrigin, rayDirection}
    /// </summary>
    [System.Serializable]
    internal class Ray_Wrapper
    {
        /// <summary>Starting point of the ray in world space</summary>
        [SerializeField] internal Vector3 rayOrigin;
        
        /// <summary>Direction vector of the ray (should be normalized)</summary>
        [SerializeField] internal Vector3 rayDirection;
    }

    /// <summary>
    /// Serializable wrapper for Unity's Ray2D struct.
    /// Ray2D cannot be directly serialized by JsonUtility, so we break it into its Vector2 components.
    /// Usage: Ray2D(origin, direction) <-> Ray2D_Wrapper{rayOrigin, rayDirection}
    /// </summary>
    [System.Serializable]
    internal class Ray2D_Wrapper
    {
        /// <summary>Starting point of the 2D ray in world space</summary>
        [SerializeField] internal Vector2 rayOrigin;
        
        /// <summary>Direction vector of the 2D ray (should be normalized)</summary>
        [SerializeField] internal Vector2 rayDirection;
    }

    /// <summary>
    /// Serializable wrapper for Unity's Bounds struct.
    /// Bounds cannot be directly serialized by JsonUtility, so we break it into center and size.
    /// Usage: Bounds(center, size) <-> Bounds_Wrapper{center, size}
    /// </summary>
    [System.Serializable]
    internal class Bounds_Wrapper
    {
        /// <summary>Center point of the bounding box in world space</summary>
        [SerializeField] internal Vector3 center;
        
        /// <summary>Total size of the bounding box (width, height, depth)</summary>
        [SerializeField] internal Vector3 size;
    }

    /// <summary>
    /// Serializable wrapper for Unity's BoundsInt struct.
    /// BoundsInt cannot be directly serialized by JsonUtility, so we break it into position and size.
    /// Usage: BoundsInt(position, size) <-> BoundsInt_Wrapper{position, size}
    /// Note: Uses position instead of center for integer-based bounds
    /// </summary>
    [System.Serializable]
    internal class BoundsInt_Wrapper
    {
        /// <summary>Bottom-left-back corner position of the integer bounding box</summary>
        [SerializeField] internal Vector3Int position;
        
        /// <summary>Total size of the integer bounding box (width, height, depth)</summary>
        [SerializeField] internal Vector3Int size;
    }

    /// <summary>
    /// Serializable wrapper for Unity's Rect struct.
    /// Rect cannot be directly serialized by JsonUtility, so we break it into position and size.
    /// Usage: Rect(position, size) <-> Rect_Wrapper{position, size}
    /// </summary>
    [System.Serializable]
    internal class Rect_Wrapper
    {
        /// <summary>Bottom-left corner position of the rectangle (x, y coordinates)</summary>
        [SerializeField] internal Vector2 position;
        
        /// <summary>Total size of the rectangle (width, height)</summary>
        [SerializeField] internal Vector2 size;
    }

    /// <summary>
    /// Serializable wrapper for Unity's RectInt struct.
    /// RectInt cannot be directly serialized by JsonUtility, so we break it into position and size.
    /// Usage: RectInt(position, size) <-> RectInt_Wrapper{position, size}
    /// </summary>
    [System.Serializable]
    internal class RectInt_Wrapper
    {
        /// <summary>Bottom-left corner position of the integer rectangle (x, y coordinates)</summary>
        [SerializeField] internal Vector2Int position;
        
        /// <summary>Total size of the integer rectangle (width, height)</summary>
        [SerializeField] internal Vector2Int size;
    }

    /// <summary>
    /// Serializable wrapper for Unity's Vector2Int struct.
    /// While Vector2Int might be serializable in newer Unity versions, this wrapper ensures
    /// consistent behavior across all Unity versions and provides explicit control.
    /// Usage: Vector2Int(x, y) <-> Vector2Int_Wrapper{data}
    /// </summary>
    [System.Serializable]
    internal class Vector2Int_Wrapper
    {
        /// <summary>The Vector2Int data being wrapped (x, y integer coordinates)</summary>
        [SerializeField] internal Vector2Int data;
    }

    /// <summary>
    /// Serializable wrapper for Unity's Vector3Int struct.
    /// While Vector3Int might be serializable in newer Unity versions, this wrapper ensures
    /// consistent behavior across all Unity versions and provides explicit control.
    /// Usage: Vector3Int(x, y, z) <-> Vector3Int_Wrapper{data}
    /// </summary>
    [System.Serializable]
    internal class Vector3Int_Wrapper
    {
        /// <summary>The Vector3Int data being wrapped (x, y, z integer coordinates)</summary>
        [SerializeField] internal Vector3Int data;
    }

    /// <summary>
    /// Serializable wrapper for Unity's Vector2 struct.
    /// While Vector2 is typically serializable, this wrapper provides consistency with other
    /// wrapper classes and explicit control over serialization format.
    /// Usage: Vector2(x, y) <-> Vector2_Wrapper{data}
    /// </summary>
    [System.Serializable]
    internal class Vector2_Wrapper
    {
        /// <summary>The Vector2 data being wrapped (x, y float coordinates)</summary>
        [SerializeField] internal Vector2 data;
    }

    /// <summary>
    /// Serializable wrapper for Unity's Vector3 struct.
    /// While Vector3 is typically serializable, this wrapper provides consistency with other
    /// wrapper classes and explicit control over serialization format.
    /// Usage: Vector3(x, y, z) <-> Vector3_Wrapper{data}
    /// </summary>
    [System.Serializable]
    internal class Vector3_Wrapper
    {
        /// <summary>The Vector3 data being wrapped (x, y, z float coordinates)</summary>
        [SerializeField] internal Vector3 data;
    }

    /*
     * UNITY JSONUTILITY LIMITATIONS ADDRESSED:
     * 
     * 1. UNSUPPORTED TYPES:
     *    - Ray, Ray2D: No direct serialization support
     *    - Bounds, BoundsInt: No direct serialization support
     *    - Rect, RectInt: Inconsistent support across Unity versions
     * 
     * 2. VERSION COMPATIBILITY:
     *    - Vector2Int, Vector3Int: Support varies by Unity version
     *    - Consistent behavior across all Unity versions with wrappers
     * 
     * 3. CONTROL AND CONSISTENCY:
     *    - Explicit field naming for JSON output
     *    - Guaranteed serialization format
     *    - No dependency on Unity's internal serialization changes
     * 
     * ALTERNATIVE APPROACHES CONSIDERED:
     * - Custom string parsing: More complex, error-prone, slower
     * - Third-party JSON libraries: External dependency, larger footprint
     * - Binary serialization: Not human-readable, less debuggable
     * - ISerializationCallbackReceiver: More complex implementation
     * 
     * EXTENSION GUIDELINES:
     * - Follow naming pattern: TypeName_Wrapper
     * - Use [SerializeField] for all data fields
     * - Keep classes simple (data-only, no methods)
     * - Use internal access for package-level visibility
     * - Document the Unity type being wrapped and usage pattern
     */
}