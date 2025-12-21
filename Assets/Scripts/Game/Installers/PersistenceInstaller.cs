using System;
using Common.Events;
using Game.Entities.Collectible.Events;
using Game.App.Flow.Events;
using Game.Persistence.Interfaces;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs persistence services for saving player progress and settings.
    /// </summary>
    public class PersistenceInstaller : IInstaller
    {
        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly CollectibleHitEventChannel _CollectibleHitChannel;

        public PersistenceInstaller(
            LevelCompletedEventChannel levelCompletedChannel,
            StageCompletedEventChannel stageCompletedChannel,
            CollectibleHitEventChannel collectibleHitChannel)
        {
            _LevelCompletedChannel = levelCompletedChannel;
            _StageCompletedChannel = stageCompletedChannel;
            _CollectibleHitChannel = collectibleHitChannel;
        }

        /// <summary>
        /// Registers persistence services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.Register<FilePlayerDataStore>(Lifetime.Singleton)
                .As<IPlayerDataStore>();

            builder.RegisterEntryPoint<PlayerDataService>()
                .WithParameter(_LevelCompletedChannel)
                .WithParameter(_StageCompletedChannel)
                .WithParameter(_CollectibleHitChannel)
                .AsSelf()
                .As<IInitializable>()
                .As<IDisposable>();
        }
    }
}
