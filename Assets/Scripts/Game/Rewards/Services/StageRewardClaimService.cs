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

        public StageRewardClaimService(
            IRewardGrantService rewardGrantService,
            RewardClaimTracker rewardClaimTracker,
            PlayerDataService playerDataService)
        {
            _RewardGrantService = rewardGrantService;
            _RewardClaimTracker = rewardClaimTracker;
            _PlayerDataService = playerDataService;
        }

        public async UniTask<bool> ClaimAsync(StageConfig stageConfig, int stageIndex)
        {
            if (stageConfig == null ||
                !_RewardClaimTracker.IsStageCompleteAvailable(stageIndex) ||
                _RewardClaimTracker.IsStageCompleteClaimed(stageIndex))
            {
                return false;
            }

            var rewards = BuildRewards(stageConfig);
            if (rewards.Count == 0)
            {
                _RewardClaimTracker.ClearStageCompleteAvailable(stageIndex);
                _PlayerDataService.ForceSave();
                return false;
            }

            await _RewardGrantService.GrantAsync(rewards);
            _RewardClaimTracker.MarkStageCompleteClaimed(stageIndex);
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
