using System;
using Game.Flow.Game;
using Game.Persistence.Data;
using Game.Persistence.Interfaces;
using Game.PowerUps.Configs;
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
        private PowerUpCatalog _PowerUpCatalog;

        private PlayerData _CachedData;

        public PlayerDataService(
            IPlayerDataStore store,
            GameData gameData,
            PowerUpCatalog powerUpCatalog = null)
        {
            _Store = store;
            _GameData = gameData;
            _PowerUpCatalog = powerUpCatalog;

            if (_PowerUpCatalog == null)
            {
                _PowerUpCatalog = Resources.Load<PowerUpCatalog>("PowerUpCatalog");
            }
        }

        public void Initialize()
        {
            _CachedData = _Store.Load() ?? new PlayerData();
            
            MigrateFromLegacyStructure();
            EnsureDataIntegrity();

            if (ShouldInitializePowerUps())
            {
                InitializeDefaultPowerUps();
            }
            
            SyncGameData();
        }

        private bool ShouldInitializePowerUps()
        {
            var inventory = _CachedData.Resources.PowerUps;
            if (inventory == null) return true;

            if (_PowerUpCatalog == null) return false;

            // If all powerups are 0, we initialize them (handles new players and fresh installs)
            foreach (var config in _PowerUpCatalog.Entries)
            {
                if (config != null && !string.IsNullOrEmpty(config.InventoryKey))
                {
                    if (inventory.GetCount(config.InventoryKey) > 0)
                        return false;
                }
            }
            return true;
        }

        private void InitializeDefaultPowerUps()
        {
            if (_PowerUpCatalog == null) return;

            foreach (var config in _PowerUpCatalog.Entries)
            {
                if (config != null && !string.IsNullOrEmpty(config.InventoryKey))
                {
                    _CachedData.Resources.PowerUps.SetCount(config.InventoryKey, 5);
                }
            }
            
            SaveNow();
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Gets the current cached player data (used by currency and other systems).
        /// </summary>
        public PlayerData GetData()
        {
            return _CachedData;
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
        /// Gets the highest stage index the player has reached.
        /// </summary>
        public int GetHighestStageIndex()
        {
            return _CachedData.Progress.HighestStageIndex;
        }

        /// <summary>
        /// Ensures power-up inventory exists and returns the instance.
        /// </summary>
        public PowerUpInventoryData EnsurePowerUpInventory()
        {
            if (_CachedData.Resources.PowerUps == null)
            {
                _CachedData.Resources.PowerUps = new PowerUpInventoryData();
            }

            return _CachedData.Resources.PowerUps;
        }

        /// <summary>
        /// Reloads persisted data and returns the power-up inventory.
        /// Keeps progress/settings in sync with the loaded data.
        /// </summary>
        public void ReloadPowerUpInventory()
        {
            var loaded = _Store.Load();
            if (loaded?.Resources?.PowerUps == null)
            {
                return;
            }

            _CachedData ??= new PlayerData();
            EnsureDataIntegrity();
            _CachedData.Resources.PowerUps = loaded.Resources.PowerUps;
        }

        /// <summary>
        /// Gets the player settings, ensuring they exist.
        /// </summary>
        public PlayerSettingsData Settings
        {
            get
            {
                if (_CachedData.Settings == null)
                {
                    _CachedData.Settings = new PlayerSettingsData();
                }
                return _CachedData.Settings;
            }
        }

        /// <summary>
        /// Schedules a save for updated power-up inventory data.
        /// </summary>
        public void SavePowerUpInventory()
        {
            SaveNow();
        }

        private void MigrateFromLegacyStructure()
        {
            if (_CachedData.Resources == null)
            {
                _CachedData.Resources = new PlayerResourcesData();
            }

            if (_CachedData.Resources.PowerUps == null)
            {
                _CachedData.Resources.PowerUps = new PowerUpInventoryData();
            }

            if (_CachedData.Resources.Currencies == null)
            {
                _CachedData.Resources.Currencies = new System.Collections.Generic.List<CurrencyBalanceEntry>();
            }
        }

        private void UpdateProgress()
        {
            _CachedData.Progress.CurrentStageIndex = _GameData.CurrentStageIndex;
            _CachedData.Progress.HighestStageIndex = Math.Max(
                _CachedData.Progress.HighestStageIndex,
                _GameData.CurrentStageIndex);
        }

        private void EnsureDataIntegrity()
        {
            _CachedData.Progress ??= new PlayerProgressData();
            _CachedData.Settings ??= new PlayerSettingsData();
            _CachedData.Resources ??= new PlayerResourcesData();
            _CachedData.Resources.PowerUps ??= new PowerUpInventoryData();
            _CachedData.Resources.Currencies ??= new System.Collections.Generic.List<CurrencyBalanceEntry>();
        }

        private void SyncGameData()
        {
            _GameData.CurrentStageIndex = _CachedData.Progress.CurrentStageIndex;
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