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
        /// Stage indices whose stage-completion rewards are available to claim.
        /// </summary>
        public List<int> AvailableStageRewards = new();

        /// <summary>
        /// Stage indices whose stage-completion rewards have been claimed.
        /// </summary>
        public List<int> ClaimedStageRewards = new();
    }
}
