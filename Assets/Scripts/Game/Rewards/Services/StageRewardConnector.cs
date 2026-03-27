using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.Flow.Stage;
using Game.Level.Catalogs;
using Game.Persistence.Services;
using Game.Rewards.Configs;
using Game.Rewards;
using Game.Store.Rewards;
using VContainer.Unity;

namespace Game.Rewards.Services
{
    /// <summary>
    /// Listens for level and stage completion and grants configured first-clear and stage-complete rewards.
    /// </summary>
    public class StageRewardConnector : IInitializable, System.IDisposable
    {
        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly StagesConfigCatalog _StagesCatalog;
        private readonly StageRewardCatalog _RewardCatalog;
        private readonly IRewardGrantService _GrantService;
        private readonly RewardClaimTracker _ClaimTracker;
        private readonly PlayerDataService _PlayerDataService;

        /// <summary>
        /// Creates the connector with flow channels and reward services.
        /// </summary>
        public StageRewardConnector(
            LevelCompletedEventChannel levelCompletedChannel,
            StageCompletedEventChannel stageCompletedChannel,
            StagesConfigCatalog stagesCatalog,
            StageRewardCatalog rewardCatalog,
            IRewardGrantService grantService,
            RewardClaimTracker claimTracker,
            PlayerDataService playerDataService)
        {
            _LevelCompletedChannel = levelCompletedChannel;
            _StageCompletedChannel = stageCompletedChannel;
            _StagesCatalog = stagesCatalog;
            _RewardCatalog = rewardCatalog;
            _GrantService = grantService;
            _ClaimTracker = claimTracker;
            _PlayerDataService = playerDataService;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            _LevelCompletedChannel.Subscribe(OnLevelCompleted);
            _StageCompletedChannel.Subscribe(OnStageCompleted);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _LevelCompletedChannel.Unsubscribe(OnLevelCompleted);
            _StageCompletedChannel.Unsubscribe(OnStageCompleted);
        }

        private void OnLevelCompleted(LevelData data)
        {
            HandleLevelCompletedAsync(data).Forget();
        }

        private void OnStageCompleted(StageData data)
        {
            HandleStageCompletedAsync(data).Forget();
        }

        private async UniTaskVoid HandleLevelCompletedAsync(LevelData data)
        {
            if (_RewardCatalog == null || data == null || data.StageConfig == null || data.Config == null)
            {
                return;
            }

            int stageIndex = _StagesCatalog.GetStageIndexOrNegative(data.StageConfig);
            int levelIndex = _StagesCatalog.GetLevelIndexInStageOrNegative(stageIndex, data.Config);
            if (stageIndex < 0 || levelIndex < 0)
            {
                return;
            }

            if (_ClaimTracker.IsLevelFirstClearClaimed(stageIndex, levelIndex))
            {
                return;
            }

            var entry = _RewardCatalog.FindEntry(data.StageConfig);
            int rewardCount = CountRewards(entry?.FirstClearRewards);
            if (entry == null || rewardCount == 0)
            {
                return;
            }

            var items = new List<IRewardItem>(rewardCount);
            CollectItems(entry.FirstClearRewards, items);

            await _GrantService.GrantAsync(items, RewardPresentationTitles.LevelFirstClear);

            _ClaimTracker.MarkLevelFirstClearClaimed(stageIndex, levelIndex);
            _PlayerDataService.ForceSave();
        }

        private async UniTaskVoid HandleStageCompletedAsync(StageData data)
        {
            if (_RewardCatalog == null || data == null)
            {
                return;
            }

            int stageIndex = data.StageIndex;
            if (_ClaimTracker.IsStageCompleteClaimed(stageIndex))
            {
                return;
            }

            var stageConfig = _StagesCatalog.GetStageConfig(stageIndex);
            var entry = stageConfig != null ? _RewardCatalog.FindEntry(stageConfig) : null;
            int rewardCount = CountRewards(entry?.StageCompleteRewards);
            if (entry == null || rewardCount == 0)
            {
                return;
            }

            var items = new List<IRewardItem>(rewardCount);
            CollectItems(entry.StageCompleteRewards, items);

            await _GrantService.GrantAsync(items, RewardPresentationTitles.StageComplete);

            _ClaimTracker.MarkStageCompleteClaimed(stageIndex);
            _PlayerDataService.ForceSave();
        }

        private static int CountRewards(IReadOnlyList<ItemReward> list)
        {
            if (list == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static void CollectItems(IReadOnlyList<ItemReward> source, List<IRewardItem> destination)
        {
            if (source == null)
            {
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (item != null)
                {
                    destination.Add(item);
                }
            }
        }
    }
}
