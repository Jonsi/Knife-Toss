using System;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Store.Data;
using UnityEngine;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Marker reward for Remove Ads entitlement.
    /// Entitlement persistence is handled by IIapService and IIapEntitlementStore.
    /// </summary>
    [Serializable]
    public class RemoveAdsReward : ItemReward, IUniqueReward
    {
        public string AlreadyOwnedReason => "Ads already removed.";

        public bool IsOwned(IRewardGrantContext context)
        {
            IIapEntitlementStore entitlementStore = null;
            try
            {
                entitlementStore = context.Resolve<IIapEntitlementStore>();
                var storeCatalog = context.Resolve<StoreCatalog>();
                var product = FindConfiguredProduct(storeCatalog);
                return entitlementStore != null && product != null && entitlementStore.HasEntitlement(product);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"Unable to resolve Remove Ads entitlement state: {ex.Message}");
#endif
                return false;
            }
        }

        public override UniTask GrantAsync(IRewardGrantContext context)
        {
            return UniTask.CompletedTask;
        }

        public override string GetDisplayText()
        {
            return "Remove Ads";
        }

        private IapProduct FindConfiguredProduct(StoreCatalog storeCatalog)
        {
            if (storeCatalog?.IapEntries == null)
                return null;

            for (var i = 0; i < storeCatalog.IapEntries.Count; i++)
            {
                var entry = storeCatalog.IapEntries[i];
                if (!ReferenceEquals(entry?.Reward, this))
                    continue;

                return entry.GetIapProduct();
            }

            return null;
        }
    }
}
