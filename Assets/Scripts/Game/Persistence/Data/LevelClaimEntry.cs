using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Identifies a single level within a stage for first-clear reward tracking.
    /// </summary>
    [Serializable]
    public class LevelClaimEntry
    {
        /// <summary>
        /// Zero-based stage index in the active catalog.
        /// </summary>
        public int StageIndex;

        /// <summary>
        /// Zero-based level index within the stage.
        /// </summary>
        public int LevelIndex;
    }
}
