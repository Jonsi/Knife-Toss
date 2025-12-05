using System;

namespace Core.Data
{
    /// <summary>
    /// Root player save data. Extend with new sections without changing IO code.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public int version = 1;
        public PlayerSettingsData settings = new PlayerSettingsData();
        public PlayerProgressData progress = new PlayerProgressData();

        public static PlayerData CreateDefault()
        {
            return new PlayerData
            {
                version = 1,
                settings = new PlayerSettingsData(),
                progress = new PlayerProgressData()
            };
        }
    }

    /// <summary>
    /// Grouped player settings.
    /// </summary>
    [Serializable]
    public class PlayerSettingsData
    {
        public AudioSettingsData audio = new AudioSettingsData { isMuted = false };
        public string language = "en";
        public bool vibrationEnabled = true;
    }

    /// <summary>
    /// Player progress payload.
    /// </summary>
    [Serializable]
    public class PlayerProgressData
    {
        public int highestLevel = 1;
        public int coins = 0;
        public LevelProgressData levelProgress = new LevelProgressData();
    }
}
