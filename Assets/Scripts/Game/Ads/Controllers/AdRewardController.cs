using System;
using Common.Events;
using Game.Ads.Data;
using Game.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.Flow.Stage;
using Game.Level.Interfaces;
using Game.UI.Popups;
using VContainer.Unity;

namespace Game.Ads.Controllers
{
    /// <summary>
    /// Orchestrates the paid "Continue after failure" flow.
    /// Spends the configured per-level coin cost and retriggers the current level without resetting stage progress.
    /// </summary>
    public class AdRewardController : IStartable, IDisposable
    {
        private readonly VoidEventChannel _ContinueRequestedChannel;
        private readonly VoidEventChannel _RetryLevelChannel;
        private readonly VoidEventChannel _RestartStageChannel;
        private readonly LevelFailedEventChannel _LevelFailedChannel;
        private readonly CurrencyBalanceChangedEventChannel _CurrencyBalanceChangedChannel;
        private readonly ICurrencyService _CurrencyService;
        private readonly ILevelCatalog _LevelCatalog;
        private readonly StageData _StageData;
        private readonly RewardedPlacement _ContinuePlacement;
        private readonly StageFailPopup _FailPopup;
        private bool _HasConsumedContinue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdRewardController"/> class.
        /// </summary>
        public AdRewardController(
            VoidEventChannel continueRequestedChannel,
            VoidEventChannel retryLevelChannel,
            VoidEventChannel restartStageChannel,
            LevelFailedEventChannel levelFailedChannel,
            CurrencyBalanceChangedEventChannel currencyBalanceChangedChannel,
            ICurrencyService currencyService,
            ILevelCatalog levelCatalog,
            StageData stageData,
            RewardedPlacement continuePlacement,
            StageFailPopup failPopup)
        {
            _ContinueRequestedChannel = continueRequestedChannel;
            _RetryLevelChannel = retryLevelChannel;
            _RestartStageChannel = restartStageChannel;
            _LevelFailedChannel = levelFailedChannel;
            _CurrencyBalanceChangedChannel = currencyBalanceChangedChannel;
            _CurrencyService = currencyService;
            _LevelCatalog = levelCatalog;
            _StageData = stageData;
            _ContinuePlacement = continuePlacement;
            _FailPopup = failPopup;
        }

        /// <summary>
        /// Subscribes to the continue-requested and level-failed event channels.
        /// </summary>
        public void Start()
        {
            _ContinueRequestedChannel.Subscribe(OnContinueRequested);
            _LevelFailedChannel.Subscribe(OnLevelFailed);
            _RestartStageChannel?.Subscribe(OnRestartStageRequested);
            _CurrencyBalanceChangedChannel?.Subscribe(OnCurrencyBalanceChanged);
        }

        /// <summary>
        /// Unsubscribes from event channels to prevent stale callbacks after scope disposal.
        /// </summary>
        public void Dispose()
        {
            _ContinueRequestedChannel.Unsubscribe(OnContinueRequested);
            _LevelFailedChannel.Unsubscribe(OnLevelFailed);
            _RestartStageChannel?.Unsubscribe(OnRestartStageRequested);
            _CurrencyBalanceChangedChannel?.Unsubscribe(OnCurrencyBalanceChanged);
        }

        private void OnLevelFailed(LevelData _)
        {
            RefreshContinueState();
        }

        private void OnContinueRequested()
        {
            int cost = GetCurrentContinueCost();
            if (!CanContinueAfterFailure(cost))
            {
                RefreshContinueState();
                return;
            }

            if (cost > 0 && !_CurrencyService.TrySpend(CoinCurrencyId, cost))
            {
                RefreshContinueState();
                return;
            }

            ConsumeContinue();
            _RetryLevelChannel.Raise();
        }

        private void OnRestartStageRequested()
        {
            _HasConsumedContinue = false;
        }

        private void OnCurrencyBalanceChanged(CurrencyBalanceChangedEventData data)
        {
            if (data.CurrencyId != CoinCurrencyId)
            {
                return;
            }

            RefreshContinueState();
        }

        private void RefreshContinueState()
        {
            int cost = GetCurrentContinueCost();
            bool visible = !HasConsumedContinueAccess();

            _FailPopup.SetContinueState(visible, visible, cost);
        }

        private bool CanContinueAfterFailure(int cost) =>
            !HasConsumedContinueAccess() && CanAfford(cost);

        private bool CanAfford(int cost) =>
            cost <= 0 || _CurrencyService.CanAfford(CoinCurrencyId, cost);

        private int GetCurrentContinueCost()
        {
            return _LevelCatalog
                .GetStageConfig(_StageData.StageIndex)?
                .GetContinueCost(_StageData.CurrentLevelIndex) ?? 0;
        }

        private bool HasConsumedContinueAccess() =>
            _ContinuePlacement != null && _ContinuePlacement.OncePerRun && _HasConsumedContinue;

        private void ConsumeContinue()
        {
            if (_ContinuePlacement != null && _ContinuePlacement.OncePerRun)
            {
                _HasConsumedContinue = true;
            }

            RefreshContinueState();
        }

        private static readonly string CoinCurrencyId = CurrencyType.Coin.ToTypeId();
    }
}
