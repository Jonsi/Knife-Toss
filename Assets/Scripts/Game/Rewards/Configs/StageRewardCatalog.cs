using System.Collections.Generic;
using Game.Level.Configurations.Stage;
using UnityEngine;

namespace Game.Rewards.Configs
{
    /// <summary>
    /// Maps each stage to first-clear and stage-complete reward bundles.
    /// </summary>
    [CreateAssetMenu(fileName = "StageRewardCatalog", menuName = "Knife Toss/Systems/Rewards/Stage Reward Catalog")]
    public class StageRewardCatalog : ScriptableObject
    {
        [field: SerializeField]
        [field: Tooltip("One entry per stage that should grant progression rewards. Each entry references a Stage asset and its reward lists.")]
        public List<StageRewardEntry> Entries { get; private set; } = new();

        /// <summary>
        /// Finds the reward entry for the given stage asset, or null if none.
        /// </summary>
        public StageRewardEntry FindEntry(StageConfig stageConfig)
        {
            if (stageConfig == null || Entries == null)
            {
                return null;
            }

            foreach (var entry in Entries)
            {
                if (entry != null && entry.Stage == stageConfig)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
