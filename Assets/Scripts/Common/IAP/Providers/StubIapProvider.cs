using System;
using System.Collections.Generic;
using System.Threading;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Cysharp.Threading.Tasks;

namespace Common.IAP.Providers
{
    /// <summary>
    /// Non-test fallback provider so non-TEST builds don't depend on mock classes.
    /// </summary>
    public class StubIapProvider : IIapProvider
    {
        public UniTask InitializeAsync(IapCatalog catalog, CancellationToken ct = default)
        {
            return UniTask.CompletedTask;
        }

        public UniTask<IapPurchaseResult> PurchaseAsync(IapProduct product, CancellationToken ct = default)
        {
            return UniTask.FromResult(IapPurchaseResult.Failed("IAP provider is not configured for this build."));
        }

        public UniTask<IReadOnlyList<IapProduct>> RestorePurchasesAsync(CancellationToken ct = default)
        {
            IReadOnlyList<IapProduct> empty = Array.Empty<IapProduct>();
            return UniTask.FromResult(empty);
        }

        public UniTask<IapProductInfo> GetProductInfoAsync(IapProduct product, CancellationToken ct = default)
        {
            return UniTask.FromResult(new IapProductInfo(product, false, false, string.Empty));
        }

        public void Dispose()
        {
        }
    }
}
