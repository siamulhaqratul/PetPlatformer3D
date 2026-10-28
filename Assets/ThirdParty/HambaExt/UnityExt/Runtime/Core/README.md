# HambaExt Core Documentation

## Overview

HambaExt Core is a fundamental module providing essential utilities, extensions, and components that serve as the foundation for the HambaExt Unity framework. This module offers a comprehensive collection of reusable code designed to streamline Unity development workflows and establish consistent patterns across projects.

## Folder Structure

The Core module is organized into three main sections:

```
Core/
├── Components/      # Reusable Unity components
├── Ext/             # Extension methods for Unity and .NET types
│   └── Collection/  # Collection-specific extension methods
├── Utility/         # Utility classes for common operations
└── Data.cs          # Core data structures and interfaces
```

## Components

### CameraFacer

#### Overview

`CameraFacer` is a utility component that automatically rotates a GameObject to face the camera. It provides a simple way to implement billboarding effects in Unity, making objects like UI elements, sprites, or visual effects always face the player's view.

#### Location

```
Assets/ThirdParty/HambaExt/UnityExt/Runtime/Core/Components/CameraFacer.cs
```

#### Features

- **Automatic Camera Tracking**: Automatically detects and faces the main camera
- **Optional Rotation Control**: Can optionally apply a 180° Y-axis rotation after facing the camera
- **Lightweight**: Minimal performance impact, suitable for many instances

#### Public Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsNotRotate` | `bool` | When true, disables the 180° Y-axis rotation after facing the camera |

#### Usage

##### Basic Setup
    1. Select any GameObject you want to always face the camera
    2. Add the `CameraFacer` component:
        - Via Inspector: Add Component → UnityExt → CameraFacer
        - Via code: `gameObject.AddComponent<CameraFacer>()`

##### Configuration
    In the Inspector:
    - **Is Not Rotate**: Check this box to disable the additional 180° Y-axis rotation

##### Code Example

```csharp
using UnityEngine;
using UnityExt;

public class BillboardExample : MonoBehaviour
{
    [SerializeField] private GameObject _uiElement;
    
    private void Start()
    {
        // Add CameraFacer to a UI element
        CameraFacer facer = _uiElement.AddComponent<CameraFacer>();
        
        // Optional: Configure rotation behavior
        // If true, the object will directly face the camera without the 180° flip
        // If false (default), the object will face the camera and then rotate 180° around Y-axis
        facer.IsNotRotate = true;
    }
}
```

#### Common Use Cases

- **UI Elements in 3D Space**: Health bars, nameplates, or indicators above characters
- **Billboard Sprites**: Trees, particles, or 2D elements in a 3D world
- **VFX**: Ensuring effects like explosions or impacts always face the camera
- **Debug Visualizations**: Making debug elements always visible to the player

#### Implementation Details

- The component automatically finds and tracks the main camera
- Rotation is applied every frame in the Update method
- Uses Unity's `LookAt()` method with the up vector set to world up (Vector3.up)

#### Tips and Best Practices

- For performance with many instances, consider updating less frequently (using coroutines or time-based checks)
- Use `IsNotRotate` when working with pre-rotated sprites or UI elements
- Consider using this component on parent objects to avoid affecting child object rotations

#### Dependencies

- Requires a Camera.main to be present in the scene

## Extension Methods

HambaExt provides numerous extension methods organized by type and functionality:

### GameObject & Transform Extensions

#### Overview

HambaExt Core provides a comprehensive suite of extension methods for Unity's `GameObject` and `Transform` classes. These extensions enhance the basic Unity functionality by adding convenient methods for common operations, efficient hierarchy traversal, component retrieval, and UI animations.
- `GameobjectEx.cs`: Extensions for GameObject manipulation, instantiation, and component management
- `TransformEx.cs`: Helper methods for Transform operations like positioning, rotation, and hierarchy management
- `GetCompEx.cs`: Simplified component access patterns and caching utilities

#### Location

```
Assets/ThirdParty/HambaExt/UnityExt/Runtime/Core/Ext/
├── GameobjectEx.cs
├── TransformEx.cs
└── GetCompEx.cs
```

#### GameObject Extensions (GameobjectEx) Examples

##### Activation Management

Extension methods for safely handling GameObject activation:

```csharp
// Safely activate/deactivate a GameObject (null-check included)
gameObject.ExSetActiveForce(true);

// Set activation state for a list of GameObjects
List<GameObject> uiElements = GetUIElements();
uiElements.ExSetActiveForce(false);

// Only change activation if current state differs from target
uiElements.ExSetActiveObjects(true);
```

##### Layer Management

Methods for efficient layer assignment across object hierarchies:

```csharp
// Set layer by name instead of using LayerMask.NameToLayer directly
gameObject.ExSetLayer("UI");

// Set layer for all children recursively
parentObject.ExSetLayerToAllChilds("Interactable");

// Set layer for immediate children only
parentObject.ExSetLayerToImmediateChilds("Default");

// Set layer for all parent objects
childObject.ExSetLayerToParents("Environment");
```

#### Transform Extensions (TransformEx) Examples

##### UI Animation

Tween-based methods for showing/hiding UI elements:

```csharp
// Show a transform by scaling from zero to one
transform.ExShow(() => Debug.Log("Animation completed"));

// For UI elements (RectTransform)
rectTransform.ExShow();

// Hide by scaling to zero and deactivate when done
transform.ExHide();

// Open/close UI panels with animation
panelTransform.ExSetUIPanel(ref isPanelOpen, InteractionType.Open);
```

##### Transform Manipulation

Utilities for modifying transform properties:

```csharp
// Reset local position, rotation, and scale
transform.ExResetLocal();

// Reset multiple transforms
transformsList.ExResetLocal();

// Set global scale by adjusting local scale appropriately
transform.ExSetGlobalScale(new Vector3(1, 1, 1));

// Convert world position to canvas position for UI placement
Vector3 canvasPos = worldObject.ExWorldToCanvasPosition(canvasRectTransform);
```

##### Hierarchy Traversal

Methods for navigating and manipulating transform hierarchies:

```csharp
// Get a string representation of the object hierarchy
string path = transform.ExGetHierarchy(); // "->Child->Parent->Root"

// Get all immediate children
List<Transform> children = transform.ExGetImmediateChilds();

// Get all immediate children as an alternative method
List<Transform> children = transform.ExGetAllImmediateChilds();

// Destroy all child objects
parentTransform.ExDestroyEverythingInside();
```

##### Powerful Hierarchy Iteration

A comprehensive set of iteration methods for transforms:

```csharp
// Execute code for each immediate child
transform.ExForEachImmediateChilds(child => {
    Debug.Log(child.name);
});

// Process parent chain upward
transform.ExForEachParentOnChain(parent => {
    parent.gameObject.tag = "Processed";
});

// Recursively process all descendants
transform.ExForEachChilds(child => {
    child.localScale = Vector3.one;
});

// With additional parameters and async support
await transform.ExForEachChilds<object>(async (child, index, siblingIndex) => {
    await SomeAsyncOperation(child);
});

// Reverse order iteration
transform.ExForEachReverseImmediateChilds(child => {
    // Process children in reverse order
});

// Conditional breaking
transform.ExForEachChilds(
    child => { ProcessTransform(child); },
    child => child.name == "StopHere" // Break condition
);
```

#### Component Retrieval (GetCompEx) Examples

Methods for efficiently finding and collecting components:

```csharp
// Get all components of type T in children recursively
List<Button> buttons = transform.ExGetComponentsInChildrenSerially<Button>();

// Include inactive GameObjects in search
List<Collider> colliders = transform.ExGetComponentsInChildrenSerially<Collider>(includeInactive: true);

// Get components in parent chain
List<Canvas> canvases = transform.ExGetComponentsInParentSerially<Canvas>();
```

#### Best Practices

- Use extension methods rather than duplicating common transform operations
- Consider performance impact when using recursive methods on deep hierarchies
- Cache results of component searches when used repeatedly
- Use the UI animation methods with DOTween for smooth transitions
- For complex hierarchies, use the conditional breaking methods to limit traversal

#### Tips

- The `ExForEach` methods are more readable alternatives to standard Unity loops
- Use the async variants for operations that might take significant time
- The `ExGetHierarchy()` method is useful for debugging object paths
- `ExResetLocal()` is a convenient shorthand for resetting transforms

These extension methods significantly reduce boilerplate code and provide a more fluent API for working with Unity's transform and GameObject systems.

### Math & Vector Extensions

#### Overview

HambaExt Core provides specialized extension methods for mathematical operations, vector manipulations, and numeric utilities that extend Unity's built-in functionality. These extensions help simplify common geometric calculations, transformations, and data formatting tasks.
- `VectorEx.cs`: Vector manipulation, conversion, and calculation helpers
- `NumbersEx.cs`: Numeric operations, interpolation, and utility functions
- `RotationEx.cs`: Quaternion and rotation helper methods
- `CurveExt.cs`: Extensions for AnimationCurve and curve-related operations
#### Location

```
Assets/ThirdParty/HambaExt/UnityExt/Runtime/Core/Ext/
├── VectorEx.cs
├── NumbersEx.cs
├── RotationEx.cs
└── CurveExt.cs
```

#### Vector Extensions (VectorEx) Examples

Vector extensions provide geometric calculations and spatial operations:

```csharp
// Find intersection point between two lines
Vector2 point1 = new Vector2(0, 0);
Vector2 point2 = new Vector2(10, 10);
Vector2 point3 = new Vector2(0, 10);
Vector2 point4 = new Vector2(10, 0);
Vector2 intersection = point1.ExGetIntersectionPointCoordinates(point2, point3, point4, out bool found);
if (found) {
    Debug.Log($"Intersection at {intersection}");
}

// Get midpoint between two positions
Vector3 midpoint = playerPosition.ExGetMidPoint(targetPosition);

// Test if moving past a point
bool passedCheckpoint = currentPosition.ExHasPassedPoint(checkpointPosition, movementDirection);

// Get point at normalized distance along a line
Vector3 halfwayPoint = startPosition.ExGetPointAtNormalizedDistance(0.5f, endPosition);
```

#### Numeric Extensions (NumbersEx) Examples

Extensions for numeric operations and formatting:

```csharp
// Test if a value is within a range
float health = 75f;
if (health.ExIsBetween(50f, 100f)) {
    // Health is between 50 and 100 (inclusive lower, exclusive upper)
}

// Test if a value is approximately equal to another
float targetSpeed = 5.0f;
if (currentSpeed.ExIsAround(targetSpeed, 0.1f)) {
    // Speed is within 0.1 of target
}

// Remap a value from one range to another
float normalizedHealth = health.ExRemap(0f, 100f, 0f, 1f);

// Format large numbers for UI display
long coins = 1250000;
string formattedCoins = coins.ExGetCoinUIString(); // "1.2M"

// Format integers with suffix
int score = 15500;
string formattedScore = score.ExGetCoinUIString(); // "15.5K" 
string formattedScoreTenK = score.ExGetCoinUIString(startsAfterTenK: true); // "15.5K"
```

#### Rotation Extensions (RotationEx) Examples

Extensions for simplified rotation around local axes:

```csharp
// Rotating around local X axis with delta tracking
float lastXAngle = 0f;
transform.ExSetRotationAroundLocalXAxis(30f, ref lastXAngle);

// Rotating around local Y axis with delta tracking
float lastYAngle = 0f;
transform.ExSetRotationAroundLocalYAxis(45f, ref lastYAngle);

// Rotating around local Z axis with delta tracking
float lastZAngle = 0f;
transform.ExSetRotationAroundLocalZAxis(15f, ref lastZAngle);
```

#### Animation Curve Extensions (CurveExt) Examples

Extensions for analyzing and extracting data from animation curves:

```csharp
// Get min/max time and values from an animation curve
AnimationCurve curve = new AnimationCurve(
    new Keyframe(0f, 1f),
    new Keyframe(0.5f, 2.5f),
    new Keyframe(1.5f, 0.5f)
);

float minTime = 0f, maxTime = 0f, minValue = 0f, maxValue = 0f;
curve.ExGetCurveUltima(ref minTime, ref maxTime, ref minValue, ref maxValue);

// minTime = 0f, maxTime = 1.5f, minValue = 0.5f, maxValue = 2.5f
```

#### Use Cases

##### Game Mechanics & Controls

```csharp
// Camera rotation with delta angle tracking
private float _lastYaw = 0f;
private float _lastPitch = 0f;

void UpdateCameraRotation(float yawInput, float pitchInput) {
    // Apply input as rotation around local axes
    _cameraTransform.ExSetRotationAroundLocalYAxis(yawInput, ref _lastYaw);
    _cameraTransform.ExSetRotationAroundLocalXAxis(pitchInput, ref _lastPitch);
}
```

##### UI Display

```csharp
// Format and display player resources
void UpdateResourceUI() {
    _goldText.text = $"Gold: {_playerGold.ExGetCoinUIString()}";
    _gemsText.text = $"Gems: {_playerGems.ExGetCoinUIString()}";
}
```

##### World Interactions

```csharp
// Check if player has crossed a trigger line
void Update() {
    if (_playerPosition.ExHasPassedPoint(_finishLine, _playerVelocity)) {
        CompleteRace();
    }
}
```

##### Animation Analysis

```csharp
// Extract animation timing information
void AnalyzeAnimationCurve() {
    float minTime = 0f, maxTime = 0f, minValue = 0f, maxValue = 0f;
    _animationCurve.ExGetCurveUltima(ref minTime, ref maxTime, ref minValue, ref maxValue);
    
    float duration = maxTime - minTime;
    float animRange = maxValue - minValue;
    Debug.Log($"Animation duration: {duration}, value range: {animRange}");
}
```

#### Tips & Best Practices

- Use `ExIsAround()` for float comparisons instead of direct equality checks
- Use `ExRemap()` to convert values between different ranges (e.g., converting raw input to normalized values)
- For high-performance code, cache the results of vector calculations instead of recomputing them
- Use `ExGetCoinUIString()` to display large numbers in a readable format on UI elements

These extension methods help simplify common mathematical operations in Unity development while improving code readability and reducing boilerplate.

### Collection Extensions

#### Overview

HambaExt Core provides a comprehensive suite of collection extension methods designed to simplify working with arrays, lists, dictionaries, and read-only collections in Unity. These extensions enhance standard .NET collections with powerful iteration patterns, null-safety features, and Unity-specific optimizations.
- `CollectionEx.cs`: General collection utilities
- `IListEx.cs`: Extensions for IList interfaces
- `IReadOnlyListEx.cs`: Extensions for IReadOnlyList interfaces
- `IDictionaryEx.cs`: Extensions for IDictionary interfaces
- `IReadOnlyDictionaryEx.cs`: Extensions for IReadOnlyDictionary interfaces

#### Location

```
Assets/ThirdParty/HambaExt/UnityExt/Runtime/Core/Ext/Collection/
├── CollectionEx.cs     
├── IListEx.cs          
├── IReadOnlyListEx.cs  
├── IDictionaryEx.cs    
└── IReadOnlyDictionaryEx.cs  
```

#### Key Features

- **Type-Safe Operations**: Specialized handling for Unity objects, custom classes, and value types
- **Null-Safety**: Robust null handling to prevent NullReferenceExceptions
- **Performance Optimizations**: No-check variants for high-performance scenarios
- **Iteration Patterns**: Forward, reverse, conditional, and indexed iteration
- **Async Support**: Task-based asynchronous operations for all collection types
- **Unity-Specific**: Special handling for Unity's reference semantics and destroyed objects

#### Basic Collection Utilities (CollectionEx) Examples

Core utilities for working with generic collections:

```csharp
// Check if array contains null elements
bool hasNulls = myArray.ExHasAnyNull();

// Count null and non-null elements
int nullCount = myArray.ExNullCount();
int validCount = myArray.ExNotNullCount();

// Create a new list with fixed count
var newList = myList.ExGetListWithCount(10);

// Create shallow copies
List<Transform> transformsCopy = transformList.ExShallowCopy();
Vector3[] positionsCopy = positionsArray.ExShallowCopyData();

// Create deep copies (for ICloneable objects)
List<CustomData> dataCopy = dataList.ExDeepCopyCustomClass();

// Filter out nulls
var cleanList = dirtyList.ExRemoveNulls();
var cleanObjects = objectList.ExRemoveNullsUnityObjects();

// Check if collection is valid (non-null and not empty)
if (myArray.ExIsValid()) {
    // Safe to use
}
```

#### List Extensions (IListEx) Examples

Rich extensions for working with mutable lists:

##### Safe Iteration

```csharp
// Safe iteration (skips nulls automatically)
objectList.ExForEachSafe(obj => {
    obj.transform.position = Vector3.zero; // No null checks needed
});

// Value type iteration
vectorList.ExForEach(vec => {
    Debug.Log(vec.magnitude);
});

// Custom class iteration with null skipping
playerDataList.ExForEachSafeCustomClass(data => {
    data.ProcessScore();
});

// With index information
transformList.ExForEachSafe((transform, index) => {
    transform.SetSiblingIndex(index);
});

// Asynchronous iteration
await enemyList.ExForEachAsyncSafe(async enemy => {
    await enemy.InitializeAsync();
});
```

##### Advanced Iteration

```csharp
// Reverse iteration
transformList.ExForEachReverseSafe(transform => {
    // Process from last to first
});

// Conditional iteration with break
enemyList.ExForEachSafe(
    enemy => { enemy.Target = player; },
    enemy => enemy.IsDefeated // Break when we find a defeated enemy
);

// High-performance iteration (no validation checks)
cachedList.ExForEach_NoCheck(item => {
    // Fast path when you know list is valid
});
```

##### Collection Manipulation

```csharp
// Randomize list order
enemySpawnPoints.ExShuffle();

// Remove duplicates
tagList.ExRemoveDuplicates();

// Add unique elements
playerList.ExAddRangeUniquely(newPlayers);
idList.ExAddUniquelyCustomClass(newId);

// Get and remove a random element
Enemy randomEnemy = enemyList.ExGetAnyRandomAndDeleteFromList();
```

#### Read-Only List Extensions (IReadOnlyListEx) Examples

Similar capabilities for read-only collections:

```csharp
// Safe iteration patterns for read-only lists
configData.ExForEachRO(item => {
    ProcessConfigItem(item);
});

// Map-based iteration (correlating two lists)
playerList.ExForEachMapRO(scoreList, (player, score, playerIdx, scoreIdx) => {
    if (score != null) {
        player.DisplayScore(score.value);
    }
});

// Safe containment check
bool hasImportantItem = collection.ExContainsOptimizedRO(searchItem);
```

#### Dictionary Extensions (IDictionaryEx) Examples

Type-safe dictionary operations with specialized handling:

```csharp
// Type-safe iteration (struct keys, struct values)
coordinateMap.ExForEach_SS((position, data) => {
    Debug.Log($"Position {position}: {data}");
});

// Unity objects with null safety
objectMappings.ExForEach_UU((key, value) => {
    // Both key and value guaranteed non-null here
});

// Optimized iteration using ReferenceEquals for Unity objects
sceneObjects.ExForEach_UU_Optimized((key, value) => {
    // Better performance for large collections
});

// Struct keys with Unity object values
playerScores.ExForEach_SU((playerId, scoreObject) => {
    // scoreObject guaranteed non-null
});

// Async operations
await entityMap.ExForEach_UC(async (entity, data) => {
    await entity.ProcessDataAsync(data);
});
```

##### Dictionary Utilities

```csharp
// Check if dictionary is valid
if (lookupTable.ExIsValid()) {
    // Safe to use
}

// Count null entries
int nullKeyCount = objectDict.ExNullKeyCount_UU();
int nullValueCount = objectDict.ExNullValueCount_UU();

// Clean up dictionaries
objectReferences.ExRemoveNullsOnIt_UU(); // Removes entries with null keys or values

// Merge dictionaries
baseConfig.ExJoin_SS(overrideConfig); // Adds or overwrites entries
```

#### Read-Only Dictionary Extensions (IReadOnlyDictionaryEx) Examples

Similar operations for read-only dictionaries:

```csharp
// Type-safe iteration with null safety
configSettings.ExForEachRO_SC((key, value) => {
    // value guaranteed non-null
});

// Optimized Unity object handling
prefabInstances.ExForEachRO_UU_Optimized((prefab, instance) => {
    // Faster null checks
});

// Validation
if (readOnlyMap.ExIsValidRO()) {
    // Safe to use
}

// Count null entries
int nullCount = dataStore.ExNullValueCountRO_UC();
```

#### Performance Considerations

- Use `_NoCheck` variants when you've already validated collections for performance-critical code
- For Unity objects, the `_Optimized` variants use `ReferenceEquals()` for better performance
- Consider the async variants for operations that might take significant time
- When iteration order doesn't matter, use forward iteration for best performance

#### Best Practices

- Use the appropriate type-specific variant (`Safe` for Unity objects, `CustomClass` for normal C# classes)
- Consider using breakable iteration patterns to stop processing early when a condition is met
- For dictionaries, use specialized type combinations (SS, SC, UC, UU, etc.) for type safety
- Always validate collections before use with `ExIsValid()` or by using the safe iteration methods
- Use the asynchronous variants when performing network or I/O operations during iteration

These collection extensions provide a robust foundation for handling data structures in Unity projects, with special consideration for Unity's object reference semantics and common game development patterns.


### Other Extensions

#### Overview

HambaExt Core provides a diverse collection of specialized extension methods that don't fit neatly into other categories but deliver significant utility across various Unity development scenarios. These extensions cover physics interactions, optimization, type discovery, UI manipulation, AI/navigation, and movement control.
- `MovementEx.cs`: Movement calculation and pathfinding helpers
- `AIEx.cs`: Artificial intelligence and behavior utilities
- `UIEx.cs`: Unity UI extensions and helper methods
- `TrailEx.cs`: Extensions for TrailRenderer and similar components
- `OptimizerEx.cs`: Performance optimization utilities
- `TypeSystemEx.cs`: Reflection and type-related helpers
- `OtherEx.cs`: Miscellaneous extensions

#### Location

```
Assets/ThirdParty/HambaExt/UnityExt/Runtime/Core/Ext/
├── OtherEx.cs       
├── TypeSystemEx.cs  
├── OptimizerEx.cs   
├── TrailEx.cs       
├── UIEx.cs          
├── AIEx.cs          
└── MovementEx.cs    
```

#### Physics and Raycast Extensions (OtherEx) Examples

Extensions for efficient physics and raycast operations:

```csharp
// Reset RaycastHit array elements
RaycastHit[] hits = new RaycastHit[10];
Physics.RaycastNonAlloc(ray, hits, 100f);
hits.ResetHits(10);

// Check if any hit has a component of type T
if (hits.HasFoundTagType<Enemy>(hitCount, out var hitCollider)) {
    Debug.Log($"Hit enemy with collider: {hitCollider.name}");
}

// Check for specific collider with component
if (hits.HasFoundTagTypeAndCollider<Enemy>(hitCount, targetCollider)) {
    Debug.Log("Hit the specific enemy we were looking for");
}

// Get object name safely
string objName = someObject.ExGetObjectName(); // Returns "NULL" if object is null
```


#### Task and Coroutine Management (OtherEx) Examples

Tools for managing asynchronous operations:

```csharp
// Kill all tasks in a list
taskHandles.KillRunningTasks(this, "Level transition");

// Kill all coroutines in a list
coroutineHandles.KillRunningTasks(this);

// Flush a single coroutine
this.ExFlushCoroutine(ref _animationCoroutine);

// Flush multiple coroutines
this.ExFlushCoroutine(ref _activeCoroutines);

// Fresh-bind a button's onClick event
confirmButton.ExFreshBind(() => {
    ProcessConfirmation();
});
```

#### Material and Component Management (OtherEx) Examples

Utilities for working with materials and components:

```csharp
// Set shared material for all renderers in children
modelRoot.ExSetSharedMaterial(highlightMaterial);

// Reset a rigidbody to default values
rigidbody.ExSetDefault();

// Extract implementations of interfaces from GameObject hierarchies
List<IInputHandler> inputHandlers = new List<IInputHandler>();
rootObjects.ExtractImplementationsFrom(inputHandlers);

// Extract a single implementation
IDataManager dataManager = null;
rootObjects.ExtractImplementationsFrom(ref dataManager);
```

#### Utility Extensions (OtherEx) Examples

Miscellaneous helpers for common tasks:

```csharp
// Check if string is valid (not null, empty, or whitespace)
if (inputText.IsStringValid()) {
    ProcessInput(inputText);
}

// Get position on platform below point
Vector3 groundPosition = airPosition.ExGetPositionInPlatform(groundLayerMask);
```

#### Performance Optimization (OptimizerEx) Examples

Tools for optimizing rendering performance:

```csharp
// Optimize all skinned mesh renderers in a hierarchy
characterModel.ExOptimizeSkinnedRenderersInside(
    shadow: false,      // Disable shadows
    occlusionCulling: true  // Enable occlusion culling
);

// Optimize all mesh renderers in a hierarchy
environmentObject.ExOptimizeMeshRenderersInside(
    shadow: true,       // Enable shadows
    occlusionCulling: true  // Enable occlusion culling
);

// Optimize individual renderers
skinnedRenderer.ExOptimize(shadow: false, occlusionCulling: true);
meshRenderer.ExOptimize(shadow: true, occlusionCulling: false);
```

#### Type Discovery and Conversion (TypeSystemEx) Examples

Extensions for finding implementations and converting collections:

```csharp
// Find all implementations of an interface in the active scene
List<IInteractable> interactables = this.GetImplementationFromCurrentlyLoadedMainScene<IInteractable>(includeInactive: true);

// Find all objects implementing an interface across all scenes
List<IDamageable> damageables = this.GetImplementedInterfaces<IDamageable>();

// Get the first implementation of an interface
IGameManager gameManager = this.GetImplementedInterface<IGameManager>();

// Check if a type implements an interface
bool implementsInterface = typeof(MyClass).ExInterfaceIsImplementedOnClass<IMyInterface>();

// Extract specific type from a collection
EnemyController enemyController = controllers.ExGetTypedScript<EnemyController, BaseController>();

// Convert MonoBehaviour collections to interface collections
List<IWeapon> weapons = monoBehaviours.ExConvertTo<IWeapon>();
```

#### UI Extensions (UIEx) Examples

Utilities for working with UI graphics:

```csharp
// Validate UI elements in development builds
uiElements.CheckGraphics("MainMenuPanel");

// Set alpha on a single graphic
fadeImage.ExSetAlpha(0.5f);

// Set alpha on multiple graphics
allUIElements.ExSetAlpha(0.7f, excludedElement1, excludedElement2);

// Enable/disable UI elements
allUIElements.ExSetActive(true);  // Enable all
allUIElements.ExSetActive(false, permanentElement); // Disable all except permanentElement
```

#### AI and Navigation Extensions (AIEx) Examples

Helpers for AI navigation:

```csharp
// Get random position on NavMesh within radius
Vector3 randomPosition = transform.ExRandomNavmeshLocation(10f);

// Sample position on NavMesh in specific direction
Vector3 targetPosition = transform.ExSamplePositionOnNavMesh(Vector3.forward * 5f);
```

#### Movement Extensions (MovementEx) Examples

Simplified movement and rotation controls:

```csharp
// Move towards a position with speed
bool reached = transform.ExMoveTowards(targetPosition, 5f);

// Rotate towards a rotation with speed
bool aligned = transform.ExRotateTowards(targetRotation, 90f);

// Look at a position with rotation speed
transform.ExLookAt(enemyPosition, 120f);

// Combined move and look at
transform.ExMoveAndLookAt(
    targetPosition,
    moveSpeed: 5f,
    rotateSpeed: 90f
);
```

#### Best Practices

- Use `ExOptimizeSkinnedRenderersInside()` and `ExOptimizeMeshRenderersInside()` early in initialization for optimal performance
- Call `ExFlushCoroutine()` in `OnDisable()` or `OnDestroy()` to prevent leaks
- For interface discovery, prefer scene-specific methods over project-wide searches when possible
- Use `ExSetSharedMaterial()` instead of iterating through renderers manually
- When working with tasks or coroutines, maintain a list of handles for proper cleanup
- Use `KillRunningTasks()` when transitioning between states to prevent async operations from continuing

These extensions provide powerful tools that address common challenges in Unity development while maintaining high performance and clean code structure.

## Utility Classes

### Overview

HambaExt Core provides several utility classes that handle common operations for compression, encryption, encoding, and editor-related functionality. These utilities simplify complex operations into clean, reusable methods that follow best practices for performance and security.
- `CompressionUtil.cs`: Data compression and decompression utilities
- `CryptoUtil.cs`: Cryptography and security functions
- `EdUtil.cs`: Editor-specific utility methods
- `EncodingUtil.cs`: Data encoding and serialization helpers

### Location

```
Assets/ThirdParty/HambaExt/UnityExt/Runtime/Core/Utility/
├── CompressionUtil.cs
├── CryptoUtil.cs
├── EdUtil.cs
└── EncodingUtil.cs
```

### Compression Utilities (CompressionUtil) Examples

`CompressionUtil` provides methods for compressing and decompressing text data using GZip compression:

```csharp
// Compress JSON string using default encoding (from UConfig)
byte[] compressedData = CompressionUtil.CompressText(jsonString);

// Compress using specific encoding
byte[] compressedUtf8Data = CompressionUtil.CompressText(jsonString, GameTextEncodingType.UTF_8);

// Decompress back to string
string decompressedJson = CompressionUtil.DecompressByte(compressedData);

// Decompress to raw bytes instead of string
byte[] decompressedBytes = CompressionUtil.DecompressByteIntoByte(compressedData);
```

#### Common Use Cases

- Compressing save data before writing to disk
- Reducing network payload sizes for API requests
- Efficient storage of large text configurations

### Cryptography Utilities (CryptoUtil) Examples

`CryptoUtil` provides AES encryption for securing sensitive data:

```csharp
// Encrypt string using project's configured keys
string encryptedText = CryptoUtil.EncryptStringAESForSaveData("sensitive data");

// Decrypt string using project's configured keys
string decryptedText = CryptoUtil.DecryptStringAESForSaveData(encryptedText);

// Encrypt raw byte data
byte[] encryptedBytes = CryptoUtil.EncryptStringAESForSaveData(rawDataBytes);

// Decrypt byte data
string decryptedText = CryptoUtil.DecryptStringAESForSaveData(encryptedBytes);
```

#### Security Features

- Uses standard AES encryption
- Configurable shared secrets and salts via UConfig
- Prepends IV (Initialization Vector) for enhanced security
- Built-in validation in development builds

### Encoding Utilities (EncodingUtil) Examples

`EncodingUtil` provides methods for converting between strings and byte arrays with configurable encodings:

```csharp
// Get bytes using default project encoding
byte[] bytes = EncodingUtil.GetBytes("Hello world");

// Get bytes with specific encoding
byte[] asciiBytes = EncodingUtil.GetBytes("Hello world", GameTextEncodingType.ASCII);

// Convert bytes back to string using default project encoding
string text = EncodingUtil.GetString(bytes);
```

#### Supported Encodings

- ASCII
- Unicode (UTF-16 little-endian)
- BigEndianUnicode (UTF-16 big-endian)
- UTF-7
- UTF-8
- UTF-32

### Editor Utilities (EdUtil) Examples

`EdUtil` provides helper methods for scene management and asset operations, with special support for editor workflows:

```csharp
// Check if GameObject is from an FBX model asset
bool isModelPart = EdUtil.IsItFBXModelFile(gameObject);

// Convert absolute path to Unity-relative path
string relativePath = absoluteFilePath.AssetsRelativePath();

// Find asset by name search
Material material = EdUtil.TryLoadAsset<Material>("MetalShiny");

// Find GameObjects by name pattern in active scene
List<GameObject> lights = new List<GameObject>();
EdUtil.GetAllGameobjectsWitMatchingName(ref lights, "Light");

// Find GameObject with specific component in scene
GameObject managerObject = EdUtil.GetScriptObjectFromScene<GameManager>();

// Clean up child objects
EdUtil.DestroyAllImmediateChildsOf(parentObject);

// Get specific child by name
GameObject playerSpawn = EdUtil.GetImmediateChildWithName(levelObject, "PlayerSpawn");
```

### Best Practices

#### Compression

- Always validate input/output strings are non-null before compression operations
- Consider compression level vs. speed tradeoffs for your specific use case
- Use the byte-to-byte methods when working with binary data

#### Encryption

- Store encryption keys and salts securely in UConfig
- Never hardcode encryption keys in source code
- Use different keys for different types of data
- Consider adding integrity verification (HMAC) for sensitive data

#### Encoding

- Be consistent with encoding choices throughout your project
- Use UTF-8 for most web API and modern text processing needs
- Be aware of encoding compatibility when sharing data with other systems

#### Editor Utilities

- Consider using `#if UNITY_EDITOR` conditionals when extending editor functionality
- Cache results of expensive editor operations when possible
- Use proper asset management to avoid reference issues

### Performance Considerations

- Compression and encryption are CPU-intensive; consider running them on background threads
- Cache the results of encoding/compression operations when working with repeated data
- For editor utilities, be mindful of operations that might be slow in large scenes

These utility classes provide a foundation for common operations in Unity development, abstracting away the complexity of standard .NET libraries while providing Unity-specific optimizations and safety features.

## Core Data Structures

### Overview

The Core Data module (Data.cs) provides fundamental data structures, interfaces, and type definitions used throughout the HambaExt framework. This file contains core building blocks that help standardize common patterns and reduce code duplication across your Unity projects.

### Location

```
Assets/ThirdParty/HambaExt/UnityExt/Runtime/Core/Data.cs
```

### Interfaces Examples

#### ICustomAwakeHandler

An interface for objects that require custom initialization, similar to Unity's `Awake` method:

```csharp
// Implement this interface to add custom initialization logic
public class MyCustomComponent : MonoBehaviour, ICustomAwakeHandler
{
    public void OnCustomAwake()
    {
        // Custom initialization code
        Debug.Log("Custom initialization complete");
    }
}
```

### Utility Classes Examples

#### MarkerObject

A lightweight empty class used as a flag or state tracker:

```csharp
// Use as a key in dictionaries or as a state marker
private Dictionary<string, MarkerObject> _completedTasks = new Dictionary<string, MarkerObject>();

// Mark a task as completed
_completedTasks["tutorial"] = new MarkerObject();

// Check if marked
if (_completedTasks.ContainsKey("tutorial"))
{
    // Tutorial is marked as completed
}
```

#### ConstField<T>

A generic wrapper for creating inspector-editable but runtime-constant fields:

```csharp
// Define a constant field in a MonoBehaviour
[SerializeField] private ConstField<int> _maxHealth = new ConstField<int>(100);

// Access the value (read-only at runtime)
void TakeDamage(int damage)
{
    currentHealth = Mathf.Clamp(currentHealth - damage, 0, _maxHealth.Value);
}
```

#### BezierPathMinimal

A lightweight implementation for calculating cubic Bezier curves:

```csharp
// Create a new Bezier path with 20 points
BezierPathMinimal path = new BezierPathMinimal(20);

// Define control points
List<Vector3> controlPoints = new List<Vector3>
{
    new Vector3(0, 0, 0),    // Start point
    new Vector3(1, 1, 0),    // Control point 1
    new Vector3(2, 1, 0),    // Control point 2
    new Vector3(3, 0, 0),    // End point
};

// Create the curve
path.CreateCurve(controlPoints, controlPoints[controlPoints.Count - 1]);

// Access the calculated points
foreach (Vector3 point in path.pathPoints)
{
    // Use the points (e.g., spawn objects along the path)
    Instantiate(prefab, point, Quaternion.identity);
}
```

#### RelativeTransform

Represents a position and rotation offset relative to a parent transform:

```csharp
// Define a relative transform in the inspector
[SerializeField] private RelativeTransform _cameraOffset;

// Position a camera relative to the player
void UpdateCameraPosition()
{
    Vector3 relativePosition = _cameraOffset.GetRelativePosition(playerTransform);
    transform.position = relativePosition;
    
    Quaternion targetRotation = Quaternion.identity;
    if (_cameraOffset.GetLookAtRotation(transform, playerTransform, ref targetRotation))
    {
        transform.rotation = targetRotation;
    }
}
```

#### RangedVector3Float

A Vector3 with components constrained to the range [-1, 1] for the inspector:

```csharp
// Define in inspector
[SerializeField] private RangedVector3Float _directionBias;

// Use in code
Vector3 adjustedDirection = baseDirection + _directionBias.Get();
```

### Enumerations Examples

```csharp
// InteractionType - For UI and object interaction states
InteractionType interaction = InteractionType.Open;
panel.SetActive(interaction == InteractionType.Open);

// SetTransformAxis - For specifying which axis to modify
void LockAxis(SetTransformAxis axis)
{
    // Lock movement on specified axis
}

// Dimension - For specifying 2D or 3D context
void ConfigurePhysics(Dimension dimension)
{
    if (dimension == Dimension.TwoD)
    {
        // Configure 2D physics
    }
    else
    {
        // Configure 3D physics
    }
}

// UnityAxis - For referring to specific axes
void RotateAround(UnityAxis axis)
{
    Vector3 axisVector = axis == UnityAxis.X ? Vector3.right : 
                          axis == UnityAxis.Y ? Vector3.up : Vector3.forward;
    transform.Rotate(axisVector, 45f);
}
```

### Delegate Types

HambaExt provides a comprehensive set of delegate types for various callback patterns:

```csharp
// Simple action delegates
public event OnDoAnything OnCompleted;
public event OnDoAnything<float> OnProgressChanged;
public event OnDoAnything<Vector3, GameObject> OnObjectSpawned;

// Boolean condition delegates
public WhenToDoFunc IsCompleted;
public WhenToDoFunc<GameObject> IsValidTarget;
public WhenToDoFunc<int, string> IsLevelUnlocked;

// Progress reporting delegates
public event OnDoAnythingFunc3 OnLevelProgress;
OnLevelProgress?.Invoke(currentLevel, progressIncrement, totalProgress);
```

### Best Practices

- Use `ConstField<T>` for values that should be configurable in the inspector but constant at runtime
- Prefer `RelativeTransform` over manual position and rotation calculations for cleaner code
- Use the delegate types to standardize callback patterns across your project
- `MarkerObject` is more lightweight than using booleans in dictionaries for state tracking
- When working with Bezier curves, adjust the point count based on the complexity and length of the curve

These data structures serve as building blocks for more complex systems in the HambaExt framework and provide standardized patterns for common Unity development tasks.