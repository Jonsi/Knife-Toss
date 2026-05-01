namespace Game.Flow.Game
{
    /// <summary>
    /// Session-wide runtime game state (stage index). Currency is tracked via ICurrencyService.
    /// </summary>
    public class GameData
    {
        /// <summary>
        /// Active stage collection (world) index.
        /// </summary>
        public int CurrentCollectionIndex { get; internal set; }

        /// <summary>
        /// Current stage index within the active collection.
        /// </summary>
        public int CurrentStageIndex { get; internal set; }

        /// <summary>
        /// Resets session progression state.
        /// </summary>
        public void Reset()
        {
            CurrentCollectionIndex = 0;
            CurrentStageIndex = 0;
        }
    }
}
