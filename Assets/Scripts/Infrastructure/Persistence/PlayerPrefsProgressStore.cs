using KnifeToss.Core.Abstractions;
using UnityEngine;

namespace KnifeToss.Infrastructure.Persistence
{



    public sealed class PlayerPrefsProgressStore : ILevelProgressStore
    {
        public const string DefaultHighestLevelKey = "HighestLevel";

        private readonly string highestLevelKey;

        public PlayerPrefsProgressStore(string key)
        {
            highestLevelKey = key;
        }

        public int LoadHighestLevel(int fallbackLevel)
        {
            return PlayerPrefs.GetInt(highestLevelKey, fallbackLevel);
        }

        public void SaveHighestLevel(int levelIndex)
        {
            PlayerPrefs.SetInt(highestLevelKey, levelIndex);
            PlayerPrefs.Save();
        }
    }
}
