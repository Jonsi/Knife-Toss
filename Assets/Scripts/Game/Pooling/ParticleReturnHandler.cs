using UnityEngine;

namespace Game.Pooling
{
    /// <summary>
    /// Simple component that handles particle system stop callbacks for pooling.
    /// </summary>
    public class ParticleReturnHandler : MonoBehaviour
    {
        private ParticlePoolProvider _provider;

        /// <summary>
        /// Initializes the handler with the pool provider.
        /// </summary>
        public void Initialize(ParticlePoolProvider provider)
        {
            _provider = provider;
        }

        private void OnParticleSystemStopped()
        {
            _provider.Release(GetComponent<ParticleSystem>());
        }
    }
}