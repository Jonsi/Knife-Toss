using Core.Abstractions;
using Core.Data;
using UnityEngine;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// In-memory player data manager with typed helpers for settings and progress.
    /// </summary>
    public sealed class PlayerDataService : IPlayerDataService
    {
        private readonly IPlayerDataStore dataStore;
        private PlayerData playerData;

        public PlayerData Data => playerData;
        public PlayerSettingsData Settings => playerData.settings;
        public PlayerProgressData Progress => playerData.progress;
        public LevelProgressData LevelProgress => playerData.progress.levelProgress;

        public PlayerDataService(IPlayerDataStore playerDataStore)
        {
            dataStore = playerDataStore;
            Reload();

            if (Application.isPlaying)
            {
                PlayerDataLocator.Runtime = this;
            }
        }

        public void Save()
        {
            dataStore.Save(playerData);
        }

        public void Reload()
        {
            playerData = dataStore.LoadOrCreate() ?? PlayerData.CreateDefault();
            EnsureDefaultSections();
        }

        public void Reset()
        {
            playerData = PlayerData.CreateDefault();
            Save();
        }

        public void Replace(PlayerData newData)
        {
            playerData = newData ?? PlayerData.CreateDefault();
            EnsureDefaultSections();
            Save();
        }

        public int GetCurrentStage(int fallbackStage)
        {
            return Mathf.Max(1, Mathf.Max(fallbackStage, playerData.progress.currentStageIndex));
        }

        public void SetCurrentStage(int stageIndex)
        {
            stageIndex = Mathf.Max(1, stageIndex);
            var progress = playerData.progress;
            bool changed = false;

            if (stageIndex != progress.currentStageIndex)
            {
                progress.currentStageIndex = stageIndex;
                changed = true;
            }

            if (stageIndex > progress.highestStageIndex)
            {
                progress.highestStageIndex = stageIndex;
                changed = true;
            }

            if (changed)
            {
                Save();
            }
        }

        public void ResetToStage(int stageIndex)
        {
            playerData.progress.currentStageIndex = Mathf.Max(1, stageIndex);
            Save();
        }

        public void AddFruitsHit(int count)
        {
            if (count > 0)
            {
                playerData.progress.totalFruitsHit += count;
                Save();
            }
        }

        public int GetTotalFruitsHit()
        {
            return playerData.progress.totalFruitsHit;
        }

        private void EnsureDefaultSections()
        {
            if (playerData.settings == null)
            {
                playerData.settings = new PlayerSettingsData();
            }

            if (playerData.progress == null)
            {
                playerData.progress = new PlayerProgressData();
            }

            if (playerData.progress.highestStageIndex < 1)
            {
                playerData.progress.highestStageIndex = Mathf.Max(1, playerData.progress.currentStageIndex);
            }

            if (playerData.progress.levelProgress == null)
            {
                playerData.progress.levelProgress = new LevelProgressData();
            }
        }
    }
}
