using System.Collections.Generic;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Store.Data;
using Game.Store.Rewards;

namespace Game.IAP.Services
{
    /// <summary>
    /// Encapsulates Remove Ads purchase flow from Main Menu.
    /// </summary>
    public class MainMenuRemoveAdsFlow
    {
        private readonly IIapService _IapService;
        private readonly IAnalyticsService _AnalyticsService;
        private readonly IapProduct _RemoveAdsProduct;

        public MainMenuRemoveAdsFlow(
            IIapService iapService,
            StoreCatalog storeCatalog,
            IAnalyticsService analyticsService)
        {
            _IapService = iapService;
            _AnalyticsService = analyticsService;
            _RemoveAdsProduct = ResolveRemoveAdsProduct(storeCatalog);
        }

        public bool IsAvailable => _RemoveAdsProduct != null;

        public bool CanPurchase =>
            _IapService != null &&
            _RemoveAdsProduct != null &&
            !_IapService.IsPurchaseInProgress &&
            !_IapService.IsOwned(_RemoveAdsProduct);

        public async UniTask TryPurchaseAsync()
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent("main_menu_remove_ads_clicked"));

            if (_IapService == null || _RemoveAdsProduct == null)
            {
                _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                    "main_menu_remove_ads_unavailable",
                    new Dictionary<string, object>
                    {
                        ["iap_service_available"] = _IapService != null,
                        ["product_available"] = _RemoveAdsProduct != null
                    }));
                return;
            }

            if (_IapService.IsPurchaseInProgress)
            {
                _AnalyticsService?.TrackEvent(new AnalyticsEvent("main_menu_remove_ads_purchase_in_progress"));
                return;
            }

            if (_IapService.IsOwned(_RemoveAdsProduct))
            {
                _AnalyticsService?.TrackEvent(new AnalyticsEvent("main_menu_remove_ads_already_owned"));
                return;
            }

            try
            {
                var result = await _IapService.PurchaseAsync(_RemoveAdsProduct);
                _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                    "main_menu_remove_ads_purchase_result",
                    new Dictionary<string, object>
                    {
                        ["success"] = result.Success,
                        ["cancelled"] = result.Cancelled,
                        ["deferred"] = result.Deferred,
                        ["already_owned"] = result.AlreadyOwned,
                        ["failure_reason"] = result.FailureReason ?? string.Empty
                    }));
            }
            catch (System.Exception exception)
            {
                _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                    "main_menu_remove_ads_purchase_exception",
                    new Dictionary<string, object>
                    {
                        ["message"] = exception.Message
                    }));
            }
        }

        private static IapProduct ResolveRemoveAdsProduct(StoreCatalog storeCatalog)
        {
            if (storeCatalog?.IapEntries == null)
            {
                return null;
            }

            for (var i = 0; i < storeCatalog.IapEntries.Count; i++)
            {
                var entry = storeCatalog.IapEntries[i];
                if (entry?.Reward is not RemoveAdsReward removeAdsReward)
                {
                    continue;
                }

                var product = removeAdsReward.Product ?? entry.Price?.IapProduct;
                if (product != null)
                {
                    return product;
                }
            }

            return null;
        }
    }
}
