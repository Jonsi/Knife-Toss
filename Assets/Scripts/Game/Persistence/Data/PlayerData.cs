using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Root container for all persisted player data.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public PlayerProgressData Progress = new PlayerProgressData();
        public PlayerSettingsData Settings = new PlayerSettingsData();
    }
}
