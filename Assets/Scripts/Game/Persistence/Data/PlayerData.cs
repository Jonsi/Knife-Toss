using System;
using Game.PowerUps.Data;

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
        public PowerUpInventoryData PowerUpInventory = new PowerUpInventoryData();
    }
}
