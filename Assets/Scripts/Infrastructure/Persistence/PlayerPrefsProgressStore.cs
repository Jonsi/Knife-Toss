using KnifeToss.Core.Abstractions;
using UnityEngine;

namespace KnifeToss.Infrastructure.Persistence
{
    /// <summary>
    /// Simple persistence adapter that hides PlayerPrefs usage behind an interface.
    /// </summary>
    public sealed class PlayerPrefsProgressStore : ILevelProgressStore
    {
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
