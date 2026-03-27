using System;
using System.Collections.Generic;
using Game.Level.Configurations.Stage;
using Game.Store.Attributes;
using Game.Store.Rewards;
using UnityEngine;

namespace Game.Rewards.Configs
{
    /// <summary>
    /// Reward bundles configured for one stage (first-clear per level and stage-complete).
    /// </summary>
    [Serializable]
    public class StageRewardEntry
    {
        [field: SerializeField]
        [field: Tooltip("Stage asset this row applies to. Must match an entry in the active Stages collection.")]
        public StageConfig Stage { get; private set; }

        [field: SerializeReference]
        [field: SerializeReferenceDropdown]
        [field: Tooltip("Granted once the first time the player clears each level in this stage (per level).")]
        public List<ItemReward> FirstClearRewards { get; private set; } = new();

        [field: SerializeReference]
        [field: SerializeReferenceDropdown]
        [field: Tooltip("Granted once when the player completes all levels in this stage for the first time.")]
        public List<ItemReward> StageCompleteRewards { get; private set; } = new();
    }
}
