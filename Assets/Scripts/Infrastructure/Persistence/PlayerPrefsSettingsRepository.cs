using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using UnityEngine;

namespace KnifeToss.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the PlayerPrefsSettingsRepository class.
    /// </summary>
    public class PlayerPrefsSettingsRepository : ISettingsRepository
    {
        private const string SettingsKey = "AudioSettings";

        /// <summary>
        /// Performs the Save operation.
        /// </summary>
        public void Save(AudioSettingsData settings)
        {
            var json = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString(SettingsKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Performs the Load operation.
        /// </summary>
        public AudioSettingsData Load()
        {
            if (PlayerPrefs.HasKey(SettingsKey))
            {
                var json = PlayerPrefs.GetString(SettingsKey);
                return JsonUtility.FromJson<AudioSettingsData>(json);
            }

            return new AudioSettingsData { isMuted = false };
        }
    }
}
