using UnityEngine;
using UnityEngine.Pool;

namespace Common.MVP
{
    public class ViewPoolProvider<TView, TModel>
        where TView : MonoBehaviour, IView<TModel>
        where TModel : IModel
    {
        private readonly TView _prefab;
        private readonly Transform _root;
        private readonly ObjectPool<TView> _pool;

        public ViewPoolProvider(TView prefab, Transform root, int defaultCapacity = 10, int maxCapacity = 50)
        {
            _prefab = prefab;
            _root = root;

            _pool = new ObjectPool<TView>(
                createFunc: CreateInstance,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroyInstance,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxCapacity
            );
        }

        private TView CreateInstance() => Object.Instantiate(_prefab, _root);

        private void OnGet(TView view) => view.Show();

        private void OnRelease(TView view) => view.Hide();

        private void OnDestroyInstance(TView view) => Object.Destroy(view.gameObject);

        public TView Get() => _pool.Get();

        public void Release(TView view) => _pool.Release(view);
    }
}
