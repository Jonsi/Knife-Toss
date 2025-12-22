using System;
using System.Collections.Generic;
using Game.Configs.Prefabs;
using Game.Entities.Knife.Views;
using Game.PowerUps.Configs;
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
        private readonly ObjectPool<KnifeView> _DefaultPool;
        private readonly ObjectPool<KnifeView> _SuperKnifePool;
        private readonly IObjectResolver _Resolver;
        private readonly Dictionary<KnifeView, ObjectPool<KnifeView>> _KnifePoolLookup = new();
        private readonly Dictionary<ObjectPool<KnifeView>, Transform> _PoolParents = new();
        private readonly List<KnifeView> _pooledObjects = new();
        private readonly PrefabsConfiguration _PrefabsConfiguration;

        /// <summary>
        /// Initializes a new instance of the knife pool provider.
        /// </summary>
        /// <param name="resolver">Resolver used to inject new knife instances.</param>
        /// <param name="prefabsConfiguration">Prefab configuration containing knife entries and pooling settings.</param>
        public KnifePoolProvider(IObjectResolver resolver, PrefabsConfiguration prefabsConfiguration)
        {
            _Resolver = resolver;
            _PrefabsConfiguration = prefabsConfiguration;
            if (prefabsConfiguration == null)
            {
                throw new ArgumentNullException(nameof(prefabsConfiguration), "Prefabs configuration is required for knife pooling");
            }

            var defaultEntry = _PrefabsConfiguration.GetDefaultKnifeEntry();
            if (defaultEntry == null)
                throw new ArgumentException("PrefabsConfiguration must include a default knife entry.", nameof(prefabsConfiguration));

            _DefaultPool = CreatePool(defaultEntry);
            var superEntry = _PrefabsConfiguration.GetSuperKnifeEntry();
            _SuperKnifePool = superEntry != null && superEntry != defaultEntry ? CreatePool(superEntry) : _DefaultPool;
        }

        private static void OnKnifeRetrieved(KnifeView knife)
        {
            // Knife is already reset in Get() method
        }

        private void OnKnifeReleased(KnifeView knife)
        {
            ResetKnifeForPool(knife);
            knife.gameObject.SetActive(false);
            if (_KnifePoolLookup.TryGetValue(knife, out var pool) &&
                _PoolParents.TryGetValue(pool, out var parent))
            {
                knife.transform.SetParent(parent, false);
            }

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
        public KnifeView Get(PowerUpConfigBase config = null)
        {
            var pool = SelectPool(config);
            var knife = pool.Get();
            _KnifePoolLookup[knife] = pool;
            _pooledObjects.Remove(knife);
            knife.transform.SetParent(null, false);
            knife.gameObject.SetActive(true);
            knife.ResetKnife();
            knife.SetSourcePowerUpConfig(config);
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

            if (!_KnifePoolLookup.TryGetValue(knife, out var pool))
                pool = _DefaultPool;

            pool.Release(knife);
        }

        /// <summary>
        /// Enumerates all currently pooled knife objects.
        /// </summary>
        public IEnumerable<KnifeView> EnumeratePooledObjects()
        {
            return _pooledObjects;
        }

        private ObjectPool<KnifeView> CreatePool(FactoryPrefabEntry<KnifeView> entry)
        {
            var prefab = entry?.Prefab;
            if (prefab == null)
                throw new ArgumentException("Knife prefab entry must reference a prefab with a KnifeView component.");

            ObjectPool<KnifeView> pool = null;
            pool = new ObjectPool<KnifeView>(
                createFunc: () => CreateKnife(prefab, () => pool),
                actionOnGet: OnKnifeRetrieved,
                actionOnRelease: OnKnifeReleased,
                defaultCapacity: entry.WarmupCount,
                maxSize: entry.MaxSize);

            var parent = GetOrCreatePoolParent(prefab);
            _PoolParents[pool] = parent;
            WarmupPool(pool, entry.WarmupCount);
            return pool;
        }

        private void WarmupPool(ObjectPool<KnifeView> pool, int warmupCount)
        {
            var warmupObjects = new KnifeView[warmupCount];
            for (int i = 0; i < warmupCount; i++)
            {
                warmupObjects[i] = pool.Get();
            }
            for (int i = 0; i < warmupCount; i++)
            {
                pool.Release(warmupObjects[i]);
            }
        }

        private ObjectPool<KnifeView> SelectPool(PowerUpConfigBase config)
        {
            if (config is SuperKnifeConfig)
                return _SuperKnifePool ?? _DefaultPool;

            return _DefaultPool;
        }

        private KnifeView CreateKnife(KnifeView prefab, System.Func<ObjectPool<KnifeView>> poolGetter)
        {
            var instance = UnityEngine.Object.Instantiate(prefab);
            _Resolver.Inject(instance);
            instance.AssignPoolProvider(this);
            var pool = poolGetter?.Invoke();
            if (pool != null)
            {
                _KnifePoolLookup[instance] = pool;
            }
            return instance;
        }

        private Transform GetOrCreatePoolParent(KnifeView prefab)
        {
            string name = prefab != null ? prefab.name : "Knife";
            var parent = new GameObject($"KnifePool_{name}").transform;
            UnityEngine.Object.DontDestroyOnLoad(parent.gameObject);
            return parent;
        }
    }
}
