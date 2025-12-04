using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using KnifeToss.Core.Abstractions.Services;
using KnifeToss.Infrastructure.Pooling;
using System.Linq;

namespace KnifeToss.Infrastructure.VFX
{
    /// <summary>
    /// Pooled VFX service that uses ParticlePool to reduce GC allocations.
    /// Manages multiple pools, one per particle prefab.
    /// </summary>
    public sealed class PooledVFXService : IVFXService, System.IDisposable
    {
        private readonly Dictionary<ParticleSystem, ParticlePool> _pools = new Dictionary<ParticleSystem, ParticlePool>();
        private readonly Dictionary<ParticleSystem, ParticlePool> _systemToPool = new Dictionary<ParticleSystem, ParticlePool>();
        private readonly Transform _poolRoot;
        private readonly List<ParticleSystem> _activeSystems = new List<ParticleSystem>();
        private readonly int _defaultPoolSize;

        /// <summary>
        /// Creates a new pooled VFX service.
        /// </summary>
        public PooledVFXService(Transform poolRoot, int defaultPoolSize = 5)
        {
            if (poolRoot == null)
            {
                GameObject rootObject = new GameObject("VFXPoolRoot");
                Object.DontDestroyOnLoad(rootObject);
                _poolRoot = rootObject.transform;
            }
            else
            {
                _poolRoot = poolRoot;
            }

            _defaultPoolSize = defaultPoolSize;
        }

        public VFXHandle Spawn(ParticleSystem prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                return new VFXHandle(null);
            }

            ParticlePool pool = GetOrCreatePool(prefab);
            ParticleSystem system = pool.Get();

            system.transform.position = position;
            system.transform.rotation = rotation;
            system.Play();

            _activeSystems.Add(system);
            _systemToPool[system] = pool; // Track which pool owns this system
            return new VFXHandle(system);
        }

        public async UniTask<VFXHandle> SpawnAsync(ParticleSystem prefab, Vector3 position, Quaternion rotation)
        {
            var handle = Spawn(prefab, position, rotation);

            if (!handle.IsValid)
            {
                return handle;
            }

            // Wait for particle system to finish
            var main = handle.System.main;
            float duration = main.duration;
            float lifetime = main.startLifetime.constantMax;

            await UniTask.Delay((int)((duration + lifetime) * 1000));

            // Auto-release when done
            Release(handle);
            return handle;
        }

        public void Release(VFXHandle handle)
        {
            if (!handle.IsValid || handle.System == null)
            {
                return;
            }

            _activeSystems.Remove(handle.System);

            // Find the pool that owns this system
            if (_systemToPool.TryGetValue(handle.System, out ParticlePool pool))
            {
                pool.Release(handle.System);
                _systemToPool.Remove(handle.System);
            }
        }

        public void StopAll()
        {
            for (int i = _activeSystems.Count - 1; i >= 0; i--)
            {
                ParticleSystem system = _activeSystems[i];
                if (system != null)
                {
                    system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    Release(new VFXHandle(system));
                }
            }

            _activeSystems.Clear();
        }

        private ParticlePool GetOrCreatePool(ParticleSystem prefab)
        {
            if (_pools.TryGetValue(prefab, out ParticlePool pool))
            {
                return pool;
            }

            // Create new pool for this prefab
            GameObject poolContainer = new GameObject($"ParticlePool_{prefab.name}");
            poolContainer.transform.SetParent(_poolRoot, false);

            pool = new ParticlePool(prefab, poolContainer.transform, _defaultPoolSize);
            _pools[prefab] = pool;

            return pool;
        }

        public void Dispose()
        {
            StopAll();

            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }

            _pools.Clear();
            _systemToPool.Clear();
        }
    }
}

