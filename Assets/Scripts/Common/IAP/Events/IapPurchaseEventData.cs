using System.Collections.Generic;
using Common.IAP.Data;

namespace Common.IAP.Events
{
    /// <summary>
    /// Event payload for IAP purchase lifecycle channels.
    /// </summary>
    public readonly struct IapPurchaseEventData
    {
        public IapProduct Product { get; }
        public IapPurchaseResult Result { get; }
        public IReadOnlyList<IapProduct> RestoredProducts { get; }

        public IapPurchaseEventData(IapProduct product, IapPurchaseResult result, IReadOnlyList<IapProduct> restoredProducts = null)
        {
            Product = product;
            Result = result;
            RestoredProducts = restoredProducts;
        }
    }
}
