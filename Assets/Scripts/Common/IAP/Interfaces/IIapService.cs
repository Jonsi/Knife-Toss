using System.Collections.Generic;
using System.Threading;
using Common.IAP.Data;
using Cysharp.Threading.Tasks;

namespace Common.IAP.Interfaces
{
    /// <summary>
    /// High-level facade for real-money purchases and restore operations.
    /// </summary>
    public interface IIapService
    {
        bool IsPurchaseInProgress { get; }
        bool IsOwned(IapProduct product);
        UniTask<IapPurchaseResult> PurchaseAsync(IapProduct product, CancellationToken ct = default);
        UniTask<IReadOnlyList<IapProduct>> RestorePurchasesAsync(CancellationToken ct = default);
        UniTask<IapProductInfo> GetProductInfoAsync(IapProduct product, CancellationToken ct = default);
    }
}
