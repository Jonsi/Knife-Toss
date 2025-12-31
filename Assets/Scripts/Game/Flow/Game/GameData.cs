namespace Game.Flow.Game
{
    public class GameData
    {
        public int CurrentStageIndex { get; internal set; } = 0;

        /// <summary>
        /// Total currency collected across the entire game session.
        /// </summary>
        public int TotalCollected { get; internal set; } = 0;

        public void Reset()
        {
            CurrentStageIndex = 0;
            TotalCollected = 0;
        }
    }
}
