#if TEST_ENABLED
using System.ComponentModel;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Common.IAP.Providers;
using Game.Store.Data;
using Game.Store.Rewards;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// SRDebugger tab for mock IAP flow and Remove Ads entitlement testing.
    /// </summary>
    public class IapCheatsTab : OptionsTab
    {
        public override string Key => "IapCheats";
        public override string Title => "6. IAP";
        public override string IconStyleKey => "Icon_Settings";
        public override int SortIndex => -105;

        [Category("State")]
        [DisplayName("IAP Enabled")]
        public bool IapEnabled => TryResolve<IapSettings>(CurrentResolver)?.IapEnabled ?? false;

        [Category("State")]
        [DisplayName("Remove Ads Owned")]
        public bool RemoveAdsOwned
        {
            get
            {
                var product = FindRemoveAdsProduct();
                return product != null && TryResolve<IIapEntitlementStore>(CurrentResolver)?.HasEntitlement(product) == true;
            }
            set
            {
                var product = FindRemoveAdsProduct();
                if (product == null)
                    return;

                TryResolve<IIapEntitlementStore>(CurrentResolver)?.SetOwned(product, value);
                TryResolve<MockIapProvider>(CurrentResolver)?.SetOwnedForDebug(product, value);
                RequestRefresh();
            }
        }

        [Category("Actions")]
        [DisplayName("Restore Purchases")]
        public async void RestorePurchases()
        {
            var service = TryResolve<IIapService>(CurrentResolver);
            if (service == null)
                return;

            await service.RestorePurchasesAsync();
            RequestRefresh();
        }

        [Category("Force Outcome")]
        [DisplayName("Force: Success")]
        public void ForceSuccess() =>
            TryResolve<MockIapProvider>(CurrentResolver)?.ForceNextOutcome(IapMockOutcome.Success);

        [Category("Force Outcome")]
        [DisplayName("Force: Cancelled")]
        public void ForceCancelled() =>
            TryResolve<MockIapProvider>(CurrentResolver)?.ForceNextOutcome(IapMockOutcome.Cancelled);

        [Category("Force Outcome")]
        [DisplayName("Force: Deferred")]
        public void ForceDeferred() =>
            TryResolve<MockIapProvider>(CurrentResolver)?.ForceNextOutcome(IapMockOutcome.Deferred);

        [Category("Force Outcome")]
        [DisplayName("Force: Failed")]
        public void ForceFailed() =>
            TryResolve<MockIapProvider>(CurrentResolver)?.ForceNextOutcome(IapMockOutcome.Failed("Forced failure."));

        private IapProduct FindRemoveAdsProduct()
        {
            var storeCatalog = TryResolve<StoreCatalog>(CurrentResolver);
            if (storeCatalog?.IapEntries == null)
                return null;

            for (var i = 0; i < storeCatalog.IapEntries.Count; i++)
            {
                var entry = storeCatalog.IapEntries[i];
                if (entry?.Reward is not RemoveAdsReward)
                    continue;

                var product = entry.GetIapProduct();
                if (product != null)
                {
                    return product;
                }
            }

            return null;
        }
    }
}
#endif
