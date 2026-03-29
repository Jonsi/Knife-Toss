using System;

namespace Game.Missions.Data
{
    /// <summary>
    /// Persisted state for one active mission instance.
    /// </summary>
    [Serializable]
    public class ActiveMissionEntry
    {
        public string DefinitionKey;
        public int CurrentCount;
        public bool Claimed;
    }
}
