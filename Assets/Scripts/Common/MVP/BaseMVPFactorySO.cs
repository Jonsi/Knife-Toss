using System;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Common.MVP
{
    public abstract class BaseMVPFactorySO<TPresenter, TView, TModel> : ScriptableObject
        where TPresenter : IPresenter<TView, TModel>
        where TView : MonoBehaviour, IView<TModel>
        where TModel : IModel
    {
        [SerializeField] protected TView prefab;
        [SerializeField] private int defaultPoolSize = 10;
        [SerializeField] private int maxPoolSize = 50;

        private ObjectPool<TView> _pool;
        private Transform _poolRoot;

        public virtual TPresenter Create(TModel model, Transform parent, IObjectResolver container)
        {
            if (_pool == null)
                InitializePool(parent);

            TView viewInstance = _pool.Get();

            return container.Instantiate<TPresenter>(new object[]
            {
                viewInstance,
                model,
                (Action<TView>)_pool.Release
            });
        }

        private void InitializePool(Transform parent)
        {
            _poolRoot = parent;
            _pool = new ObjectPool<TView>(
                createFunc: () => Instantiate(prefab, _poolRoot),
                actionOnGet: v => v.Show(),
                actionOnRelease: v => v.Hide(),
                actionOnDestroy: v => Destroy(v.gameObject),
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize
            );
        }
    }
}
