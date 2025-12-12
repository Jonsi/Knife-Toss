using Game.Pooling;
using UnityEngine;

namespace Game.Feedback.VFX.Systems
{
    /// <summary>
    /// Bridges particle stop callbacks to the particle pool provider.
    /// </summary>
    public class PooledParticleReturner : MonoBehaviour
    {
        private ParticlePoolProvider _Provider;
        private ParticleSystem _ParticleSystem;

        /// <summary>
        /// Initializes the returner with its provider and particle reference.
        /// </summary>
        /// <param name="provider">Pool provider to receive returned particles.</param>
        /// <param name="particleSystem">Particle system instance.</param>
        public void Initialize(ParticlePoolProvider provider, ParticleSystem particleSystem)
        {
            _Provider = provider;
            _ParticleSystem = particleSystem;
        }

        private void OnParticleSystemStopped()
        {
            _Provider?.OnParticleStopped(_ParticleSystem);
        }
    }
}
