using UnityEngine;
using UnityEngine.Pool;

namespace Common.Pooling
{
    public class ViewPoolProvider<TView> where TView : Component, IPooledView
    {
        private readonly ObjectPool<TView> _pool;
        private readonly TView _prefab;
        private readonly Transform _parent;

        public ViewPoolProvider(TView prefab, Transform parent, int defaultCapacity = 10, int maxSize = 50)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<TView>(
                CreateView,
                OnGet,
                OnRelease,
                OnDestroyView,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        public TView Get()
        {
            return _pool.Get();
        }

        public void Release(TView view)
        {
            if (view == null)
            {
                return;
            }

            _pool.Release(view);
        }

        private TView CreateView()
        {
            return Object.Instantiate(_prefab, _parent);
        }

        private static void OnGet(TView view)
        {
            if (view == null)
            {
                return;
            }

            view.gameObject.SetActive(true);
            view.OnSpawned();
        }

        private static void OnRelease(TView view)
        {
            if (view == null)
            {
                return;
            }

            view.OnDespawned();
            view.gameObject.SetActive(false);
        }

        private static void OnDestroyView(TView view)
        {
            if (view == null)
            {
                return;
            }

            Object.Destroy(view.gameObject);
        }
    }
}
