using System;
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
        private const int FirstStageIndex = 0;
        private const int MinimumStageCount = 1;

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
            MigrateFromLegacyStructure();
            EnsureDataIntegrity();
            SyncGameData();
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
        /// Gets the current stage index stored in player progress.
        /// </summary>
        public int GetCurrentStageIndex()
        {
            return _CachedData.Progress.CurrentStageIndex;
        }

        /// <summary>
        /// Returns the highest unlocked stage index clamped to the provided catalog size.
        /// </summary>
        /// <param name="stageCount">Total stage count in the active catalog.</param>
        public int GetHighestUnlockedStageIndex(int stageCount)
        {
            return ClampStageIndex(GetHighestStageIndex(), stageCount);
        }

        /// <summary>
        /// Determines whether a stage is currently unlocked.
        /// </summary>
        /// <param name="stageIndex">Zero-based stage index to check.</param>
        /// <param name="stageCount">Total stage count in the active catalog.</param>
        public bool IsStageUnlocked(int stageIndex, int stageCount)
        {
            if (stageIndex < FirstStageIndex || stageIndex >= GetNormalizedStageCount(stageCount))
            {
                return false;
            }

            return stageIndex <= GetHighestUnlockedStageIndex(stageCount);
        }

        /// <summary>
        /// Clamps a stage index into the currently unlocked stage range.
        /// </summary>
        /// <param name="stageIndex">Requested stage index.</param>
        /// <param name="stageCount">Total stage count in the active catalog.</param>
        public int ClampStageIndexToUnlockedRange(int stageIndex, int stageCount)
        {
            int highestUnlockedStageIndex = GetHighestUnlockedStageIndex(stageCount);
            if (stageIndex < FirstStageIndex)
            {
                return FirstStageIndex;
            }

            if (stageIndex > highestUnlockedStageIndex)
            {
                return highestUnlockedStageIndex;
            }

            return stageIndex;
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
        /// Ensures reward claim data exists and returns the instance.
        /// </summary>
        public RewardClaimData EnsureRewardClaimData()
        {
            if (_CachedData.RewardClaims == null)
            {
                _CachedData.RewardClaims = new RewardClaimData();
            }

            if (_CachedData.RewardClaims.ClaimedLevelRewards == null)
            {
                _CachedData.RewardClaims.ClaimedLevelRewards = new System.Collections.Generic.List<LevelClaimEntry>();
            }

            if (_CachedData.RewardClaims.ClaimedStageRewards == null)
            {
                _CachedData.RewardClaims.ClaimedStageRewards = new System.Collections.Generic.List<int>();
            }

            return _CachedData.RewardClaims;
        }

        /// <summary>
        /// Ensures knife skin collection exists and returns the instance.
        /// </summary>
        public KnifeSkinCollectionData EnsureKnifeSkinCollection()
        {
            if (_CachedData.Resources.KnifeSkins == null)
            {
                _CachedData.Resources.KnifeSkins = new KnifeSkinCollectionData();
            }

            return _CachedData.Resources.KnifeSkins;
        }

        /// <summary>
        /// Reloads persisted data and updates only the knife skin collection.
        /// </summary>
        public void ReloadKnifeSkinCollection()
        {
            var loaded = _Store.Load();
            if (loaded?.Resources?.KnifeSkins == null)
            {
                return;
            }

            _CachedData ??= new PlayerData();
            EnsureDataIntegrity();
            _CachedData.Resources.KnifeSkins = loaded.Resources.KnifeSkins;
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

        /// <summary>
        /// Schedules a save for updated knife skin unlock data.
        /// </summary>
        public void SaveKnifeSkinCollection()
        {
            SaveNow();
        }

        /// <summary>
        /// Resets all persisted player data to defaults and syncs runtime state.
        /// </summary>
        public void ResetAllData()
        {
            _CachedData = new PlayerData();
            EnsureDataIntegrity();
            SyncGameData();
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

            if (_CachedData.Resources.KnifeSkins == null)
            {
                _CachedData.Resources.KnifeSkins = new KnifeSkinCollectionData();
            }

            if (_CachedData.RewardClaims == null)
            {
                _CachedData.RewardClaims = new RewardClaimData();
            }

            if (_CachedData.RewardClaims.ClaimedLevelRewards == null)
            {
                _CachedData.RewardClaims.ClaimedLevelRewards = new System.Collections.Generic.List<LevelClaimEntry>();
            }

            if (_CachedData.RewardClaims.ClaimedStageRewards == null)
            {
                _CachedData.RewardClaims.ClaimedStageRewards = new System.Collections.Generic.List<int>();
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
            _CachedData.Resources.KnifeSkins ??= new KnifeSkinCollectionData();
            _CachedData.RewardClaims ??= new RewardClaimData();
            _CachedData.RewardClaims.ClaimedLevelRewards ??= new System.Collections.Generic.List<LevelClaimEntry>();
            _CachedData.RewardClaims.ClaimedStageRewards ??= new System.Collections.Generic.List<int>();
        }

        private void SyncGameData()
        {
            _GameData.CurrentStageIndex = _CachedData.Progress.CurrentStageIndex;
        }

        private int ClampStageIndex(int stageIndex, int stageCount)
        {
            int maxStageIndex = GetNormalizedStageCount(stageCount) - MinimumStageCount;
            if (stageIndex < FirstStageIndex)
            {
                return FirstStageIndex;
            }

            if (stageIndex > maxStageIndex)
            {
                return maxStageIndex;
            }

            return stageIndex;
        }

        private int GetNormalizedStageCount(int stageCount)
        {
            return Math.Max(stageCount, MinimumStageCount);
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
