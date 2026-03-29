using System;
using System.Collections.Generic;
using Game.Missions.Data;

namespace Game.Missions.Daily.Data
{
    /// <summary>
    /// Persisted state for the active daily mission rotation.
    /// </summary>
    [Serializable]
    public class DailyMissionData
    {
        public string RefreshDateUtc;
        public List<ActiveMissionEntry> ActiveMissions = new();
    }
}
