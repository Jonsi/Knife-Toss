using System;
using System.Collections.Generic;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Tracks which progression rewards have already been claimed.
    /// </summary>
    [Serializable]
    public class RewardClaimData
    {
        /// <summary>
        /// Stage-completion rewards available to claim.
        /// </summary>
        public List<StageRewardKey> AvailableStageRewards = new();

        /// <summary>
        /// Stage-completion rewards already claimed.
        /// </summary>
        public List<StageRewardKey> ClaimedStageRewards = new();
    }
}
