using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Persistence.Data;
using Game.Persistence.Interfaces;
using Game.PowerUps.Data;
using VContainer.Unity;
using UnityEngine;

namespace Game.Persistence.Services
{
    /// <summary>
    /// Manages loading and saving player data to the configured data store.
    /// </summary>
    public class PlayerDataService : IInitializable, IDisposable
    {
        private readonly IPlayerDataStore _Store;
        private readonly GameData _GameData;

        private PlayerData _CachedData;

        public PlayerDataService(
            IPlayerDataStore store,
            GameData gameData)
        {
            _Store = store;
            _GameData = gameData;
        }

        public void Initialize()
        {
            _CachedData = _Store.Load() ?? new PlayerData();
            EnsureDataIntegrity();
            SyncGameData();
        }

        public void Dispose()
        {
            // No cleanup needed since we removed async scheduling
        }

        /// <summary>
        /// Requests a save. Useful for frequent updates like collecting items.
        /// </summary>
        public void RequestSave()
        {
            UpdateProgress();
            SaveNow();
        }

        /// <summary>
        /// Forces an immediate save. Useful for major checkpoints like level completion.
        /// </summary>
        public void ForceSave()
        {
            UpdateProgress();
            SaveNow();
        }

        /// <summary>
        /// Ensures power-up inventory exists and returns the instance.
        /// </summary>
        public PowerUpInventoryData EnsurePowerUpInventory()
        {
            if (_CachedData.PowerUpInventory == null)
            {
                _CachedData.PowerUpInventory = new PowerUpInventoryData();
            }

            return _CachedData.PowerUpInventory;
        }

        /// <summary>
        /// Reloads persisted data and returns the power-up inventory.
        /// Keeps progress/settings in sync with the loaded data.
        /// </summary>
        public void ReloadPowerUpInventory()
        {
            var loaded = _Store.Load();
            if (loaded != null)
            {
                _CachedData = loaded;
                EnsureDataIntegrity();
                SyncGameData();
            }
        }

        /// <summary>
        /// Schedules a save for updated power-up inventory data.
        /// </summary>
        public void SavePowerUpInventory()
        {
            SaveNow();
        }

        private void UpdateProgress()
        {
            _CachedData.Progress.CurrentStageIndex = _GameData.CurrentStageIndex;
            _CachedData.Progress.HighestStageIndex = Math.Max(
                _CachedData.Progress.HighestStageIndex,
                _GameData.CurrentStageIndex);
            _CachedData.Progress.TotalCollectiblesCollected = _GameData.TotalCollected;
        }

        private void EnsureDataIntegrity()
        {
            _CachedData.Progress ??= new PlayerProgressData();
            _CachedData.Settings ??= new PlayerSettingsData();
            _CachedData.PowerUpInventory ??= new PowerUpInventoryData();
        }

        private void SyncGameData()
        {
            _GameData.CurrentStageIndex = _CachedData.Progress.CurrentStageIndex;
            _GameData.TotalCollected = _CachedData.Progress.TotalCollectiblesCollected;
        }

        private void SaveNow()
        {
            try
            {
                _Store.Save(_CachedData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save player data: {e.Message}");
            }
        }
    }
}
