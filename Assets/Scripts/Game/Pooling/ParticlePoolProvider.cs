using Game.Feedback.VFX.Systems;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Pooling
{
    /// <summary>
    /// Provides pooled particle systems backed by Unity ObjectPool.
    /// </summary>
    public class ParticlePoolProvider
    {
        private readonly ObjectPool<ParticleSystem> _Pool;

        /// <summary>
        /// Initializes a new instance of the particle pool provider.
        /// </summary>
        /// <param name="prefab">Particle system prefab used for pool creation.</param>
        /// <param name="lifecycleActions">Optional lifecycle hooks.</param>
        public ParticlePoolProvider(ParticleSystem prefab, PoolLifecycleActions<ParticleSystem> lifecycleActions = null)
        {
            _Pool = new ObjectPool<ParticleSystem>(
                () => CreateParticle(prefab, this),
                particle =>
                {
                    lifecycleActions?.OnGet?.Invoke(particle);
                    particle.gameObject.SetActive(true);
                    particle.Play(true);
                },
                particle =>
                {
                    lifecycleActions?.OnRelease?.Invoke(particle);
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particle.gameObject.SetActive(false);
                });
        }

        /// <summary>
        /// Retrieves a particle system from the pool and positions it.
        /// </summary>
        /// <param name="position">World position to place the particle system.</param>
        /// <returns>Pooled particle system instance.</returns>
        public ParticleSystem Get(Vector3 position)
        {
            var particle = _Pool.Get();
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

        private static ParticleSystem CreateParticle(ParticleSystem prefab, ParticlePoolProvider provider)
        {
            var instance = Object.Instantiate(prefab);
            var main = instance.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            var returner = instance.gameObject.AddComponent<PooledParticleReturner>();
            returner.Initialize(provider, instance);
            instance.gameObject.SetActive(false);
            instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return instance;
        }
    }
}
