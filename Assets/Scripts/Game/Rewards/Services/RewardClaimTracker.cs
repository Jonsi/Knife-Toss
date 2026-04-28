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
        /// Returns whether stage-completion rewards for this stage index were already claimed.
        /// </summary>
        public bool IsStageCompleteClaimed(int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            return data.ClaimedStageRewards.Contains(stageIndex);
        }

        /// <summary>
        /// Returns whether stage-completion rewards for this stage are available to claim.
        /// </summary>
        public bool IsStageCompleteAvailable(int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            return data.AvailableStageRewards.Contains(stageIndex);
        }

        /// <summary>
        /// Records that stage-completion rewards for this stage became available.
        /// </summary>
        public void MarkStageCompleteAvailable(int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            if (data.AvailableStageRewards.Contains(stageIndex) || data.ClaimedStageRewards.Contains(stageIndex))
            {
                return;
            }

            data.AvailableStageRewards.Add(stageIndex);
        }

        /// <summary>
        /// Records that stage-completion rewards for this stage were claimed.
        /// </summary>
        public void MarkStageCompleteClaimed(int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            if (data.ClaimedStageRewards.Contains(stageIndex))
            {
                return;
            }

            data.ClaimedStageRewards.Add(stageIndex);
            data.AvailableStageRewards.Remove(stageIndex);
        }

        /// <summary>
        /// Clears the claimable state for a stage reward without marking it as claimed.
        /// </summary>
        public void ClearStageCompleteAvailable(int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            data.AvailableStageRewards.Remove(stageIndex);
        }
    }
}
