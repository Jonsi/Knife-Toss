using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Level.Configurations.Stage;
using Game.Persistence.Services;

namespace Game.Rewards.Services
{
    /// <summary>
    /// Builds and grants deferred stage-completion rewards from the stages menu.
    /// </summary>
    public class StageRewardClaimService
    {
        private readonly IRewardGrantService _RewardGrantService;
        private readonly RewardClaimTracker _RewardClaimTracker;
        private readonly PlayerDataService _PlayerDataService;

        /// <summary>
        /// Creates the claim service with reward and persistence dependencies.
        /// </summary>
        public StageRewardClaimService(
            IRewardGrantService rewardGrantService,
            RewardClaimTracker rewardClaimTracker,
            PlayerDataService playerDataService)
        {
            _RewardGrantService = rewardGrantService;
            _RewardClaimTracker = rewardClaimTracker;
            _PlayerDataService = playerDataService;
        }

        /// <summary>
        /// Grants configured rewards for a stage if the claim tracker allows it.
        /// </summary>
        public async UniTask<bool> ClaimAsync(StageConfig stageConfig, int collectionIndex, int stageIndex)
        {
            if (stageConfig == null ||
                !_RewardClaimTracker.IsStageCompleteAvailable(collectionIndex, stageIndex) ||
                _RewardClaimTracker.IsStageCompleteClaimed(collectionIndex, stageIndex))
            {
                return false;
            }

            var rewards = BuildRewards(stageConfig);
            if (rewards.Count == 0)
            {
                _RewardClaimTracker.ClearStageCompleteAvailable(collectionIndex, stageIndex);
                _PlayerDataService.ForceSave();
                return false;
            }

            await _RewardGrantService.GrantAsync(rewards);
            _RewardClaimTracker.MarkStageCompleteClaimed(collectionIndex, stageIndex);
            _PlayerDataService.ForceSave();
            return true;
        }

        private static List<IRewardItem> BuildRewards(StageConfig stageConfig)
        {
            var items = new List<IRewardItem>();
            var entries = stageConfig.Rewards;
            if (entries == null)
            {
                return items;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.TryCreateGrantItem(out var item) && item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }
}
