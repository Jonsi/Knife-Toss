using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using UnityEngine;

namespace KnifeToss.Infrastructure.Persistence
{
    public sealed class PlayerPrefsProgressStore : ILevelProgressStore
    {
        public const string DefaultHighestLevelKey = "HighestLevel";
        private const string ProgressKey = "LevelProgress";

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

        public LevelProgressData LoadProgress()
        {
            if (PlayerPrefs.HasKey(ProgressKey))
            {
                var json = PlayerPrefs.GetString(ProgressKey);
                try
                {
                    return JsonUtility.FromJson<LevelProgressData>(json);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load level progress: {e.Message}");
                    return new LevelProgressData();
                }
            }
            return new LevelProgressData();
        }

        public void SaveProgress(LevelProgressData progress)
        {
            var json = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(ProgressKey, json);
            PlayerPrefs.Save();
        }
    }
}
