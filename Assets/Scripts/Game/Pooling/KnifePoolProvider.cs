using System.Collections.Generic;
using Game.Entities.Knife.Views;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Game.Pooling
{
    /// <summary>
    /// Provides pooled knife instances backed by Unity ObjectPool.
    /// Maintains its own list of pooled objects for efficient management.
    /// </summary>
    public class KnifePoolProvider
    {
        private readonly ObjectPool<KnifeView> _Pool;
        private readonly Transform _poolParent;
        private readonly List<KnifeView> _pooledObjects = new();

        /// <summary>
        /// Initializes a new instance of the knife pool provider.
        /// </summary>
        /// <param name="resolver">Resolver used to inject new knife instances.</param>
        /// <param name="poolSettings">Pool settings containing knife prefab and configuration.</param>
        public KnifePoolProvider(IObjectResolver resolver, PoolSettings poolSettings)
        {
            var knifePrefab = poolSettings.KnifePrefab;
            if (knifePrefab == null)
            {
                throw new System.ArgumentException("KnifePrefab cannot be null in pool settings", nameof(poolSettings));
            }

            // Create a dedicated parent for pooled objects
            var poolParent = new GameObject($"KnifePool_{knifePrefab.name}").transform;
            Object.DontDestroyOnLoad(poolParent.gameObject);
            _poolParent = poolParent;
            _Pool = new ObjectPool<KnifeView>(
                createFunc: () => CreateKnife(resolver, knifePrefab),
                actionOnGet: OnKnifeRetrieved,
                actionOnRelease: OnKnifeReleased,
                defaultCapacity: poolSettings.KnifePoolWarmupCount,
                maxSize: poolSettings.KnifePoolMaxSize);

            // Pre-warm the pool by creating initial objects
            var warmupObjects = new KnifeView[poolSettings.KnifePoolWarmupCount];
            for (int i = 0; i < poolSettings.KnifePoolWarmupCount; i++)
            {
                warmupObjects[i] = _Pool.Get();
            }
            for (int i = 0; i < poolSettings.KnifePoolWarmupCount; i++)
            {
                _Pool.Release(warmupObjects[i]);
            }
        }

        private static void OnKnifeRetrieved(KnifeView knife)
        {
            // Knife is already reset in Get() method
        }

        private void OnKnifeReleased(KnifeView knife)
        {
            ResetKnifeForPool(knife);
            knife.gameObject.SetActive(false);
            knife.transform.SetParent(_poolParent, false);
            _pooledObjects.Add(knife);
        }

        private static void ResetKnifeForPool(KnifeView knife)
        {
            knife.ResetKnife();
            knife.SetColliderEnabled(false);
        }

        /// <summary>
        /// Retrieves a knife from the pool.
        /// </summary>
        public KnifeView Get()
        {
            var knife = _Pool.Get();
            knife.transform.SetParent(null, false);
            knife.gameObject.SetActive(true);
            knife.ResetKnife();
            return knife;
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

        /// <summary>
        /// Enumerates all currently pooled knife objects.
        /// </summary>
        public IEnumerable<KnifeView> EnumeratePooledObjects()
        {
            return _pooledObjects;
        }

        private static KnifeView CreateKnife(IObjectResolver resolver, KnifeView prefab)
        {
            var instance = Object.Instantiate(prefab);
            resolver.Inject(instance);
            return instance;
        }
    }
}
