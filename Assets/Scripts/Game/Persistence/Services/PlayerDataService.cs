using System;
using System.Collections.Generic;
using Game.Flow.Game;
using Game.Level.Interfaces;
using Game.Missions.Daily.Data;
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

        /// <summary>
        /// Creates the service with persistence and session state.
        /// </summary>
        public PlayerDataService(
            IPlayerDataStore store,
            GameData gameData)
        {
            _Store = store;
            _GameData = gameData;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            _CachedData = _Store.Load() ?? new PlayerData();
            MigrateFromLegacyStructure();
            EnsureDataIntegrity();
            SyncGameData();
        }

        /// <inheritdoc />
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
        /// Highest unlocked collection index (world frontier).
        /// </summary>
        public int GetHighestCollectionIndex()
        {
            return _CachedData.Progress.HighestCollectionIndex;
        }

        /// <summary>
        /// Highest unlocked stage index within the highest unlocked collection.
        /// </summary>
        public int GetHighestStageIndex()
        {
            return _CachedData.Progress.HighestStageIndex;
        }

        /// <summary>
        /// Current collection index in saved progress.
        /// </summary>
        public int GetCurrentCollectionIndex()
        {
            return _CachedData.Progress.CurrentCollectionIndex;
        }

        /// <summary>
        /// Current stage index within the current collection in saved progress.
        /// </summary>
        public int GetCurrentStageIndex()
        {
            return _CachedData.Progress.CurrentStageIndex;
        }

        /// <summary>
        /// Highest unlocked local stage index in the given collection, capped by stage count.
        /// </summary>
        public int GetHighestUnlockedLocalStageIndex(int collectionIndex, int stageCountInCollection, IStagesCollectionCatalog stagesCatalog)
        {
            int normalizedCount = GetNormalizedStageCount(stageCountInCollection);
            int maxStageIndex = normalizedCount - MinimumStageCount;
            int highestC = GetHighestCollectionIndex();
            int highestS = GetHighestStageIndex();

            if (collectionIndex < highestC)
            {
                return maxStageIndex;
            }

            if (collectionIndex > highestC)
            {
                return FirstStageIndex - 1;
            }

            return Math.Min(highestS, maxStageIndex);
        }

        /// <summary>
        /// Whether the stage at the given world and local index is unlocked.
        /// </summary>
        public bool IsStageUnlocked(int collectionIndex, int localStageIndex, IStagesCollectionCatalog stagesCatalog)
        {
            if (stagesCatalog == null)
            {
                return false;
            }

            int stageCount = stagesCatalog.GetStageSlotCountForWorld(collectionIndex);
            int normalized = GetNormalizedStageCount(stageCount);
            if (localStageIndex < FirstStageIndex || localStageIndex >= normalized)
            {
                return false;
            }

            int highestC = GetHighestCollectionIndex();
            int highestS = GetHighestStageIndex();
            return CompareProgress(collectionIndex, localStageIndex, highestC, highestS) <= 0;
        }

        /// <summary>
        /// Whether the player has cleared this stage (progress frontier is strictly past this stage).
        /// </summary>
        public bool IsStageCleared(int collectionIndex, int localStageIndex, IStagesCollectionCatalog stagesCatalog)
        {
            if (!IsStageUnlocked(collectionIndex, localStageIndex, stagesCatalog))
            {
                return false;
            }

            int highestC = GetHighestCollectionIndex();
            int highestS = GetHighestStageIndex();
            return CompareProgress(collectionIndex, localStageIndex, highestC, highestS) < 0;
        }

        /// <summary>
        /// Clamps a local stage index to the unlocked range for that collection.
        /// </summary>
        public int ClampLocalStageToUnlockedRange(
            int collectionIndex,
            int localStageIndex,
            int stageCountInCollection,
            IStagesCollectionCatalog stagesCatalog)
        {
            int maxUnlocked = GetHighestUnlockedLocalStageIndex(collectionIndex, stageCountInCollection, stagesCatalog);
            if (maxUnlocked < FirstStageIndex)
            {
                return FirstStageIndex;
            }

            if (localStageIndex < FirstStageIndex)
            {
                return FirstStageIndex;
            }

            if (localStageIndex > maxUnlocked)
            {
                return maxUnlocked;
            }

            return localStageIndex;
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

            if (_CachedData.RewardClaims.AvailableStageRewards == null)
            {
                _CachedData.RewardClaims.AvailableStageRewards = new List<StageRewardKey>();
            }

            if (_CachedData.RewardClaims.ClaimedStageRewards == null)
            {
                _CachedData.RewardClaims.ClaimedStageRewards = new List<StageRewardKey>();
            }

            return _CachedData.RewardClaims;
        }

        /// <summary>
        /// Ensures daily mission data exists and returns the instance.
        /// </summary>
        public DailyMissionData EnsureDailyMissionData()
        {
            if (_CachedData.DailyMissions == null)
            {
                _CachedData.DailyMissions = new DailyMissionData();
            }

            if (_CachedData.DailyMissions.ActiveMissions == null)
            {
                _CachedData.DailyMissions.ActiveMissions = new List<Game.Missions.Data.ActiveMissionEntry>();
            }

            return _CachedData.DailyMissions;
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
        /// Ensures IAP entitlements data exists and returns the instance.
        /// </summary>
        public IapEntitlementsData EnsureIapEntitlements()
        {
            if (_CachedData.Resources.IapEntitlements == null)
            {
                _CachedData.Resources.IapEntitlements = new IapEntitlementsData();
            }

            if (_CachedData.Resources.IapEntitlements.OwnedProductKeys == null)
            {
                _CachedData.Resources.IapEntitlements.OwnedProductKeys = new List<string>();
            }

            return _CachedData.Resources.IapEntitlements;
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
        /// Schedules a save for updated IAP entitlement data.
        /// </summary>
        public void SaveIapEntitlements()
        {
            SaveNow();
        }

        /// <summary>
        /// Resets all persisted player data to defaults and syncs runtime state.
        /// </summary>
        public void ResetAllData()
        {
            ResetAllData(null);
        }

        /// <summary>
        /// Resets all persisted player data and applies optional resource defaults.
        /// </summary>
        public void ResetAllData(PlayerDefaultBalanceConfiguration defaultBalanceConfiguration)
        {
            _CachedData = new PlayerData();
            EnsureDataIntegrity();
            defaultBalanceConfiguration?.ApplyTo(_CachedData);
            SyncGameData();
            SaveNow();
        }

        private static int CompareProgress(int collectionIndexA, int stageIndexA, int collectionIndexB, int stageIndexB)
        {
            int collectionCompare = collectionIndexA.CompareTo(collectionIndexB);
            if (collectionCompare != 0)
            {
                return collectionCompare;
            }

            return stageIndexA.CompareTo(stageIndexB);
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
                _CachedData.Resources.Currencies = new List<CurrencyBalanceEntry>();
            }

            if (_CachedData.Resources.KnifeSkins == null)
            {
                _CachedData.Resources.KnifeSkins = new KnifeSkinCollectionData();
            }

            if (_CachedData.Resources.IapEntitlements == null)
            {
                _CachedData.Resources.IapEntitlements = new IapEntitlementsData();
            }

            if (_CachedData.Resources.IapEntitlements.OwnedProductKeys == null)
            {
                _CachedData.Resources.IapEntitlements.OwnedProductKeys = new List<string>();
            }

            if (_CachedData.RewardClaims == null)
            {
                _CachedData.RewardClaims = new RewardClaimData();
            }

            if (_CachedData.RewardClaims.AvailableStageRewards == null)
            {
                _CachedData.RewardClaims.AvailableStageRewards = new List<StageRewardKey>();
            }

            if (_CachedData.RewardClaims.ClaimedStageRewards == null)
            {
                _CachedData.RewardClaims.ClaimedStageRewards = new List<StageRewardKey>();
            }

            if (_CachedData.DailyMissions == null)
            {
                _CachedData.DailyMissions = new DailyMissionData();
            }

            if (_CachedData.DailyMissions.ActiveMissions == null)
            {
                _CachedData.DailyMissions.ActiveMissions = new List<Game.Missions.Data.ActiveMissionEntry>();
            }

            if (_CachedData.Onboarding == null)
            {
                _CachedData.Onboarding = new PlayerOnboardingData();
            }
        }

        private void UpdateProgress()
        {
            _CachedData.Progress.CurrentCollectionIndex = _GameData.CurrentCollectionIndex;
            _CachedData.Progress.CurrentStageIndex = _GameData.CurrentStageIndex;
            if (CompareProgress(
                    _GameData.CurrentCollectionIndex,
                    _GameData.CurrentStageIndex,
                    _CachedData.Progress.HighestCollectionIndex,
                    _CachedData.Progress.HighestStageIndex) > 0)
            {
                _CachedData.Progress.HighestCollectionIndex = _GameData.CurrentCollectionIndex;
                _CachedData.Progress.HighestStageIndex = _GameData.CurrentStageIndex;
            }
        }

        private void EnsureDataIntegrity()
        {
            _CachedData.Progress ??= new PlayerProgressData();
            _CachedData.Settings ??= new PlayerSettingsData();
            _CachedData.Resources ??= new PlayerResourcesData();
            _CachedData.Resources.PowerUps ??= new PowerUpInventoryData();
            _CachedData.Resources.Currencies ??= new List<CurrencyBalanceEntry>();
            _CachedData.Resources.KnifeSkins ??= new KnifeSkinCollectionData();
            _CachedData.Resources.IapEntitlements ??= new IapEntitlementsData();
            _CachedData.Resources.IapEntitlements.OwnedProductKeys ??= new List<string>();
            _CachedData.RewardClaims ??= new RewardClaimData();
            _CachedData.RewardClaims.AvailableStageRewards ??= new List<StageRewardKey>();
            _CachedData.RewardClaims.ClaimedStageRewards ??= new List<StageRewardKey>();
            _CachedData.DailyMissions ??= new DailyMissionData();
            _CachedData.DailyMissions.ActiveMissions ??= new List<Game.Missions.Data.ActiveMissionEntry>();
            _CachedData.Onboarding ??= new PlayerOnboardingData();
        }

        private void SyncGameData()
        {
            _GameData.CurrentCollectionIndex = _CachedData.Progress.CurrentCollectionIndex;
            _GameData.CurrentStageIndex = _CachedData.Progress.CurrentStageIndex;
        }

        /// <summary>
        /// Clamps session collection and stage indices to the unlocked frontier and valid catalog bounds.
        /// </summary>
        public void ClampSelectionToUnlocked(IStagesCollectionCatalog stagesCatalog)
        {
            if (_GameData == null || stagesCatalog == null)
            {
                return;
            }

            int highestC = _CachedData.Progress.HighestCollectionIndex;
            int highestS = _CachedData.Progress.HighestStageIndex;
            int c = _GameData.CurrentCollectionIndex;
            int s = _GameData.CurrentStageIndex;

            if (CompareProgress(c, s, highestC, highestS) > 0)
            {
                _GameData.CurrentCollectionIndex = highestC;
                _GameData.CurrentStageIndex = highestS;
                c = highestC;
                s = highestS;
            }

            int maxCol = Math.Max(0, stagesCatalog.CollectionsCount - 1);
            c = Mathf.Clamp(c, 0, maxCol);
            _GameData.CurrentCollectionIndex = c;

            int stageCount = stagesCatalog.GetStageSlotCountForWorld(c);
            _GameData.CurrentStageIndex = ClampLocalStageToUnlockedRange(c, s, stageCount, stagesCatalog);
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
