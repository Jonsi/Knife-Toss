using System;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
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
        [field: SerializeField] public IapProduct Product { get; private set; }

        public string AlreadyOwnedReason => "Ads already removed.";

        public bool IsOwned(IRewardGrantContext context)
        {
            IIapEntitlementStore entitlementStore = null;
            try
            {
                entitlementStore = context.Resolve<IIapEntitlementStore>();
            }
            catch
            {
                return false;
            }

            return entitlementStore != null && Product != null && entitlementStore.HasEntitlement(Product);
        }

        public override UniTask GrantAsync(IRewardGrantContext context)
        {
            return UniTask.CompletedTask;
        }

        public override string GetDisplayText()
        {
            return "Remove Ads";
        }
    }
}
