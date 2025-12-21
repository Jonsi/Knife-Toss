namespace Game.App.Flow.Events
{
    /// <summary>
    /// Payload for when a level begins.
    /// </summary>
    public class LevelStartedEventData
    {
        /// <summary>
        /// Zero-based stage index.
        /// </summary>
        public int StageIndex { get; set; }

        /// <summary>
        /// Zero-based level index within the stage.
        /// </summary>
        public int LevelIndex { get; set; }

        /// <summary>
        /// Total levels in the current stage.
        /// </summary>
        public int TotalLevelsInStage { get; set; }

        /// <summary>
        /// Total knives allocated for this level.
        /// </summary>
        public int TotalKnives { get; set; }
    }
}
