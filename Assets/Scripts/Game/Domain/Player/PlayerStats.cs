namespace Game.Domain.Player
{
    /// <summary>
    /// Pure player progress data (no Unity dependencies).
    /// </summary>
    public class PlayerStats
    {
        public int CurrentStageIndex { get; set; }
        public int HighestStageIndex { get; set; }
        public int TotalCollectiblesCollected { get; set; }
    }
}
