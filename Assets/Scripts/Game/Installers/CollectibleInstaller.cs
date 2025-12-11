using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Events;
using Game.Entities.Collectible.Spawning;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs collectible feature services.
    /// </summary>
    public class CollectibleInstaller : IInstaller
    {
        private readonly CollectibleHitEventChannel _HitChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectibleInstaller"/> class.
        /// </summary>
        /// <param name="hitChannel">Event channel for collectible hits.</param>
        public CollectibleInstaller(CollectibleHitEventChannel hitChannel)
        {
            _HitChannel = hitChannel;
        }

        /// <summary>
        /// Registers collectible feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.Register<CollectibleSpawner>(Lifetime.Singleton);
            builder.RegisterInstance(_HitChannel).As<IEventChannel<CollectibleHitEventData>>();
        }
    }
}

