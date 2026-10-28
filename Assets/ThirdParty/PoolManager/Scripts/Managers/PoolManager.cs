using UnityEngine;
using System.Collections.Generic;
using UnityExt;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

namespace ManagerPooling
{
    public enum SceneType
    {
        OnceLoad, DontDestroy
    }

    public class PoolManager : MonoBehaviour
    {
        // =========================== CONSTANTS ===========================
        private const string DUPLICATE_NAME_TITLE = "Duplicate Name";
        private const float PREWARM_FRAME_BUDGET_MS = 4f;
        private const double MILLISECONDS_PER_SECOND = 1000.0;

        // ======================= STATIC FIELDS ==========================
        private static PoolManager _instance = null;

        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [Tooltip("Holder asset that lists every pool data SO in the project.")]
        [SerializeField] private PoolDataHolderSO _poolDataHolderSO;
        [Tooltip("OnceLoad lives in one scene; DontDestroy persists across scene loads.")]
        [SerializeField] private SceneType _typeLoad;

        [Tooltip("OnceLoad only: initialize every pool instead of the scene pool list.")]
        [SerializeField]
        private bool _shouldInitializeAllPools = true;

        [Tooltip("Pools for this scene (sorted alphabetically). Used when not initializing all pools.")]
        [SerializeField]
        private List<DataForPoolSO> _scenePoolList;


        [Tooltip("Read-only view of all pools from the holder (sorted alphabetically).")]
        [SerializeField]
        private List<DataForPoolSO> _allPoolListShow;

        /// <summary>
        /// If enabled, displays the current spawn count on each pool's parent GameObject in the hierarchy for debugging purposes.
        /// </summary>
        [Header("Show Spwan Count On Each PoolParent")]
        [Tooltip("Debug: show the spawn count on each pool parent (check children of this transform).")]
        [SerializeField] private bool _showSpwanCount;

        // =================== PRIVATE/PROTECTED FIELDS ====================
        private Dictionary<GameObject, int> _objectIdMap = new Dictionary<GameObject, int>();
        private Dictionary<int, Pool> _allPools = new Dictionary<int, Pool>();
        private Transform _transform;

        public SceneType TypeLoad => _typeLoad;

        // =========================== PROPERTIES ==========================
        public static PoolManager Instance => _instance;

        // ======================= UNITY CALLBACKS =========================
        private void Awake()
        {

            _showSpwanCount = false;
            SetTypeManager();
            _transform = transform;
            InitializeList();
        }
        private void SetTypeManager()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            switch (_typeLoad)
            {
                case SceneType.OnceLoad: { break; }
                case SceneType.DontDestroy:
                    {
                        DontDestroyOnLoad(this);
                        break;
                    }
            }
        }

        private void Start()
        {
            _poolDataHolderSO.CheckIfAnyPrefabNullOrDuplicate();
            CreateObjectPool();

            void CreateObjectPool()
            {
                List<DataForPoolSO> listToSpawnObjects = new();
                if (ShouldShowSceneList())
                {
                    listToSpawnObjects.AddRange(_scenePoolList);
                }
                else
                {
                    listToSpawnObjects.AddRange(_poolDataHolderSO.DataForPools);
                }
                listToSpawnObjects.Sort((a, b) => string.Compare(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty, System.StringComparison.OrdinalIgnoreCase));

                var poolsToPrewarm = new List<Pool>(listToSpawnObjects.Count);
                for (int i = 0; i < listToSpawnObjects.Count; i++)
                {
                    DataForPoolSO data = listToSpawnObjects[i];

                    poolsToPrewarm.Add(PoolInstaller(data, false));
                }
                // Spawning creates objects on demand, so a pool that is still filling stays usable.
                StartCoroutine(PrewarmPools(poolsToPrewarm));
            }
        }

        private System.Collections.IEnumerator PrewarmPools(List<Pool> pools)
        {
            for (int i = 0; i < pools.Count; i++)
            {
                Pool pool = pools[i];
                while (!pool.IsPrewarmComplete)
                {
                    long frameStart = System.Diagnostics.Stopwatch.GetTimestamp();
                    while (!pool.IsPrewarmComplete && GetElapsedMs(frameStart) < PREWARM_FRAME_BUDGET_MS)
                    {
                        pool.PrewarmStep();
                    }
                    yield return null;
                }
            }
        }

        private static double GetElapsedMs(long startTimestamp)
        {
            long elapsedTicks = System.Diagnostics.Stopwatch.GetTimestamp() - startTimestamp;
            return elapsedTicks * MILLISECONDS_PER_SECOND / System.Diagnostics.Stopwatch.Frequency;
        }

        // ========================= PUBLIC METHODS ========================
        private Pool PoolInstaller(DataForPoolSO dataForPool, bool prewarmImmediately = true)
        {
            int id = dataForPool.IdGroup;
            if (_allPools.TryGetValue(id, out Pool existingPool))
            {
                return existingPool;
            }

            GameObject prefab = dataForPool.Prefab;
            int size = dataForPool.NumPreparedObjects;

            var rootTransform = new GameObject();
            rootTransform.name = dataForPool.name + " Pool";
            rootTransform.transform.SetParent(_transform);

            var pool = new Pool(id, prefab, size, rootTransform.transform, _objectIdMap, _showSpwanCount, prewarmImmediately);
            _allPools.Add(id, pool);

            return pool;
        }

        /// <summary>
        /// Spawns a <see cref="GameObject"/> from the pool based on the provided <paramref name="dataForPool"/>.
        /// If the pool for the specified data does not exist, it attempts to initialize it and retry spawning.
        /// Handles error reporting and validation for the provided data.
        /// </summary>
        /// <param name="dataForPool">
        /// The <see cref="DataForPoolSO"/> containing information about the pool and prefab to spawn.
        /// </param>
        /// <returns>
        /// The spawned <see cref="GameObject"/> if successful; otherwise, <c>null</c>.
        /// </returns>
        public GameObject SpawnObject(DataForPoolSO dataForPool, bool activate = true)
        {
            if (dataForPool == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Datapool SO is null");
#endif
                return null;
            }
            if (_allPools.TryGetValue(dataForPool.IdGroup, out var pool))
            {
                var objectFromPool = pool.GetFromPool(activate);
                return objectFromPool;
            }
            else
            {

                if (dataForPool.Prefab != null && dataForPool.IsDataPoolNameCorrect == true)
                {
                    Pool tempPool = PoolInstaller(dataForPool);
#if UNITY_EDITOR
                    if (ShouldShowSceneList())
                    {
                        _scenePoolList.Add(dataForPool);
                    }

                    UnityEditor.EditorUtility.DisplayDialog(
                      "Item Not Initialized",
                      $"{dataForPool} not initialized. \nIt was not added in Scene Pool list. \nAdd it kindly",
                      "OK"
                              );
#endif
                    return tempPool.GetFromPool(activate);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError("Not Proper item");
                    UnityEditor.EditorUtility.DisplayDialog(
                        "Item Not Proper",
                        $"DataPoolSO {dataForPool} not correct",
                        "OK"
                    );
#endif
                    return null;
                }

            }
        }


        /// <summary>
        /// Returns a <see cref="GameObject"/> to its associated pool.
        /// If the object is not tracked by any pool, it is destroyed and an error is reported.
        /// </summary>
        /// <param name="item">The <see cref="GameObject"/> to return to the pool.</param>
        public void BackToPool(GameObject item)
        {
            if (item == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Gameobject item is null. You are trying to back it to pool");
#endif
                return;
            }
            if (_objectIdMap.TryGetValue(item, out int key))
            {
                _allPools[key].BackToPool(item);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("Object Not Initialized");
                UnityEditor.EditorUtility.DisplayDialog(
                    "Item Not Initialized",
                    $"Gameobject {item.name} not initialized in Pool",
                    "OK"
                );
#endif
                Destroy(item);
            }
        }


        public bool HasObjectsInUse()
        {
            foreach (Pool pool in _allPools.Values)
            {
                if (pool.HasObjectsInUse) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns all currently tracked pooled objects back to their respective pools.
        /// </summary>
        public void LoadAllPoolObjectsBackToPool()
        {
            // Iterate over all tracked objects and return them to their pool if possible
            var objects = _objectIdMap.Keys.ToList();
            foreach (var obj in objects)
            {
                if (obj == null || obj.Equals(null))
                    continue;

                if (_objectIdMap.TryGetValue(obj, out int poolId))
                {
                    _allPools[poolId].BackToPool(obj);
                }
            }
        }


        // ======================================================================
        // ======================================================================
        // ======================================================================
        // ========================= EDITOR =====================================
        // ======================================================================
        // ======================================================================
        // ======================================================================


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying || _poolDataHolderSO == null) return;
            InitializeList();
        }
#endif

        private void InitializeList()
        {
            _allPoolListShow.Clear();
            //_scenePoolList.RemoveAll(item => item == null || item.Equals(null));
            _scenePoolList.ExRemoveNullsOnIt();
            _poolDataHolderSO.UpdateIDs();
            _allPoolListShow.AddRange(_poolDataHolderSO.DataForPools);
            _allPoolListShow.Sort((a, b) => string.Compare(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty, System.StringComparison.OrdinalIgnoreCase));
        }

        [ContextMenu("Check Scene Pool List")]
        private void CheckSceneList()
        {
            _scenePoolList.ExRemoveNullsOnIt();
            var wrongItems = new List<string>();

#if UNITY_EDITOR
            var nameSet = new HashSet<DataForPoolSO>();
            var duplicates = new List<string>();

            foreach (var data in _scenePoolList)
            {
                if (data == null) continue;
                if (!nameSet.Add(data))
                {
                    duplicates.Add(data.name);
                }
            }

            if (duplicates.Count > 0)
            {
                string duplicateNames = string.Join(", ", duplicates.Distinct());
                UnityEditor.EditorUtility.DisplayDialog(
                    DUPLICATE_NAME_TITLE,
                    $"DataPool have duplicates in the Scene Pool List:\n\n{duplicateNames}\n\nThey have been removed",
                    "OK"
                );
            }
#endif

            _scenePoolList.ExRemoveDuplicates();
            _scenePoolList.Sort((a, b) => string.Compare(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty, System.StringComparison.OrdinalIgnoreCase));

            foreach (var data in _scenePoolList)
            {
                if (data == null) continue;
                if (data.IsDataPoolNameCorrect == false)
                {
                    wrongItems.Add(data.name);
                }
            }

            if (wrongItems.Count > 0)
            {
                string wrongNames = string.Join(", ", wrongItems.Distinct());
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog(
                    DUPLICATE_NAME_TITLE,
                    $"DataPool have wrongly named items in the Scene Pool List, as they are duplicate names of previously existing SO:\n\n{wrongNames}\n\nPlease change the name of your SO(s).",
                    "OK"
                );
#endif
                _scenePoolList.ExForEachSafe(x => { if (x.IsDataPoolNameCorrect == false) { _scenePoolList.Remove(x); } });
            }
        }

        //[ShowIf(nameof(ShouldShowSceneList))]
        [ContextMenu("Add Pools From Scene In List")]
        private void AddPoolsFromSceneInList()
        {
#if UNITY_EDITOR
            var found = new HashSet<DataForPoolSO>();
            var monoBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var monoBehaviour in monoBehaviours)
            {
                if (monoBehaviour == null) continue;
                if (monoBehaviour == this) continue;

                var serializedObject = new UnityEditor.SerializedObject(monoBehaviour);
                var property = serializedObject.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                    {
                        var obj = property.objectReferenceValue as DataForPoolSO;
                        if (obj != null)
                        {
                            found.Add(obj);
                        }
                    }
                }
            }

            if (_scenePoolList == null)
                _scenePoolList = new List<DataForPoolSO>();
            else
                _scenePoolList.Clear();

            _scenePoolList.AddRange(found);
            _scenePoolList.Sort((a, b) => string.Compare(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty, System.StringComparison.OrdinalIgnoreCase));
            UnityEditor.EditorUtility.SetDirty(this);
#else
            Debug.LogWarning("AddPoolsFromSceneInList is only available in the Unity Editor.");
#endif
        }

        private bool ShouldShowSceneList()
        {
            return _typeLoad == (SceneType.OnceLoad) && _shouldInitializeAllPools == false;
        }



        private void OnEnable()
        {
            if (_typeLoad == SceneType.DontDestroy)
            {
                SceneManager.sceneUnloaded += OnSceneUnloaded;
                //SceneManager.sceneLoaded += OnSceneLoaded;
            }

        }



        private void OnDisable()
        {
            if (_typeLoad == SceneType.DontDestroy)
            {
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                //SceneManager.sceneLoaded -= OnSceneLoaded;
            }

        }

        private void OnSceneUnloaded(Scene scene)
        {
            // Find all objects tracked by the pool that were in this scene
            var objectsToRemove = _objectIdMap.Keys
                .Where(obj => obj == null || obj.Equals(null) || obj.scene == scene)
                .ToList();

            foreach (var obj in objectsToRemove)
            {
                // Make sure pool knows about the cleanup
                if (obj != null && _objectIdMap.TryGetValue(obj, out var poolId))
                {
                    if (_allPools.TryGetValue(poolId, out var pool))
                    {
                        pool.ObjectsInPool.Remove(obj);
                    }
                }

                // Remove from master map
                _objectIdMap.Remove(obj);
            }
        }

        private bool IsOnRuntime()
        {
            return Application.isPlaying;
        }
    }

}








