using Game.Missions.Configs;
using Game.Missions.Interfaces;

namespace Game.Missions.Events
{
    /// <summary>
    /// Raised when a mission crosses its completion threshold.
    /// </summary>
    public readonly struct MissionCompletedEventData
    {
        public IMissionProvider Source { get; }
        public int MissionIndex { get; }
        public MissionDefinition Definition { get; }

        public MissionCompletedEventData(IMissionProvider source, int missionIndex, MissionDefinition definition)
        {
            Source = source;
            MissionIndex = missionIndex;
            Definition = definition;
        }
    }
}
