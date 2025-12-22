using Game.DI;
using Common.Events;
using Game.Entities.Collectible.Events;
using Game.App.Flow.Events;
using Game.PowerUps.Configs;
using Game.PowerUps.Events;
using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using Game.PowerUps.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs power-up services and configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpInstaller", menuName = "Knife Toss/Installers/PowerUps")]
    public class PowerUpInstallerSO : BaseInstallerSO
    {
        [Header("Catalog")]
        [SerializeField] private PowerUpCatalog _Catalog;

        [Header("Event Channels")]
        [SerializeField] private PowerUpActivatedEventChannel _ActivatedChannel;
        [SerializeField] private PowerUpUsedEventChannel _UsedChannel;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private CollectibleHitEventChannel _CollectibleHitChannel;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Catalog);
            builder.RegisterInstance(_ActivatedChannel).As<IEventChannel<PowerUpActivatedEventData>>();
            builder.RegisterInstance(_UsedChannel).As<IEventChannel<PowerUpUsedEventData>>();
            builder.Register<PowerUpService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf()
                .WithParameter(_ActivatedChannel)
                .WithParameter(_UsedChannel)
                .WithParameter(_LevelStartedChannel)
                .WithParameter(_CollectibleHitChannel);
        }
    }
}
