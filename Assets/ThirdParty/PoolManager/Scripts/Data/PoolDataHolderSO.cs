using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityExt;
using UnityExt.Tons;

//[CreateAssetMenu(fileName = "PoolDataHolder", menuName = "Pool/PoolDataHolder")]
public class PoolDataHolderSO : SingletonSO<PoolDataHolderSO>
{
    [Tooltip("Every pool data SO in the project. Filled automatically.")]
    [SerializeField] private List<DataForPoolSO> _dataForPoolList;
    public IReadOnlyList<DataForPoolSO> DataForPools => _dataForPoolList;

    [Tooltip("Read-only view of all pools, sorted alphabetically. Use the context menu to refresh.")]
    [SerializeField] private List<DataForPoolSO> _allPoolListShow;

    [SerializeField, HideInInspector] private List<DataForPoolSO> _wrongDataPool = new();
    public List<DataForPoolSO> WrongDataPool => _wrongDataPool;
    public bool AddDataOrCheckDataToList(DataForPoolSO dataForPool, bool callByWrongIteration = false)
    {
        if (dataForPool == null || dataForPool.Equals(null))
            return false;

        // Check for duplicate by asset name
        _dataForPoolList.ExRemoveNullsOnIt();
#if UNITY_EDITOR
        //      UnityEditor.EditorUtility.SetDirty(this); // mark SO dirty so Unity saves the change
#endif

        // No duplicates, add to listI
        if (_dataForPoolList.Contains(dataForPool))
        {
            foreach (var data in _dataForPoolList)
            {
                if (data.Equals(dataForPool))
                    continue;

                if (data.name == dataForPool.name)
                {
                    _dataForPoolList.Remove(dataForPool);

                    if (_wrongDataPool.Contains(dataForPool) == false)
                    {
                         _wrongDataPool.Add(dataForPool);
                    }
                   

                    if (callByWrongIteration == false)
                    {
                        UpdateIDs();
                    }

                    return false;
                }
            }
        }
        else
        {
            foreach (var data in _dataForPoolList)
            {
                if (data.name == dataForPool.name)
                {
                     if (_wrongDataPool.Contains(dataForPool) == false)
                    {
                         _wrongDataPool.Add(dataForPool);
                    }
                    return false;
                }
            }

            _dataForPoolList.Add(dataForPool);
        }

        if (callByWrongIteration == false)
        {
            UpdateIDs();
        }

        return true;
    }


    public void UpdateIDs()
    {
        IterateThroughWrongList();
        _dataForPoolList.ExRemoveNullsOnIt();
        //_dataForPoolList.Sort((a, b) => string.Compare(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty, System.StringComparison.OrdinalIgnoreCase));
        _dataForPoolList.ExForEachSafe((data, index) => { data.SetId(index); });

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this); // mark SO dirty so Unity saves the change
#endif
    }

    private void IterateThroughWrongList()
    {
        // Create a temporary list to avoid modifying the collection during iteration
        _wrongDataPool.ExRemoveNullsOnIt();
        var toRemove = new List<DataForPoolSO>();

        foreach (var data in _wrongDataPool)
        {
            if (AddDataOrCheckDataToList(data, true))
            {
                toRemove.Add(data);
            }
        }

        foreach (var data in toRemove)
        {
            _wrongDataPool.Remove(data);
        }
    }

    [ContextMenu("Refresh Pool List")]
    private void InitializeList()
    {
        UpdateIDs();
        _allPoolListShow = _dataForPoolList.ExShallowCopy();
        _allPoolListShow.Sort((a, b) => string.Compare(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty, System.StringComparison.OrdinalIgnoreCase));
    }


    internal void CheckIfAnyPrefabNullOrDuplicate()
{
#if UNITY_EDITOR
    if (_dataForPoolList == null || _dataForPoolList.Count == 0)
    {
        // UnityEditor.EditorUtility.DisplayDialog("Pool Data Validation", "List is empty.", "OK");
        return;
    }

    var nullPrefabItems = new List<DataForPoolSO>();
    var byPrefab = new Dictionary<GameObject, List<DataForPoolSO>>();

    for (int i = 0; i < _dataForPoolList.Count; i++)
    {
        var data = _dataForPoolList[i];
        var prefab = data.Prefab;
        if (prefab == null)
        {
            nullPrefabItems.Add(data);
            continue;
        }

        if (!byPrefab.TryGetValue(prefab, out var owners))
        {
            owners = new List<DataForPoolSO>();
            byPrefab[prefab] = owners;
        }
        owners.Add(data);
    }

    // 1) Report all SOs with null prefab
    if (nullPrefabItems.Count > 0)
    {
        var msg = BuildDialogMessage(
            header: $"Found {nullPrefabItems.Count} item(s) with NULL Prefab:",
            lines: nullPrefabItems.ConvertAll(x => $"- SO: {SafeName(x)}"),
            maxLines: 30
        );
        UnityEditor.EditorUtility.DisplayDialog("Pool Data: Null Prefabs", msg, "OK");
    }

    // 2) Report duplicates by prefab
    var duplicateGroups = new List<string>();
    foreach (var keyValue in byPrefab)
    {
        var list = keyValue.Value;
        if (list.Count <= 1) continue;
        string dataName = list[0].name;
        list.RemoveAt(0);
        string owners = string.Join(
            "\n",
            list.ConvertAll(x => $"    - {SafeName(x)}")
        );
        duplicateGroups.Add($"Prefab '{SafeName(keyValue.Key)}' is referenced by {dataName} other {list.Count} DataForPoolSO has it:\n{owners}");
    }

    if (duplicateGroups.Count > 0)
    {
        var msg = BuildDialogMessage(
            header: $"Found {duplicateGroups.Count} duplicate prefab group(s):",
            lines: duplicateGroups,
            maxLines: 30
        );
        UnityEditor.EditorUtility.DisplayDialog("Pool Data: Duplicate Prefab References", msg, "OK");
    }

    // 3) Report wrong data pool entries
    if (_wrongDataPool.Count > 0)
    {
        var wrongList = new List<string>();
        foreach (var wrong in _wrongDataPool)
        {
            wrongList.Add($"- SO: {SafeName(wrong)}");
        }
        var msg = BuildDialogMessage(
            header: $"Found {_wrongDataPool.Count} item(s) with duplicate or invalid names:",
            lines: wrongList,
            maxLines: 30
        );
        UnityEditor.EditorUtility.DisplayDialog("Pool Data: Wrong DataForPoolSO Names", msg, "OK");
    }

    if (nullPrefabItems.Count == 0) //&& duplicateGroups.Count == 0 && _wrongDataPool.Count == 0)
    {
        // UnityEditor.EditorUtility.DisplayDialog("Pool Data Validation", "All good! No null prefabs, duplicate prefab references, or wrong-named DataForPoolSO found.", "OK");
    }
    else
    {
        EditorApplication.isPlaying = false;
    }
#else
   // Debug.LogWarning("CheckIfAnyPrefabNullOrDuplicate is only available in the Unity Editor.");
#endif
}

#if UNITY_EDITOR
    private static string BuildDialogMessage(string header, IEnumerable<string> lines, int maxLines)
    {
        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(header)) sb.AppendLine(header);
        int count = 0;
        foreach (var line in lines)
        {
            if (count++ >= maxLines) break;
            sb.AppendLine(line);
        }
        // If there are more lines than shown, indicate truncation
        if (lines is ICollection<string> col && col.Count > maxLines)
        {
            sb.AppendLine($"... and {col.Count - maxLines} more");
        }
        return sb.ToString();
    }

    private string SafeName(UnityEngine.Object obj) => obj == null ? "<null>" : obj.name;
#endif

    public bool CheckIfPrefabHasDuplicateInList(DataForPoolSO dataForPool, out DataForPoolSO dataForPoolThatHasPrefab)
    {
        var prefab = dataForPool.Prefab;
        dataForPoolThatHasPrefab = null;
        if (prefab == null) return false;

        int count = 0;
        for (int i = 0; i < _dataForPoolList.Count; i++)
        {
            var data = _dataForPoolList[i];
            if (data.Prefab == prefab)
            {
                if (count == 0)
                {
                    dataForPoolThatHasPrefab = data;
                    if (dataForPool == data)
                    {
                        return false;
                    }

                }
                count++;
            }
        }

        if (count > 1)
        {
            return true;
        }
        return false;
    }


}


