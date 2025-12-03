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

        /// <summary>
        /// Performs the PlayerPrefsProgressStore operation.
        /// </summary>
        public PlayerPrefsProgressStore(string key)
        {
            highestLevelKey = key;
        }

        /// <summary>
        /// Performs the LoadHighestLevel operation.
        /// </summary>
        public int LoadHighestLevel(int fallbackLevel)
        {
            return PlayerPrefs.GetInt(highestLevelKey, fallbackLevel);
        }

        /// <summary>
        /// Performs the SaveHighestLevel operation.
        /// </summary>
        public void SaveHighestLevel(int levelIndex)
        {
            PlayerPrefs.SetInt(highestLevelKey, levelIndex);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Performs the LoadProgress operation.
        /// </summary>
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

        /// <summary>
        /// Performs the SaveProgress operation.
        /// </summary>
        public void SaveProgress(LevelProgressData progress)
        {
            var json = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(ProgressKey, json);
            PlayerPrefs.Save();
        }
    }
}
