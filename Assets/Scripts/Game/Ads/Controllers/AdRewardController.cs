using System;
using Common.Ads.Interfaces;
using Common.Events;
using Cysharp.Threading.Tasks;
using Game.Ads.Data;
using Game.Ads.Extensions;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.UI.Popups;
using VContainer;
using VContainer.Unity;

namespace Game.Ads.Controllers
{
    /// <summary>
    /// Orchestrates the rewarded-ad "Continue after failure" flow.
    /// Listens for the player requesting a continue, shows a rewarded ad via <see cref="IAdService"/>,
    /// and on success grants any configured reward and triggers a level retry
    /// using the existing retry channel — which reloads the current level without resetting stage progress.
    /// On ad failure (e.g. user taps Fail, network error), triggers stage restart so the player
    /// does not continue from the same level.
    /// Also refreshes the continue button's availability each time the fail popup appears.
    /// </summary>
    public class AdRewardController : IStartable, IDisposable
    {
        private readonly IAdService _AdService;
        private readonly RewardedPlacement _ContinuePlacement;
        private readonly IObjectResolver _Resolver;
        private readonly VoidEventChannel _ContinueRequestedChannel;
        private readonly VoidEventChannel _RetryLevelChannel;
        private readonly VoidEventChannel _RestartStageChannel;
        private readonly LevelFailedEventChannel _LevelFailedChannel;
        private readonly StageFailPopup _FailPopup;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdRewardController"/> class.
        /// </summary>
        public AdRewardController(
            IAdService adService,
            RewardedPlacement continuePlacement,
            IObjectResolver resolver,
            VoidEventChannel continueRequestedChannel,
            VoidEventChannel retryLevelChannel,
            VoidEventChannel restartStageChannel,
            LevelFailedEventChannel levelFailedChannel,
            StageFailPopup failPopup)
        {
            _AdService = adService;
            _ContinuePlacement = continuePlacement;
            _Resolver = resolver;
            _ContinueRequestedChannel = continueRequestedChannel;
            _RetryLevelChannel = retryLevelChannel;
            _RestartStageChannel = restartStageChannel;
            _LevelFailedChannel = levelFailedChannel;
            _FailPopup = failPopup;
        }

        /// <summary>
        /// Subscribes to the continue-requested and level-failed event channels.
        /// </summary>
        public void Start()
        {
            _ContinueRequestedChannel.Subscribe(OnContinueRequested);
            _LevelFailedChannel.Subscribe(OnLevelFailed);
        }

        /// <summary>
        /// Unsubscribes from event channels to prevent stale callbacks after scope disposal.
        /// </summary>
        public void Dispose()
        {
            _ContinueRequestedChannel.Unsubscribe(OnContinueRequested);
            _LevelFailedChannel.Unsubscribe(OnLevelFailed);
        }

        private void OnLevelFailed(LevelData _)
        {
            _FailPopup.SetContinueAvailable(_AdService.CanShowAd(_ContinuePlacement));
        }

        private async void OnContinueRequested()
        {
            if (!_AdService.CanShowAd(_ContinuePlacement))
            {
                _FailPopup.SetContinueAvailable(false);
                return;
            }

            _FailPopup.SetContinueAvailable(false);

            var result = await _AdService.ShowRewardedAsync(_ContinuePlacement);

            if (result.Success)
            {
                await _ContinuePlacement.TryGrantRewardAsync(_Resolver);
                _RetryLevelChannel.Raise();
            }
            else
            {
                _RestartStageChannel?.Raise();
            }
        }
    }
}
