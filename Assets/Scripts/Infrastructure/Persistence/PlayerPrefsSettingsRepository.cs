using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using UnityEngine;

namespace KnifeToss.Infrastructure.Persistence
{
    public class PlayerPrefsSettingsRepository : ISettingsRepository
    {
        private const string SettingsKey = "AudioSettings";

        public void Save(AudioSettingsData settings)
        {
            var json = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString(SettingsKey, json);
            PlayerPrefs.Save();
        }

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
