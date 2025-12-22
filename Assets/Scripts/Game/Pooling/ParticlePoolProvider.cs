using System;
using System.Collections.Generic;
using Game.Configs.Prefabs;
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
        private readonly FactoryPrefabEntry<ParticleSystem> _Entry;

        /// <summary>
        /// Initializes a new instance of the particle pool provider.
        /// </summary>
        /// <param name="prefabsConfiguration">Prefab configuration containing particle entries.</param>
        public ParticlePoolProvider(PrefabsConfiguration prefabsConfiguration)
        {
            _Entry = prefabsConfiguration?.GetKnifeHitParticles();
            if (_Entry == null)
                throw new ArgumentException("No particle prefab entry found for knife hit particles.", nameof(prefabsConfiguration));

            var prefab = _Entry.Prefab;
            if (prefab == null)
                throw new ArgumentException("Particle prefab entry must reference a prefab with ParticleSystem component.");

            // Create a dedicated parent for pooled objects
            var poolParent = new GameObject($"ParticlePool_{prefab.name}").transform;
            UnityEngine.Object.DontDestroyOnLoad(poolParent.gameObject);
            _poolParent = poolParent;
            _Pool = new ObjectPool<ParticleSystem>(
                () => CreateParticle(prefab),
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
                defaultCapacity: _Entry.WarmupCount,
                maxSize: _Entry.MaxSize);

            // Pre-warm the pool by creating initial objects
            var warmupObjects = new ParticleSystem[_Entry.WarmupCount];
            for (int i = 0; i < _Entry.WarmupCount; i++)
            {
                warmupObjects[i] = _Pool.Get();
            }
            for (int i = 0; i < _Entry.WarmupCount; i++)
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

        private ParticleSystem CreateParticle(ParticleSystem prefab)
        {
            var instance = UnityEngine.Object.Instantiate(prefab);
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to instantiate particle system from prefab {prefab}");
            }
            var main = instance.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            var handler = instance.gameObject.AddComponent<ParticleReturnHandler>();
            handler.Initialize(this, instance);
            instance.gameObject.SetActive(false);
            instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return instance;
        }
    }
}
