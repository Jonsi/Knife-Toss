using Common.Ads.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Ads.Data;
using Game.Ads.Extensions;
using Game.Feedback.Events;
using Game.Store.Rewards;
using Common.Rewards.Interfaces;
using Game.UI.Store;
using UnityEngine;
using VContainer.Unity;

namespace Game.Ads.Controllers
{
    /// <summary>
    /// Bridges the store's "Watch Ad" CTA to <see cref="IAdService"/>.
    /// On initialization, sets up the <see cref="StoreAdCtaView"/> with the reward display text and click handler.
    /// Manages the button's interactable state based on ad availability and respects cooldown.
    /// On a successful rewarded ad, grants the configured reward via the polymorphic <see cref="Game.Store.Rewards.ItemReward"/> system.
    /// </summary>
    public class StoreAdController : IStartable
    {
        private readonly IAdService _AdService;
        private readonly RewardedPlacement _Placement;
        private readonly IRewardGrantContext _GrantContext;
        private readonly StoreAdCtaView _View;
        private readonly StoreBalanceChipView _BalanceChipView;
        private readonly FeedbackEventChannel _FeedbackEventChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAdController"/> class.
        /// </summary>
        public StoreAdController(
            IAdService adService,
            RewardedPlacement placement,
            IRewardGrantContext grantContext,
            StoreAdCtaView view,
            StoreBalanceChipView balanceChipView,
            FeedbackEventChannel feedbackEventChannel)
        {
            _AdService = adService;
            _Placement = placement;
            _GrantContext = grantContext;
            _View = view;
            _BalanceChipView = balanceChipView;
            _FeedbackEventChannel = feedbackEventChannel;
        }

        /// <summary>
        /// Configures the CTA view and sets initial interactable state.
        /// </summary>
        public void Start()
        {
            _View.Setup(
                _Placement.Reward?.GetDisplayText() ?? string.Empty,
                OnWatchAdClicked,
                _FeedbackEventChannel);
            _View.SetInteractable(_AdService.CanShowAd(_Placement));
        }

        private async void OnWatchAdClicked()
        {
            _View.SetInteractable(false);

            var result = await _AdService.ShowRewardedAsync(_Placement);

            if (result.Success)
            {
                await _Placement.TryGrantRewardAsync(_GrantContext);
                PlayRewardAccumulation();
            }

            _View.ResolveSubmission(result.Success);

            _View.SetInteractable(_AdService.CanShowAd(_Placement));
        }

        private void PlayRewardAccumulation()
        {
            if (_BalanceChipView == null || _Placement?.Reward is not CurrencyReward currencyReward || currencyReward.Amount <= 0)
            {
                return;
            }

            var source = _View.ActionButton != null ? _View.ActionButton.transform as RectTransform : null;
            _BalanceChipView.PlayAccumulationFromUiElement(source, currencyReward.Amount);
        }
    }
}
