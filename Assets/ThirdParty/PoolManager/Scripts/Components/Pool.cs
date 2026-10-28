using System.Collections.Generic;
using System;
using UnityEngine;

namespace ManagerPooling
{
    public class Pool
    {
        // =================== PRIVATE/PROTECTED FIELDS ====================

        private Dictionary<GameObject, ItemPool> _objectsInPool = new Dictionary<GameObject, ItemPool>();
        private Queue<ItemPool> _freeItems = new Queue<ItemPool>();
        private GameObject _prefabObject;
        private Transform _parent;
        private Transform _inactiveCreationParent;
        private Dictionary<GameObject, int> _objectIdMap;
        private int _id;
        private bool _showSpwanCountOnEachPoolParent;
        private string _parentObjectName;
        private bool _doesPrefabHavePoolItemComponent = false;
        private int _prewarmSize;

        // =========================== PROPERTIES ==========================

        public Dictionary<GameObject, ItemPool> ObjectsInPool => _objectsInPool;
        public bool IsPrewarmComplete => _objectsInPool.Count >= _prewarmSize;
        public bool HasObjectsInUse => _freeItems.Count < _objectsInPool.Count;

        // ========================= INITIALIZE METHODS ========================

        public Pool(int id, GameObject prefab, int size, Transform parent, Dictionary<GameObject, int> objectIdMap, bool showSpwanCountOnEachPoolParent)
            : this(id, prefab, size, parent, objectIdMap, showSpwanCountOnEachPoolParent, true)
        {
        }

        public Pool(int id, GameObject prefab, int size, Transform parent, Dictionary<GameObject, int> objectIdMap, bool showSpwanCountOnEachPoolParent, bool prewarmImmediately)
        {
            _prewarmSize = size;
            _id = id;
            _prefabObject = prefab;
            _parent = parent;
            _objectIdMap = objectIdMap;
            _showSpwanCountOnEachPoolParent = showSpwanCountOnEachPoolParent;

            if (_showSpwanCountOnEachPoolParent)
            {
                _parentObjectName = _parent.gameObject.name;
            }
            _doesPrefabHavePoolItemComponent = prefab.GetComponent<IPoolable>() != null;

            GameObject creationRoot = new GameObject("Inactive Creation Root");
            _inactiveCreationParent = creationRoot.transform;
            _inactiveCreationParent.SetParent(_parent, false);
            creationRoot.SetActive(false);

            if (!prewarmImmediately) return;
            while (!IsPrewarmComplete)
            {
                CreateNewObject();
            }
        }

        // ======================= PUBLIC METHODS ========================

        public void PrewarmStep()
        {
            if (!IsPrewarmComplete)
            {
                CreateNewObject();
            }
        }

        public GameObject GetFromPool(bool activate = true)
        {
            if (_freeItems.Count == 0)
            {
                CreateNewObject();
            }
            return _freeItems.Dequeue().GetPoolObject(activate);
        }

        public void BackToPool(GameObject item)
        {
            if (_objectsInPool.TryGetValue(item, out var poolItem))
            {
                if (!poolItem.Free)
                {
                    poolItem.GoBackToPool();
                    _freeItems.Enqueue(poolItem);
                }
            }
        }

        // ======================= PRIVATE METHODS =======================

        private void CreateNewObject()
        {
            GameObject itemObject = UnityEngine.Object.Instantiate(_prefabObject, _inactiveCreationParent);
            var itemPool = new ItemPool(itemObject, _parent,_doesPrefabHavePoolItemComponent);
            _objectsInPool.Add(itemObject, itemPool);
            _freeItems.Enqueue(itemPool);
            _objectIdMap.Add(itemObject, _id);
#if UNITY_EDITOR
            if (_showSpwanCountOnEachPoolParent)
            {
                _parent.gameObject.name = $"({_objectsInPool.Count}) {_parentObjectName} ";
            }
#endif
        }
    }
}
