using System;
using Common.Ads.Data;
using Common.Ads.Interfaces;
using Common.Events;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.Flow.Stage;
using VContainer.Unity;

namespace Game.Ads.Controllers
{
    /// <summary>
    /// Drives interstitial ad frequency by counting game events and showing an interstitial
    /// during the retry or restart transition when the frequency gate has been satisfied.
    ///
    /// Counting: increments the attempt counter on every level failure or stage completion.
    /// Showing:  when the player taps Retry or Restart, checks <see cref="IAdService.CanShowAd"/>
    ///           and fires-and-forgets <see cref="IAdService.ShowInterstitialAsync"/> so the
    ///           interstitial appears while the scene transition begins without blocking the
    ///           retry / restart event dispatch chain.
    /// </summary>
    public class InterstitialAdTrigger : IStartable, IDisposable
    {
        private readonly IAdService _AdService;
        private readonly InterstitialPlacement _Placement;
        private readonly LevelFailedEventChannel _LevelFailedChannel;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly VoidEventChannel _RetryChannel;
        private readonly VoidEventChannel _RestartChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterstitialAdTrigger"/> class.
        /// </summary>
        public InterstitialAdTrigger(
            IAdService adService,
            InterstitialPlacement placement,
            LevelFailedEventChannel levelFailedChannel,
            StageCompletedEventChannel stageCompletedChannel,
            VoidEventChannel retryChannel,
            VoidEventChannel restartChannel)
        {
            _AdService = adService;
            _Placement = placement;
            _LevelFailedChannel = levelFailedChannel;
            _StageCompletedChannel = stageCompletedChannel;
            _RetryChannel = retryChannel;
            _RestartChannel = restartChannel;
        }

        /// <summary>
        /// Subscribes to the counting events (fail, stage complete) and the transition triggers
        /// (retry, restart) that can show an interstitial.
        /// </summary>
        public void Start()
        {
            _LevelFailedChannel.Subscribe(OnLevelFailed);
            _StageCompletedChannel.Subscribe(OnStageCompleted);
            _RetryChannel.Subscribe(OnTransition);
            _RestartChannel.Subscribe(OnTransition);
        }

        /// <summary>
        /// Unsubscribes from all event channels to prevent stale callbacks after scope disposal.
        /// </summary>
        public void Dispose()
        {
            _LevelFailedChannel.Unsubscribe(OnLevelFailed);
            _StageCompletedChannel.Unsubscribe(OnStageCompleted);
            _RetryChannel.Unsubscribe(OnTransition);
            _RestartChannel.Unsubscribe(OnTransition);
        }

        private void OnLevelFailed(LevelData _) =>
            _AdService.IncrementAttemptCounter(_Placement);

        private void OnStageCompleted(StageData _) =>
            _AdService.IncrementAttemptCounter(_Placement);

        private async void OnTransition()
        {
            if (!_AdService.CanShowAd(_Placement)) return;

            await _AdService.ShowInterstitialAsync(_Placement);
        }
    }
}
