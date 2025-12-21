using System;
using System.Threading.Tasks;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Events;
using Game.App.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Level;
using Game.Flow.Stage;
using Game.Persistence.Data;
using Game.Persistence.Interfaces;
using Game.PowerUps.Data;
using UnityEngine;
using VContainer.Unity;

namespace Game.Persistence.Services
{
    /// <summary>
    /// Manages loading and saving player data to the configured data store.
    /// </summary>
    public class PlayerDataService : IInitializable, IDisposable
    {
        private readonly IPlayerDataStore _Store;
        private readonly GameData _GameData;
        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly CollectibleHitEventChannel _CollectibleHitChannel;

        private PlayerData _CachedData;
        private bool _IsSaveScheduled;

        public PlayerDataService(
            IPlayerDataStore store,
            GameData gameData,
            LevelCompletedEventChannel levelCompletedChannel,
            StageCompletedEventChannel stageCompletedChannel,
            CollectibleHitEventChannel collectibleHitChannel)
        {
            _Store = store;
            _GameData = gameData;
            _LevelCompletedChannel = levelCompletedChannel;
            _StageCompletedChannel = stageCompletedChannel;
            _CollectibleHitChannel = collectibleHitChannel;
        }

        public void Initialize()
        {
            _CachedData = _Store.Load() ?? new PlayerData();

            if (_CachedData.Progress == null)
            {
                _CachedData.Progress = new PlayerProgressData();
            }

            if (_CachedData.Settings == null)
            {
                _CachedData.Settings = new PlayerSettingsData();
            }

            if (_CachedData.PowerUpInventory == null)
            {
                _CachedData.PowerUpInventory = new PowerUpInventoryData();
            }

            _GameData.CurrentStageIndex = _CachedData.Progress.CurrentStageIndex;
            _GameData.TotalCollected = _CachedData.Progress.TotalCollectiblesCollected;

            _LevelCompletedChannel.Subscribe(OnLevelCompleted);
            _StageCompletedChannel.Subscribe(OnStageCompleted);

            if (_CollectibleHitChannel != null)
            {
                _CollectibleHitChannel.Subscribe(OnCollectibleHit);
            }
        }

        public void Dispose()
        {
            _LevelCompletedChannel.Unsubscribe(OnLevelCompleted);
            _StageCompletedChannel.Unsubscribe(OnStageCompleted);

            if (_CollectibleHitChannel != null)
            {
                _CollectibleHitChannel.Unsubscribe(OnCollectibleHit);
            }
        }

        private void OnLevelCompleted(LevelData data)
        {
            _CachedData.Progress.CurrentStageIndex = _GameData.CurrentStageIndex;
            _CachedData.Progress.HighestStageIndex = Math.Max(
                _CachedData.Progress.HighestStageIndex,
                _GameData.CurrentStageIndex);
            _CachedData.Progress.TotalCollectiblesCollected = _GameData.TotalCollected;

            SaveNow();
        }

        private void OnStageCompleted(StageData data)
        {
            _CachedData.Progress.CurrentStageIndex = _GameData.CurrentStageIndex;
            _CachedData.Progress.HighestStageIndex = Math.Max(
                _CachedData.Progress.HighestStageIndex,
                _GameData.CurrentStageIndex);
            _CachedData.Progress.TotalCollectiblesCollected = _GameData.TotalCollected;

            SaveNow();
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            _CachedData.Progress.TotalCollectiblesCollected = _GameData.TotalCollected;
            ScheduleSave();
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
        public PowerUpInventoryData ReloadPowerUpInventory()
        {
            var loaded = _Store.Load();
            if (loaded != null)
            {
                loaded.Progress ??= new PlayerProgressData();
                loaded.Settings ??= new PlayerSettingsData();
                loaded.PowerUpInventory ??= new PowerUpInventoryData();

                _CachedData = loaded;
                _GameData.CurrentStageIndex = _CachedData.Progress.CurrentStageIndex;
                _GameData.TotalCollected = _CachedData.Progress.TotalCollectiblesCollected;
            }

            if (_CachedData.PowerUpInventory == null)
            {
                _CachedData.PowerUpInventory = new PowerUpInventoryData();
            }

            return _CachedData.PowerUpInventory;
        }

        /// <summary>
        /// Schedules a save for updated power-up inventory data.
        /// </summary>
        public void SavePowerUpInventory()
        {
            ScheduleSave();
        }

        private void SaveNow()
        {
            _Store.Save(_CachedData);
        }

        private void ScheduleSave()
        {
            if (_IsSaveScheduled)
            {
                return;
            }

            _IsSaveScheduled = true;
            _ = SaveAfterDelay();
        }

        private async Task SaveAfterDelay()
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(0.5f));
                _Store.Save(_CachedData);
            }
            finally
            {
                _IsSaveScheduled = false;
            }
        }
    }
}
