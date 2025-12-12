using Game.Entities.Knife.Views;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Game.Pooling
{
    /// <summary>
    /// Provides pooled knife instances backed by Unity ObjectPool.
    /// </summary>
    public class KnifePoolProvider
    {
        private readonly ObjectPool<KnifeView> _Pool;

        /// <summary>
        /// Initializes a new instance of the knife pool provider.
        /// </summary>
        /// <param name="resolver">Resolver used to inject new knife instances.</param>
        /// <param name="knifePrefab">Prefab used for pool creation.</param>
        /// <param name="lifecycleActions">Optional lifecycle hooks.</param>
        public KnifePoolProvider(IObjectResolver resolver, KnifeView knifePrefab, PoolLifecycleActions<KnifeView> lifecycleActions = null)
        {
            _Pool = new ObjectPool<KnifeView>(
                () => CreateKnife(resolver, knifePrefab),
                knife =>
                {
                    lifecycleActions?.OnGet?.Invoke(knife);
                    knife.ResetKnife();
                },
                knife =>
                {
                    lifecycleActions?.OnRelease?.Invoke(knife);
                    knife.ReturnToPool();
                });
        }

        /// <summary>
        /// Retrieves a knife from the pool.
        /// </summary>
        public KnifeView Get()
        {
            return _Pool.Get();
        }

        /// <summary>
        /// Returns a knife to the pool.
        /// </summary>
        /// <param name="knife">Knife instance to release.</param>
        public void Release(KnifeView knife)
        {
            if (knife == null)
                return;

            _Pool.Release(knife);
        }

        private static KnifeView CreateKnife(IObjectResolver resolver, KnifeView prefab)
        {
            var instance = Object.Instantiate(prefab);
            resolver.Inject(instance);
            return instance;
        }
    }
}
