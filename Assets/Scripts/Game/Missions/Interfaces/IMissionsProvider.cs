using System.Collections.Generic;
using Game.Missions.Configs;

namespace Game.Missions.Interfaces
{
    /// <summary>
    /// Supplies a pool of mission definitions plus the desired active count.
    /// This lets runtime mission systems read from assets now and alternate providers later.
    /// </summary>
    public interface IMissionsProvider
    {
        IReadOnlyList<MissionDefinition> MissionPool { get; }
        int ActiveMissionCount { get; }
    }
}
