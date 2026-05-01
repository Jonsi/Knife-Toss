using Common.Rewards.Interfaces;
using Game.Flow.Events;
using Game.Flow.Stage;
using Game.Level.Catalogs;
using Game.Persistence.Services;
using VContainer.Unity;

namespace Game.Rewards.Services
{
    /// <summary>
    /// Listens for stage completion and marks configured per-stage rewards as claimable.
    /// </summary>
    public class StageRewardConnector : IInitializable, System.IDisposable
    {
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly StagesConfigCatalog _StagesCatalog;
        private readonly RewardClaimTracker _ClaimTracker;
        private readonly PlayerDataService _PlayerDataService;

        /// <summary>
        /// Creates the connector with flow channels and reward services.
        /// </summary>
        public StageRewardConnector(
            StageCompletedEventChannel stageCompletedChannel,
            StagesConfigCatalog stagesCatalog,
            RewardClaimTracker claimTracker,
            PlayerDataService playerDataService)
        {
            _StageCompletedChannel = stageCompletedChannel;
            _StagesCatalog = stagesCatalog;
            _ClaimTracker = claimTracker;
            _PlayerDataService = playerDataService;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            _StageCompletedChannel.Subscribe(OnStageCompleted);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _StageCompletedChannel.Unsubscribe(OnStageCompleted);
        }

        private void OnStageCompleted(StageData data)
        {
            if (data == null)
            {
                return;
            }

            int stageIndex = data.StageIndex;
            int collectionIndex = data.CollectionIndex;
            if (_ClaimTracker.IsStageCompleteClaimed(collectionIndex, stageIndex) || _ClaimTracker.IsStageCompleteAvailable(collectionIndex, stageIndex))
            {
                return;
            }

            var stageConfig = _StagesCatalog.GetStageConfig(stageIndex);
            var entries = stageConfig?.Rewards;
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var grant = entries[i];
                if (grant != null && grant.TryCreateGrantItem(out IRewardItem item) && item != null)
                {
                    _ClaimTracker.MarkStageCompleteAvailable(collectionIndex, stageIndex);
                    _PlayerDataService.ForceSave();
                    return;
                }
            }
        }
    }
}
