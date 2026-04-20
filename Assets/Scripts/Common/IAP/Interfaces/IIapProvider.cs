using System.Collections.Generic;
using System.Threading;
using Common.IAP.Data;
using Cysharp.Threading.Tasks;

namespace Common.IAP.Interfaces
{
    /// <summary>
    /// Low-level store SDK adapter contract (or mock provider).
    /// </summary>
    public interface IIapProvider
    {
        UniTask InitializeAsync(IapCatalog catalog, CancellationToken ct = default);
        UniTask<IapPurchaseResult> PurchaseAsync(IapProduct product, CancellationToken ct = default);
        UniTask<IReadOnlyList<IapProduct>> RestorePurchasesAsync(CancellationToken ct = default);
        UniTask<IapProductInfo> GetProductInfoAsync(IapProduct product, CancellationToken ct = default);
        void Dispose();
    }
}
