using System.Collections.Generic;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Common.IAP.Data;
using Common.IAP.Interfaces;

namespace Game.IAP.Services
{
    /// <summary>
    /// Analytics-backed IAP telemetry listener for purchase lifecycle outcomes.
    /// </summary>
    public class IapAnalyticsTelemetryListener : IIapTelemetryListener
    {
        private readonly IAnalyticsService _AnalyticsService;

        public IapAnalyticsTelemetryListener(IAnalyticsService analyticsService)
        {
            _AnalyticsService = analyticsService;
        }

        public void OnPurchaseStarted(IapProduct product)
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "iap_purchase_started",
                new Dictionary<string, object>
                {
                    ["product_key"] = product?.ProductKey ?? string.Empty,
                    ["store_product_id"] = product?.StoreProductId ?? string.Empty
                }));
        }

        public void OnPurchaseCompleted(IapProduct product, IapPurchaseResult result)
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "iap_purchase_completed",
                new Dictionary<string, object>
                {
                    ["product_key"] = product?.ProductKey ?? string.Empty,
                    ["store_product_id"] = product?.StoreProductId ?? string.Empty,
                    ["success"] = result.Success,
                    ["already_owned"] = result.AlreadyOwned,
                    ["receipt_present"] = !string.IsNullOrEmpty(result.Receipt)
                }));
        }

        public void OnPurchaseFailed(IapProduct product, string reason)
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "iap_purchase_failed",
                new Dictionary<string, object>
                {
                    ["product_key"] = product?.ProductKey ?? string.Empty,
                    ["store_product_id"] = product?.StoreProductId ?? string.Empty,
                    ["reason"] = reason ?? string.Empty
                }));
        }

        public void OnPurchasesRestored(int restoredCount)
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "iap_restore_completed",
                new Dictionary<string, object>
                {
                    ["restored_count"] = restoredCount
                }));
        }
    }
}
