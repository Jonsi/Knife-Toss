using System;
using Common.Ads.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Events;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.Flow.Stage;
using Game.Missions.Data;
using Game.PowerUps.Data;
using Game.PowerUps.Events;
using Game.Store.Events;
using VContainer.Unity;

namespace Game.Missions.Services
{
    /// <summary>
    /// Converts gameplay event channels into generic mission progress reports.
    /// </summary>
    public class MissionProgressRouter : IInitializable, IDisposable
    {
        private readonly MissionTracker _Tracker;
        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly LevelFailedEventChannel _LevelFailedChannel;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly CollectibleHitEventChannel _CollectibleHitChannel;
        private readonly PowerUpUsedEventChannel _PowerUpUsedChannel;
        private readonly PurchaseCompletedEventChannel _PurchaseCompletedChannel;
        private readonly AdCompletedEventChannel _AdCompletedChannel;

        private bool _HasFailedCurrentStage;

        public MissionProgressRouter(
            MissionTracker tracker,
            LevelCompletedEventChannel levelCompletedChannel,
            LevelFailedEventChannel levelFailedChannel,
            StageCompletedEventChannel stageCompletedChannel,
            CollectibleHitEventChannel collectibleHitChannel,
            PowerUpUsedEventChannel powerUpUsedChannel,
            PurchaseCompletedEventChannel purchaseCompletedChannel,
            AdCompletedEventChannel adCompletedChannel)
        {
            _Tracker = tracker;
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
            _StageCompletedChannel = stageCompletedChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _PowerUpUsedChannel = powerUpUsedChannel;
            _PurchaseCompletedChannel = purchaseCompletedChannel;
            _AdCompletedChannel = adCompletedChannel;
        }

        public void Initialize()
        {
            _LevelCompletedChannel?.Subscribe(OnLevelCompleted);
            _LevelFailedChannel?.Subscribe(OnLevelFailed);
            _StageCompletedChannel?.Subscribe(OnStageCompleted);
            _CollectibleHitChannel?.Subscribe(OnCollectibleHit);
            _PowerUpUsedChannel?.Subscribe(OnPowerUpUsed);
            _PurchaseCompletedChannel?.Subscribe(OnPurchaseCompleted);
            _AdCompletedChannel?.Subscribe(OnAdCompleted);
        }

        public void Dispose()
        {
            _LevelCompletedChannel?.Unsubscribe(OnLevelCompleted);
            _LevelFailedChannel?.Unsubscribe(OnLevelFailed);
            _StageCompletedChannel?.Unsubscribe(OnStageCompleted);
            _CollectibleHitChannel?.Unsubscribe(OnCollectibleHit);
            _PowerUpUsedChannel?.Unsubscribe(OnPowerUpUsed);
            _PurchaseCompletedChannel?.Unsubscribe(OnPurchaseCompleted);
            _AdCompletedChannel?.Unsubscribe(OnAdCompleted);
        }

        private void OnLevelCompleted(LevelData _)
        {
            _Tracker.ReportProgress(MissionObjectiveKeys.CompleteLevels, 1);
        }

        private void OnLevelFailed(LevelData _)
        {
            _HasFailedCurrentStage = true;
        }

        private void OnStageCompleted(StageData _)
        {
            var hasFailedCurrentStage = _HasFailedCurrentStage;
            _HasFailedCurrentStage = false;

            if (hasFailedCurrentStage)
            {
                return;
            }

            _Tracker.ReportProgress(MissionObjectiveKeys.CompleteStageNoFail, 1);
        }

        private void OnCollectibleHit(CollectibleHitEventData _)
        {
            _Tracker.ReportProgress(MissionObjectiveKeys.HitCollectibles, 1);
        }

        private void OnPowerUpUsed(PowerUpUsedEventData _)
        {
            _Tracker.ReportProgress(MissionObjectiveKeys.UsePowerUps, 1);
        }

        private void OnPurchaseCompleted(PurchaseCompletedEventData _)
        {
            _Tracker.ReportProgress(MissionObjectiveKeys.PurchaseItem, 1);
        }

        private void OnAdCompleted(AdEventData data)
        {
            if (!data.Result.Success)
            {
                return;
            }

            _Tracker.ReportProgress(MissionObjectiveKeys.WatchAd, 1);
        }
    }
}
