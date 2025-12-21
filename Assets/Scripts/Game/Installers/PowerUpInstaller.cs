using Common.Events;
using Game.Entities.Collectible.Events;
using Game.Entities.Collectible.Data;
using Game.App.Flow.Events;
using Game.PowerUps.Configs;
using Game.PowerUps.Events;
using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using Game.PowerUps.Services;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs power-up services and configuration.
    /// </summary>
    public class PowerUpInstaller : IInstaller
    {
        private readonly PowerUpCatalog _Catalog;
        private readonly PowerUpActivatedEventChannel _ActivatedChannel;
        private readonly PowerUpUsedEventChannel _UsedChannel;
        private readonly LevelStartedEventChannel _LevelStartedChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;

        public PowerUpInstaller(
            PowerUpCatalog catalog,
            PowerUpActivatedEventChannel activatedChannel,
            PowerUpUsedEventChannel usedChannel,
            LevelStartedEventChannel levelStartedChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel)
        {
            _Catalog = catalog;
            _ActivatedChannel = activatedChannel;
            _UsedChannel = usedChannel;
            _LevelStartedChannel = levelStartedChannel;
            _CollectibleHitChannel = collectibleHitChannel;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Catalog);
            builder.RegisterInstance(_ActivatedChannel).As<IEventChannel<PowerUpActivatedEventData>>();
            builder.RegisterInstance(_UsedChannel).As<IEventChannel<PowerUpUsedEventData>>();
            builder.RegisterEntryPoint<PowerUpService>()
                .As<IPowerUpService>()
                .AsSelf()
                .WithParameter(_ActivatedChannel)
                .WithParameter(_UsedChannel)
                .WithParameter(_LevelStartedChannel)
                .WithParameter(_CollectibleHitChannel);
        }
    }
}
