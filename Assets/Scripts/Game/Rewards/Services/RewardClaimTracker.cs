using System.Linq;
using Game.Persistence.Data;
using Game.Persistence.Services;

namespace Game.Rewards.Services
{
    /// <summary>
    /// Reads and updates which progression rewards were already claimed in player data.
    /// </summary>
    public class RewardClaimTracker
    {
        private readonly PlayerDataService _PlayerDataService;

        /// <summary>
        /// Creates a tracker bound to the player data service.
        /// </summary>
        public RewardClaimTracker(PlayerDataService playerDataService)
        {
            _PlayerDataService = playerDataService;
        }

        /// <summary>
        /// Returns whether first-clear rewards for this level were already claimed.
        /// </summary>
        public bool IsLevelFirstClearClaimed(int stageIndex, int levelIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            return data.ClaimedLevelRewards.Any(e => e != null && e.StageIndex == stageIndex && e.LevelIndex == levelIndex);
        }

        /// <summary>
        /// Returns whether stage-complete rewards for this stage index were already claimed.
        /// </summary>
        public bool IsStageCompleteClaimed(int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            return data.ClaimedStageRewards.Contains(stageIndex);
        }

        /// <summary>
        /// Records that first-clear rewards for this level were claimed.
        /// </summary>
        public void MarkLevelFirstClearClaimed(int stageIndex, int levelIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            if (IsLevelFirstClearClaimed(stageIndex, levelIndex))
            {
                return;
            }

            data.ClaimedLevelRewards.Add(new LevelClaimEntry { StageIndex = stageIndex, LevelIndex = levelIndex });
        }

        /// <summary>
        /// Records that stage-complete rewards for this stage were claimed.
        /// </summary>
        public void MarkStageCompleteClaimed(int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            if (data.ClaimedStageRewards.Contains(stageIndex))
            {
                return;
            }

            data.ClaimedStageRewards.Add(stageIndex);
        }
    }
}
