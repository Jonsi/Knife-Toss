namespace Game.Domain.Stage
{
    /// <summary>
    /// Pure stage progress data (no Unity dependencies).
    /// </summary>
    public class StageStats
    {
        public int StageIndex { get; set; }
        public int CurrentLevelIndex { get; set; }
        public int TotalLevelsInStage { get; set; }
    }
}
