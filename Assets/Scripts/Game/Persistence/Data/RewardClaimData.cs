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
        /// Levels whose first-clear rewards have been claimed.
        /// </summary>
        public List<LevelClaimEntry> ClaimedLevelRewards = new();

        /// <summary>
        /// Stage indices whose stage-complete rewards have been claimed.
        /// </summary>
        public List<int> ClaimedStageRewards = new();
    }
}
