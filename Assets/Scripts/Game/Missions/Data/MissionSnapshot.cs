using Game.Missions.Configs;

namespace Game.Missions.Data
{
    /// <summary>
    /// Immutable mission state snapshot used by UI.
    /// </summary>
    public readonly struct MissionSnapshot
    {
        public MissionDefinition Definition { get; }
        public int CurrentCount { get; }
        public int RequiredCount { get; }
        public bool Claimed { get; }
        public bool IsCompleted { get; }

        public MissionSnapshot(
            MissionDefinition definition,
            int currentCount,
            int requiredCount,
            bool claimed,
            bool isCompleted)
        {
            Definition = definition;
            CurrentCount = currentCount;
            RequiredCount = requiredCount;
            Claimed = claimed;
            IsCompleted = isCompleted;
        }
    }
}
