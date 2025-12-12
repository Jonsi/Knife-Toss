using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Pooling
{
    /// <summary>
    /// Provides pooled particle systems backed by Unity ObjectPool.
    /// Maintains its own list of pooled objects for efficient management.
    /// </summary>
    public class ParticlePoolProvider
    {
        private readonly ObjectPool<ParticleSystem> _Pool;
        private readonly Transform _poolParent;
        private readonly List<ParticleSystem> _pooledObjects = new();
        private readonly ParticleSystem _prefab;

        /// <summary>
        /// Initializes a new instance of the particle pool provider.
        /// </summary>
        /// <param name="poolSettings">Pool settings containing particle prefab and configuration.</param>
        public ParticlePoolProvider(PoolSettings poolSettings)
        {
            if (poolSettings == null)
            {
                throw new System.ArgumentNullException(nameof(poolSettings), "Pool settings cannot be null");
            }

            var prefab = poolSettings.KnifeHitTargetParticlesPrefab;
            if (prefab == null)
            {
                throw new System.ArgumentException("KnifeHitTargetParticlesPrefab cannot be null in pool settings", nameof(poolSettings));
            }

            _prefab = prefab;

            // Create a dedicated parent for pooled objects
            var poolParent = new GameObject($"ParticlePool_{prefab.name}").transform;
            Object.DontDestroyOnLoad(poolParent.gameObject);
            _poolParent = poolParent;
            _Pool = new ObjectPool<ParticleSystem>(
                () => CreateParticle(),
                particle =>
                {
                    particle.transform.SetParent(null, false);
                    particle.gameObject.SetActive(true);
                    particle.Play(true);
                },
                particle =>
                {
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particle.gameObject.SetActive(false);
                    particle.transform.SetParent(_poolParent, false);
                    _pooledObjects.Add(particle);
                },
                defaultCapacity: poolSettings.ParticlePoolWarmupCount,
                maxSize: poolSettings.ParticlePoolMaxSize);

            // Pre-warm the pool by creating initial objects
            var warmupObjects = new ParticleSystem[poolSettings.ParticlePoolWarmupCount];
            for (int i = 0; i < poolSettings.ParticlePoolWarmupCount; i++)
            {
                warmupObjects[i] = _Pool.Get();
            }
            for (int i = 0; i < poolSettings.ParticlePoolWarmupCount; i++)
            {
                _Pool.Release(warmupObjects[i]);
            }
        }

        /// <summary>
        /// Retrieves a particle system from the pool and positions it.
        /// </summary>
        /// <param name="position">World position to place the particle system.</param>
        /// <returns>Pooled particle system instance.</returns>
        public ParticleSystem Get(Vector3 position)
        {
            var particle = _Pool.Get();
            _pooledObjects.Remove(particle);
            particle.transform.position = position;
            return particle;
        }

        /// <summary>
        /// Returns a particle system to the pool.
        /// </summary>
        /// <param name="particle">Particle system instance to release.</param>
        public void Release(ParticleSystem particle)
        {
            if (particle == null)
                return;

            _Pool.Release(particle);
        }

        /// <summary>
        /// Handles particle stop callback to return to the pool.
        /// </summary>
        /// <param name="particle">Particle system that stopped.</param>
        public void OnParticleStopped(ParticleSystem particle)
        {
            Release(particle);
        }

        private ParticleSystem CreateParticle()
        {
            if (_prefab == null)
            {
                throw new System.InvalidOperationException("Particle prefab is null");
            }

            var instance = Object.Instantiate<ParticleSystem>(_prefab);
            if (instance == null)
            {
                throw new System.InvalidOperationException($"Failed to instantiate particle system from prefab {_prefab}");
            }
            var main = instance.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            var handler = instance.gameObject.AddComponent<ParticleReturnHandler>();
            handler.Initialize(this);
            instance.gameObject.SetActive(false);
            instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return instance;
        }
    }
}
