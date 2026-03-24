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
    }
}
