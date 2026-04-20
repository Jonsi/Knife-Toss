using Common.Ads.Interfaces;
using Common.IAP.Interfaces;
using Game.Store.Data;
using Game.Store.Rewards;

namespace Game.Ads.Services
{
    /// <summary>
    /// Interstitial eligibility gate backed by owned IAP entitlements.
    /// </summary>
    public class IapAdEntitlementGate : IAdEntitlementGate
    {
        private readonly IIapEntitlementStore _EntitlementStore;
        private readonly StoreCatalog _StoreCatalog;

        public IapAdEntitlementGate(IIapEntitlementStore entitlementStore, StoreCatalog storeCatalog)
        {
            _EntitlementStore = entitlementStore;
            _StoreCatalog = storeCatalog;
        }

        public bool CanShowInterstitial()
        {
            if (_EntitlementStore == null || _StoreCatalog?.IapEntries == null)
                return true;

            for (var i = 0; i < _StoreCatalog.IapEntries.Count; i++)
            {
                var entry = _StoreCatalog.IapEntries[i];
                if (entry?.Reward is not RemoveAdsReward removeAdsReward)
                    continue;

                var product = removeAdsReward.Product ?? entry.Price?.IapProduct;
                if (product != null && _EntitlementStore.HasEntitlement(product))
                    return false;
            }

            return true;
        }
    }
}
