namespace Game.Flow.Stage
{
    /// <summary>
    /// Runtime state for the loaded stage (world index, stage-in-world, level).
    /// </summary>
    public class StageData
    {
        /// <summary>
        /// Stage collection (world) index for this run.
        /// </summary>
        public int CollectionIndex { get; internal set; }

        /// <summary>
        /// Stage index within the collection.
        /// </summary>
        public int StageIndex { get; internal set; }

        /// <summary>
        /// Current level index within the stage.
        /// </summary>
        public int CurrentLevelIndex { get; internal set; }

        /// <summary>
        /// Creates stage data with initial indices.
        /// </summary>
        public StageData(int collectionIndex, int stageIndex)
        {
            CollectionIndex = collectionIndex;
            StageIndex = stageIndex;
            CurrentLevelIndex = 0;
        }
    }
}
