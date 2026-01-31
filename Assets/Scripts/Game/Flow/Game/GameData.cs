namespace Game.Flow.Game
{
    /// <summary>
    /// Session-wide runtime game state (stage index). Currency is tracked via ICurrencyService.
    /// </summary>
    public class GameData
    {
        /// <summary>
        /// Current stage index in progression.
        /// </summary>
        public int CurrentStageIndex { get; internal set; }

        /// <summary>
        /// Resets session progression state.
        /// </summary>
        public void Reset()
        {
            CurrentStageIndex = 0;
        }
    }
}
