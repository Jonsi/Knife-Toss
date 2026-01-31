using System.IO;
using Game.Persistence.Data;
using Game.Persistence.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Persistence.Storage
{
    /// <summary>
    /// Persists player data to a JSON file on disk using Newtonsoft.Json for dictionary and list support.
    /// </summary>
    public class FilePlayerDataStore : IPlayerDataStore
    {
        private const string FileName = "playerData.json";
        private readonly string _FilePath;
        private readonly string _TempFilePath;

        public FilePlayerDataStore()
        {
            _FilePath = Path.Combine(Application.persistentDataPath, FileName);
            _TempFilePath = _FilePath + ".tmp";
        }

        /// <summary>
        /// Loads player data from disk.
        /// </summary>
        public PlayerData Load()
        {
            try
            {
                if (!File.Exists(_FilePath))
                {
                    return new PlayerData();
                }

                string json = File.ReadAllText(_FilePath);
                var data = JsonConvert.DeserializeObject<PlayerData>(json);
                return data ?? new PlayerData();
            }
            catch
            {
                return new PlayerData();
            }
        }

        /// <summary>
        /// Saves player data to disk.
        /// </summary>
        public void Save(PlayerData data)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                EnsureDirectory();

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(_TempFilePath, json);

                if (File.Exists(_FilePath))
                {
                    File.Delete(_FilePath);
                }

                File.Move(_TempFilePath, _FilePath);
            }
            catch
            {
                if (File.Exists(_TempFilePath))
                {
                    File.Delete(_TempFilePath);
                }
            }
        }

        private void EnsureDirectory()
        {
            string directory = Path.GetDirectoryName(_FilePath);
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
