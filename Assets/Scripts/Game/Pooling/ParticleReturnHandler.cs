using UnityEngine;

namespace Game.Pooling
{
    /// <summary>
    /// Simple component that handles particle system stop callbacks for pooling.
    /// </summary>
    public class ParticleReturnHandler : MonoBehaviour
    {
        private ParticlePoolProvider _provider;
        private ParticleSystem _particleSystem;

        /// <summary>
        /// Initializes the handler with the pool provider.
        /// </summary>
        public void Initialize(ParticlePoolProvider provider, ParticleSystem particleSystem)
        {
            _provider = provider;
            _particleSystem = particleSystem;
        }

        private void OnParticleSystemStopped()
        {
            if (_particleSystem != null)
            {
                _provider?.Release(_particleSystem);
            }
        }
    }
}
