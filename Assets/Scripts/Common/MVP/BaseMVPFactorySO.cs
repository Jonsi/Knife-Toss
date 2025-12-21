using System;
using Common.MVP.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Common.MVP
{
    public abstract class BaseMVPFactorySO<TPresenter, TView, TModel> : ScriptableObject, IFactory<TPresenter, TView, TModel>
        where TPresenter : BasePresenter<TView, TModel>
        where TView : MonoBehaviour, IView<TModel>
        where TModel : IModel
    {
        [SerializeField] protected TView prefab;
        [SerializeField] private int _DefaultPoolSize = 10;
        [SerializeField] private int _MaxPoolSize = 50;

        private ObjectPool<TView> _Pool;
        private Transform _PoolRoot;

        public TPresenter Create(TView prefabOverride, TModel model, Transform parent)
        {
            var prefabToUse = prefabOverride != null ? prefabOverride : prefab;
            if (_Pool == null || prefabToUse != prefab)
                InitializePool(parent, prefabToUse);

            TView viewInstance = _Pool.Get();
            var presenter = CreatePresenter(viewInstance, model, _Pool.Release);
            return presenter;
        }

        protected abstract TPresenter CreatePresenter(TView view, TModel model, Action<TView> release);

        private void InitializePool(Transform parent, TView prefabToUse)
        {
            _PoolRoot = parent;
            prefab = prefabToUse;
            _Pool = new ObjectPool<TView>(
                createFunc: () => Instantiate(prefab, _PoolRoot),
                actionOnGet: v => v.Show().Forget(),
                actionOnRelease: v => v.Hide().Forget(),
                actionOnDestroy: v => Destroy(v.gameObject),
                defaultCapacity: _DefaultPoolSize,
                maxSize: _MaxPoolSize
            );
        }
    }
}
