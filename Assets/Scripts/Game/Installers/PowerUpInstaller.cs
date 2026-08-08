using Common.Events;
using Game.Flow.Events;
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
        private readonly PowerUpCancelledEventChannel _CancelledChannel;
        private readonly PowerUpOutcomeEventChannel _OutcomeChannel;
        private readonly LevelStartedEventChannel _LevelStartedChannel;

        public PowerUpInstaller(
            PowerUpCatalog catalog,
            PowerUpActivatedEventChannel activatedChannel,
            PowerUpUsedEventChannel usedChannel,
            PowerUpCancelledEventChannel cancelledChannel,
            PowerUpOutcomeEventChannel outcomeChannel,
            LevelStartedEventChannel levelStartedChannel)
        {
            _Catalog = catalog;
            _ActivatedChannel = activatedChannel;
            _UsedChannel = usedChannel;
            _CancelledChannel = cancelledChannel;
            _OutcomeChannel = outcomeChannel;
            _LevelStartedChannel = levelStartedChannel;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Catalog);
            builder.RegisterInstance(_ActivatedChannel).As<IEventChannel<PowerUpActivatedEventData>>();
            builder.RegisterInstance(_UsedChannel).As<IEventChannel<PowerUpUsedEventData>>();
            builder.RegisterInstance(_CancelledChannel).As<IEventChannel<PowerUpCancelledEventData>>();
            builder.RegisterInstance(_OutcomeChannel).As<IEventChannel<PowerUpOutcomeEventData>>();
            builder.RegisterEntryPoint<PowerUpService>()
                .As<IPowerUpService>()
                .AsSelf()
                .WithParameter(_ActivatedChannel)
                .WithParameter(_UsedChannel)
                .WithParameter(_CancelledChannel)
                .WithParameter(_OutcomeChannel)
                .WithParameter(_LevelStartedChannel);
            builder.RegisterEntryPoint<PowerUpEffectService>()
                .AsSelf()
                .WithParameter(_OutcomeChannel);
        }
    }
}
