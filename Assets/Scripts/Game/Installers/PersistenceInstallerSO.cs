using System;
using Game.DI;
using Common.Events;
using Game.Entities.Collectible.Events;
using Game.App.Flow.Events;
using Game.Persistence.Interfaces;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs persistence services for saving player progress and settings.
    /// </summary>
    [CreateAssetMenu(fileName = "PersistenceInstaller", menuName = "Knife Toss/Installers/Persistence")]
    public class PersistenceInstallerSO : BaseInstallerSO
    {
        [Header("Event Channels")]
        [SerializeField] private LevelCompletedEventChannel _LevelCompletedChannel;
        [SerializeField] private StageCompletedEventChannel _StageCompletedChannel;
        [SerializeField] private CollectibleHitEventChannel _CollectibleHitChannel;

        /// <summary>
        /// Registers persistence services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<FilePlayerDataStore>(Lifetime.Singleton)
                .As<IPlayerDataStore>();

            builder.Register<PlayerDataService>(Lifetime.Singleton)
                .WithParameter(_LevelCompletedChannel)
                .WithParameter(_StageCompletedChannel)
                .WithParameter(_CollectibleHitChannel)
                .AsSelf()
                .AsImplementedInterfaces();
        }
    }
}
