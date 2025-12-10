namespace Game.Flow.Game
{
    public class GameData
    {
        public int CurrentStageIndex { get; internal set; }
        
        // Add other game-session persistent data here (like total score, lives, etc if not in PlayerData)

        public GameData()
        {
            CurrentStageIndex = 0;
        }

        public void Reset()
        {
            CurrentStageIndex = 0;
        }
    }
}
