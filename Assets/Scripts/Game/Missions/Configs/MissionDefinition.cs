using System.Collections.Generic;
using Game.Rewards.Configs;
using UnityEngine;

namespace Game.Missions.Configs
{
    /// <summary>
    /// Generic mission definition reusable across daily, weekly, or event-based providers.
    /// </summary>
    [CreateAssetMenu(menuName = "Knife Toss/Missions/Mission Definition")]
    public class MissionDefinition : ScriptableObject
    {
        [field: SerializeField]
        [field: Tooltip("Shared mission objective key routed from gameplay events.")]
        public string ObjectiveTypeKey { get; private set; }

        [field: SerializeField]
        [field: Min(1)]
        [field: Tooltip("How much progress is required before the mission completes.")]
        public int TargetCount { get; private set; } = 1;

        [field: SerializeField]
        [field: Tooltip("Formatted with the target count, for example: Complete {0} levels.")]
        public string DescriptionFormat { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Objective icon used in the mission list.")]
        public Sprite Icon { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Rewards granted when the mission is claimed.")]
        public List<RewardGrantEntryBase> Rewards { get; private set; } = new();

        [field: SerializeField]
        [field: Min(1)]
        [field: Tooltip("Relative weight used when randomly selecting active missions.")]
        public int SelectionWeight { get; private set; } = 1;

        public string DefinitionKey => name;

        public string GetDescription()
        {
            return string.IsNullOrEmpty(DescriptionFormat)
                ? string.Empty
                : string.Format(DescriptionFormat, TargetCount);
        }
    }
}
