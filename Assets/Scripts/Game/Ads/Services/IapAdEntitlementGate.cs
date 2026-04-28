using Common.Ads.Interfaces;
using Common.IAP.Interfaces;
using Game.IAP.Services;
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
        private bool? _CachedResult;

        public IapAdEntitlementGate(IIapEntitlementStore entitlementStore, StoreCatalog storeCatalog)
        {
            _EntitlementStore = entitlementStore;
            _StoreCatalog = storeCatalog;

            if (_EntitlementStore is PlayerDataIapEntitlementStore playerDataStore)
                playerDataStore.EntitlementsChanged += Invalidate;
        }

        public bool CanShowInterstitial()
        {
            if (_CachedResult.HasValue)
                return _CachedResult.Value;

            if (_EntitlementStore == null || _StoreCatalog?.IapEntries == null)
                return Cache(true);

            for (var i = 0; i < _StoreCatalog.IapEntries.Count; i++)
            {
                var entry = _StoreCatalog.IapEntries[i];
                if (entry?.Reward is not RemoveAdsReward)
                    continue;

                var product = entry.GetIapProduct();
                if (product != null && _EntitlementStore.HasEntitlement(product))
                    return Cache(false);
            }

            return Cache(true);
        }

        public void Invalidate()
        {
            _CachedResult = null;
        }

        private bool Cache(bool result)
        {
            _CachedResult = result;
            return result;
        }
    }
}
