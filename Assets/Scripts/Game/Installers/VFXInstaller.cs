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
        private readonly PoolSettings _PoolSettings;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="VFXInstaller"/> class.
        /// </summary>
        public VFXInstaller(
            PoolSettings poolSettings,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel)
        {
            _PoolSettings = poolSettings;
            _KnifeImpactChannel = knifeImpactChannel;
        }

        /// <summary>
        /// Registers visual feedback services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            // Only register ParticlePoolProvider if the prefab is available
            if (_PoolSettings?.KnifeHitTargetParticlesPrefab != null)
            {
                builder.Register<ParticlePoolProvider>(Lifetime.Singleton)
                    .WithParameter(_PoolSettings);
            }

            builder.RegisterEntryPoint<VFXFeedbackService>();
        }
    }
}

