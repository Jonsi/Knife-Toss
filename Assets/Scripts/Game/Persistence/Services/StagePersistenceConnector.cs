using System;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Events;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.Flow.Stage;
using VContainer.Unity;

namespace Game.Persistence.Services
{
    /// <summary>
    /// Connects stage-specific events to the global PlayerDataService.
    /// Acts as a bridge between the transient stage scope and the persistent global scope.
    /// </summary>
    public class StagePersistenceConnector : IInitializable, IDisposable
    {
        private readonly PlayerDataService _PlayerDataService;
        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly CollectibleHitEventChannel _CollectibleHitChannel;

        public StagePersistenceConnector(
            PlayerDataService playerDataService,
            LevelCompletedEventChannel levelCompletedChannel,
            StageCompletedEventChannel stageCompletedChannel,
            CollectibleHitEventChannel collectibleHitChannel)
        {
            _PlayerDataService = playerDataService;
            _LevelCompletedChannel = levelCompletedChannel;
            _StageCompletedChannel = stageCompletedChannel;
            _CollectibleHitChannel = collectibleHitChannel;
        }

        public void Initialize()
        {
            _LevelCompletedChannel.Subscribe(OnLevelCompleted);
            _StageCompletedChannel.Subscribe(OnStageCompleted);
            _CollectibleHitChannel.Subscribe(OnCollectibleHit);
        }

        public void Dispose()
        {
            _LevelCompletedChannel.Unsubscribe(OnLevelCompleted);
            _StageCompletedChannel.Unsubscribe(OnStageCompleted);
            _CollectibleHitChannel.Unsubscribe(OnCollectibleHit);
        }

        private void OnLevelCompleted(LevelData data)
        {
            _PlayerDataService.ForceSave();
        }

        private void OnStageCompleted(StageData data)
        {
            _PlayerDataService.ForceSave();
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            _PlayerDataService.RequestSave();
        }
    }
}
