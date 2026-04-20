using Common.IAP.Data;

namespace Common.IAP.Interfaces
{
    /// <summary>
    /// Persists product ownership for non-consumable IAP entitlements.
    /// </summary>
    public interface IIapEntitlementStore
    {
        bool HasEntitlement(IapProduct product);
        void SetOwned(IapProduct product, bool owned);
    }
}
