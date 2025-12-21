namespace Game.Domain.Level
{
    /// <summary>
    /// Pure level statistics data (no Unity dependencies).
    /// </summary>
    public class LevelStats
    {
        public int KnifeCount { get; set; }
        public int KnivesThrown { get; set; }
        public int KnivesStuck { get; set; }
        public bool IsThrowingActive { get; set; }
        public int CollectedCount { get; set; }
    }
}
