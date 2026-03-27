using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Flow.Events;
using Game.Flow.Stage;
using Game.Level.Catalogs;
using Game.Persistence.Services;
using Game.Rewards;
using Game.Rewards.Configs;
using VContainer.Unity;

namespace Game.Rewards.Services
{
    /// <summary>
    /// Listens for stage completion and grants configured per-stage rewards once per stage.
    /// </summary>
    public class StageRewardConnector : IInitializable, System.IDisposable
    {
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly StagesConfigCatalog _StagesCatalog;
        private readonly IRewardGrantService _GrantService;
        private readonly RewardClaimTracker _ClaimTracker;
        private readonly PlayerDataService _PlayerDataService;

        /// <summary>
        /// Creates the connector with flow channels and reward services.
        /// </summary>
        public StageRewardConnector(
            StageCompletedEventChannel stageCompletedChannel,
            StagesConfigCatalog stagesCatalog,
            IRewardGrantService grantService,
            RewardClaimTracker claimTracker,
            PlayerDataService playerDataService)
        {
            _StageCompletedChannel = stageCompletedChannel;
            _StagesCatalog = stagesCatalog;
            _GrantService = grantService;
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
            HandleStageCompletedAsync(data).Forget();
        }

        private async UniTaskVoid HandleStageCompletedAsync(StageData data)
        {
            if (data == null)
            {
                return;
            }

            int stageIndex = data.StageIndex;
            if (_ClaimTracker.IsStageCompleteClaimed(stageIndex))
            {
                return;
            }

            var stageConfig = _StagesCatalog.GetStageConfig(stageIndex);
            var entries = stageConfig?.Rewards;
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            var items = new List<IRewardItem>(entries.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                var grant = entries[i];
                if (grant != null && grant.TryCreateGrantItem(out var item))
                {
                    items.Add(item);
                }
            }

            if (items.Count == 0)
            {
                return;
            }

            await _GrantService.GrantAsync(items, RewardPresentationTitles.StageComplete);

            _ClaimTracker.MarkStageCompleteClaimed(stageIndex);
            _PlayerDataService.ForceSave();
        }
    }
}
