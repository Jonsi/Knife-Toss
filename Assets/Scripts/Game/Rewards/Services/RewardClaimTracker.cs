using System.Collections.Generic;
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
        /// Returns whether stage-completion rewards for this stage were already claimed.
        /// </summary>
        public bool IsStageCompleteClaimed(int collectionIndex, int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            return ListContains(data.ClaimedStageRewards, collectionIndex, stageIndex);
        }

        /// <summary>
        /// Returns whether stage-completion rewards for this stage are available to claim.
        /// </summary>
        public bool IsStageCompleteAvailable(int collectionIndex, int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            return ListContains(data.AvailableStageRewards, collectionIndex, stageIndex);
        }

        /// <summary>
        /// Records that stage-completion rewards for this stage became available.
        /// </summary>
        public void MarkStageCompleteAvailable(int collectionIndex, int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            if (ListContains(data.AvailableStageRewards, collectionIndex, stageIndex) ||
                ListContains(data.ClaimedStageRewards, collectionIndex, stageIndex))
            {
                return;
            }

            data.AvailableStageRewards.Add(new StageRewardKey { CollectionIndex = collectionIndex, StageIndex = stageIndex });
        }

        /// <summary>
        /// Records that stage-completion rewards for this stage were claimed.
        /// </summary>
        public void MarkStageCompleteClaimed(int collectionIndex, int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            if (ListContains(data.ClaimedStageRewards, collectionIndex, stageIndex))
            {
                return;
            }

            data.ClaimedStageRewards.Add(new StageRewardKey { CollectionIndex = collectionIndex, StageIndex = stageIndex });
            RemoveFromList(data.AvailableStageRewards, collectionIndex, stageIndex);
        }

        /// <summary>
        /// Clears the claimable state for a stage reward without marking it as claimed.
        /// </summary>
        public void ClearStageCompleteAvailable(int collectionIndex, int stageIndex)
        {
            var data = _PlayerDataService.EnsureRewardClaimData();
            RemoveFromList(data.AvailableStageRewards, collectionIndex, stageIndex);
        }

        private static bool ListContains(List<StageRewardKey> list, int collectionIndex, int stageIndex)
        {
            if (list == null)
            {
                return false;
            }

            for (var i = 0; i < list.Count; i++)
            {
                StageRewardKey key = list[i];
                if (key != null && key.CollectionIndex == collectionIndex && key.StageIndex == stageIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveFromList(List<StageRewardKey> list, int collectionIndex, int stageIndex)
        {
            if (list == null)
            {
                return;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                StageRewardKey key = list[i];
                if (key != null && key.CollectionIndex == collectionIndex && key.StageIndex == stageIndex)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
