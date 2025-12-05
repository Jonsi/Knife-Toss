using System;
using System.IO;
using Core.Abstractions;
using Core.Data;
using UnityEngine;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// JSON-backed player data store that writes to Application.persistentDataPath.
    /// </summary>
    public sealed class FilePlayerDataStore : IPlayerDataStore
    {
        private const string DefaultFileName = "playerData.json";

        private readonly string filePath;
        private readonly string tempFilePath;

        public string FilePath => filePath;

        public FilePlayerDataStore()
        {
            filePath = Path.Combine(UnityEngine.Application.persistentDataPath, DefaultFileName);
            tempFilePath = filePath + ".tmp";
        }

        public PlayerData LoadOrCreate()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        PlayerData loaded = JsonUtility.FromJson<PlayerData>(json);
                        if (loaded != null)
                        {
                            return loaded;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load player data from {filePath}: {e.Message}");
            }

            return PlayerData.CreateDefault();
        }

        public void Save(PlayerData data)
        {
            if (data == null)
            {
                data = PlayerData.CreateDefault();
            }

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(tempFilePath, json);

                if (File.Exists(filePath))
                {
                    File.Replace(tempFilePath, filePath, null);
                }
                else
                {
                    File.Move(tempFilePath, filePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save player data to {filePath}: {e.Message}");
            }
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete player data at {filePath}: {e.Message}");
            }
        }
    }
}
