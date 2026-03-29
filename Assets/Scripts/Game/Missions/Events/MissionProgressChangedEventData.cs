using Game.Missions.Interfaces;

namespace Game.Missions.Events
{
    /// <summary>
    /// Raised when mission progress changes for a provider.
    /// </summary>
    public readonly struct MissionProgressChangedEventData
    {
        public IMissionProvider Source { get; }
        public int MissionIndex { get; }
        public int CurrentCount { get; }
        public int RequiredCount { get; }

        public MissionProgressChangedEventData(
            IMissionProvider source,
            int missionIndex,
            int currentCount,
            int requiredCount)
        {
            Source = source;
            MissionIndex = missionIndex;
            CurrentCount = currentCount;
            RequiredCount = requiredCount;
        }
    }
}
