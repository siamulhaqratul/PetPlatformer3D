using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "P_Mango", menuName = "Pool/New Pool Data SO", order = 0)]
public class DataForPoolSO : ScriptableObject
{
    // =========================== CONSTANTS ===========================

    private const int MIN_PREPARED_OBJECTS = 2;
    private const int DEFAULT_PREPARED_OBJECTS = 10;

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Tooltip("Pool id assigned by the Pool Data Holder. Do not edit.")]
    [SerializeField]
    private int _idGroup = 0;

    [Tooltip("Prefab asset to pool. Use the context menu to rename it to this SO's name.")]
    [SerializeField]
    private GameObject _prefab = default;

    [Tooltip("Objects created up front when the pool is prewarmed.")]
    [SerializeField, Min(MIN_PREPARED_OBJECTS)]
    private int _numPreparedObjects = DEFAULT_PREPARED_OBJECTS;

    [SerializeField, HideInInspector]
    private bool _isDataPoolNameCorrect = true;

    [Tooltip("Shown as a warning when this SO's name duplicates another pool data SO.")]
    [SerializeField]
    private string _changeTheName = "Change The Name Of Your SO";

    [Tooltip("Pool Data Holder this SO registers itself with. Found automatically.")]
    [SerializeField]
    private PoolDataHolderSO _poolDataHolderSO;

    // =========================== PROPERTIES ==========================

    public int IdGroup => _idGroup;
    public GameObject Prefab => _prefab;
    public int NumPreparedObjects => _numPreparedObjects;
    internal bool IsDataPoolNameCorrect => _isDataPoolNameCorrect;

    // ========================= INITIALIZE METHODS ========================

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (_poolDataHolderSO == null)
        {
            GetPoolHolder();
        }
        Init();

        void GetPoolHolder()
        {
            string[] guids = AssetDatabase.FindAssets("t:PoolDataHolderSO");
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _poolDataHolderSO = AssetDatabase.LoadAssetAtPath<PoolDataHolderSO>(path);
        }
    }

    private void Init()
    {
        if (Application.isPlaying) return;
        AddSOFileToHolder();
    }

    private void OnValidate()
    {
        CheckNumOfPreparedObjects();
        if (_prefab == null || PoolDataHolderSO.Instance == null) return;
        string errorMessage = string.Empty;
        if (CheckIfPrefabIsDuplicateInHolder(_prefab, ref errorMessage) == false)
        {
            Debug.LogWarning($"{name}: {errorMessage}", this);
        }
    }
#endif

    // ======================= PUBLIC METHODS ========================

    internal void SetId(int index)
    {
        _idGroup = index;
        _isDataPoolNameCorrect = true;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this); // mark SO dirty so Unity saves the change
#endif
    }

    // ======================= PRIVATE METHODS =======================

    private void AddSOFileToHolder()
    {
#if UNITY_EDITOR
        if (_poolDataHolderSO.AddDataOrCheckDataToList(this) == false)
        {
            if (_isDataPoolNameCorrect == false) return;
            _isDataPoolNameCorrect = false;
            UnityEditor.EditorUtility.DisplayDialog(
                "Duplicate Name",
                $"The DataPool '{name}' already exists with same name in Pool Data Holder'.\n\nPlease change the name of your SO.",
                "OK"
            );
        }
        else
        {
            _isDataPoolNameCorrect = true;
        }
        UnityEditor.EditorUtility.SetDirty(this); // mark SO dirty so Unity saves the change
#endif
    }

    [ContextMenu("Rename Prefab To SO Name")]
    private void RenamePrefab()
    {
#if UNITY_EDITOR
        string selectedName = name;
        if (_prefab.name.Equals(selectedName) == false)
        {
            RenameAsset(_prefab, selectedName);
        }
#endif
    }

    private bool CheckIfPrefabIsDuplicateInHolder(GameObject gameObject, ref string errorMessage)
    {
        if (PoolDataHolderSO.Instance.CheckIfPrefabHasDuplicateInList(this, out DataForPoolSO dataPoolThatHasPrefab))
        {
            errorMessage = $"Prefab already assigned to {dataPoolThatHasPrefab.name}";
            return false;
        }
        return true;
    }

    private void CheckNumOfPreparedObjects()
    {
        if (_numPreparedObjects < MIN_PREPARED_OBJECTS)
        {
            _numPreparedObjects = MIN_PREPARED_OBJECTS;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this); // mark SO dirty so Unity saves the change
#endif
        }
    }

    private void RenameAsset(UnityEngine.Object objectToRename, string newName)
    {
#if UNITY_EDITOR
        string selectedName = newName;
        if (objectToRename.name.Equals(selectedName) == false)
        {
            string path = AssetDatabase.GetAssetPath(objectToRename);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            objectToRename.name = selectedName;
            AssetDatabase.RenameAsset(path, $"{objectToRename.name}");
        }
#endif
    }
}
