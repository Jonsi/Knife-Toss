using UnityEngine;

namespace Game.UI.Stages
{
    /// <summary>
    /// View data for a single stage list entry.
    /// </summary>
    public class StageListItemData
    {
        /// <summary>
        /// Zero-based stage index in the catalog.
        /// </summary>
        public int StageIndex { get; set; }

        /// <summary>
        /// One-based stage number shown in UI.
        /// </summary>
        public int StageNumber { get; set; }

        /// <summary>
        /// Display name shown for the stage.
        /// </summary>
        public string StageName { get; set; }

        /// <summary>
        /// Optional stage icon sprite.
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// Whether the stage is locked for the current player.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Whether the stage is the current selected stage.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Whether the stage has a configured reward chest.
        /// </summary>
        public bool HasReward { get; set; }

        /// <summary>
        /// Whether the stage reward can currently be claimed.
        /// </summary>
        public bool CanClaimReward { get; set; }

        /// <summary>
        /// Whether the stage reward was already claimed.
        /// </summary>
        public bool IsRewardClaimed { get; set; }

        /// <summary>
        /// Whether the player has cleared this stage (beat it; frontier is past it).
        /// </summary>
        public bool IsStageComplete { get; set; }
    }
}
