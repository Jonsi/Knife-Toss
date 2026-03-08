using Common.Ads.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Ads.Data;
using Game.Ads.Extensions;
using Game.UI.Store;
using VContainer;
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
        private readonly IObjectResolver _Resolver;
        private readonly StoreAdCtaView _View;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAdController"/> class.
        /// </summary>
        public StoreAdController(
            IAdService adService,
            RewardedPlacement placement,
            IObjectResolver resolver,
            StoreAdCtaView view)
        {
            _AdService = adService;
            _Placement = placement;
            _Resolver = resolver;
            _View = view;
        }

        /// <summary>
        /// Configures the CTA view and sets initial interactable state.
        /// </summary>
        public void Start()
        {
            _View.Setup(_Placement.Reward?.GetDisplayText() ?? string.Empty, OnWatchAdClicked);
            _View.SetInteractable(_AdService.CanShowAd(_Placement));
        }

        private async void OnWatchAdClicked()
        {
            _View.SetInteractable(false);

            var result = await _AdService.ShowRewardedAsync(_Placement);

            if (result.Success)
                await _Placement.TryGrantRewardAsync(_Resolver);

            _View.SetInteractable(_AdService.CanShowAd(_Placement));
        }
    }
}
