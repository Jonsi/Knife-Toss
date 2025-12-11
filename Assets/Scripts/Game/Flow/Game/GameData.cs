namespace Game.Flow.Game
{
    public class GameData
    {
        public int CurrentStageIndex { get; internal set; }
        
        /// <summary>
        /// Total currency collected across the entire game session.
        /// </summary>
        public int TotalCollected { get; internal set; }

        public GameData()
        {
            CurrentStageIndex = 0;
            TotalCollected = 0;
        }

        public void Reset()
        {
            CurrentStageIndex = 0;
            TotalCollected = 0;
        }
    }
}
