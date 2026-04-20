using Common.IAP.Data;
using Common.IAP.Interfaces;
using Game.Persistence.Services;

namespace Game.IAP.Services
{
    /// <summary>
    /// IIapEntitlementStore backed by persisted player data.
    /// </summary>
    public class PlayerDataIapEntitlementStore : IIapEntitlementStore
    {
        private readonly PlayerDataService _PlayerDataService;

        public PlayerDataIapEntitlementStore(PlayerDataService playerDataService)
        {
            _PlayerDataService = playerDataService;
        }

        public bool HasEntitlement(IapProduct product)
        {
            if (product == null || string.IsNullOrEmpty(product.ProductKey))
                return false;

            return _PlayerDataService.EnsureIapEntitlements().IsOwned(product.ProductKey);
        }

        public void SetOwned(IapProduct product, bool owned)
        {
            if (product == null || string.IsNullOrEmpty(product.ProductKey))
                return;

            var changed = _PlayerDataService.EnsureIapEntitlements().SetOwned(product.ProductKey, owned);
            if (changed)
                _PlayerDataService.SaveIapEntitlements();
        }
    }
}
