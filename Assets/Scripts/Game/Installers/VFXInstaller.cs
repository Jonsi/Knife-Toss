using Common.Events;
using Game.Entities.Knife.Data;
using Game.Feedback.VFX.Services;
using Game.Pooling;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs visual feedback systems.
    /// </summary>
    public class VFXInstaller : IInstaller
    {
        private readonly PrefabsConfiguration _PrefabsConfiguration;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="VFXInstaller"/> class.
        /// </summary>
        public VFXInstaller(
            PrefabsConfiguration prefabsConfiguration,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel)
        {
            _PrefabsConfiguration = prefabsConfiguration;
            _KnifeImpactChannel = knifeImpactChannel;
        }

        /// <summary>
        /// Registers visual feedback services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            // Only register ParticlePoolProvider if the prefab entry is available
            if (_PrefabsConfiguration?.GetKnifeHitParticles() != null)
            {
                builder.Register<ParticlePoolProvider>(Lifetime.Singleton)
                    .WithParameter(_PrefabsConfiguration)
                    .WithParameter(ParticlePrefabType.KnifeHitTarget);
            }

            builder.RegisterEntryPoint<VFXFeedbackService>();
        }
    }
}

