using DG.Tweening;
using UnityEngine;

namespace ManagerPooling
{
    public class ItemPool
    {
        private GameObject _itemObject;
        private bool _free = true;
        private Transform _cachedTransform;
        private Vector3 _scale;
        private Transform _parent;
        public bool Free => _free;
        private bool _doesPrefabHavePoolItemComponent = false;
        private IPoolable _poolable;

        public ItemPool(GameObject gameObject, Transform parent, bool doesPrefabHavePoolItemComponent)
        {
            _itemObject = gameObject;
            _cachedTransform = _itemObject.transform;
            _parent = parent;
            _doesPrefabHavePoolItemComponent = doesPrefabHavePoolItemComponent;

            if (doesPrefabHavePoolItemComponent)
            {
                _poolable = _itemObject.GetComponent<IPoolable>();
            }

            // Pool creates instances beneath an inactive staging parent. Disable
            // the item before moving it to the live pool root so OnEnable does
            // not run during prewarming or dynamic pool expansion.
            _itemObject.SetActive(false);
            _cachedTransform.SetParent(_parent);
            _scale = _cachedTransform.localScale;
            _free = true;

        }

        public void GoBackToPool()
        {
            _itemObject.SetActive(false);
            _cachedTransform.DOKill();
            _cachedTransform.SetParent(_parent);
            _cachedTransform.localPosition = Vector3.zero;
            _cachedTransform.localRotation = Quaternion.identity;
            _cachedTransform.localScale = _scale;
            _free = true;
            
            if (_doesPrefabHavePoolItemComponent)
            {
                _poolable.OnReturnToPool();
            }
        }

        public GameObject GetPoolObject(bool activate = true)
        {
            _free = false;
            if (activate)
            {
                _itemObject.SetActive(true);
            }
            
            if (_doesPrefabHavePoolItemComponent)
            {
                _poolable.OnSpawnFromPool();
            }

            return _itemObject;
        }

    }
}
