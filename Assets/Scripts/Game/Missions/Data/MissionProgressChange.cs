using Game.Missions.Configs;

namespace Game.Missions.Data
{
    /// <summary>
    /// Describes a progress mutation returned by a mission provider.
    /// </summary>
    public readonly struct MissionProgressChange
    {
        public int MissionIndex { get; }
        public MissionDefinition Definition { get; }
        public int CurrentCount { get; }
        public int RequiredCount { get; }
        public bool JustCompleted { get; }

        public MissionProgressChange(
            int missionIndex,
            MissionDefinition definition,
            int currentCount,
            int requiredCount,
            bool justCompleted)
        {
            MissionIndex = missionIndex;
            Definition = definition;
            CurrentCount = currentCount;
            RequiredCount = requiredCount;
            JustCompleted = justCompleted;
        }
    }
}
