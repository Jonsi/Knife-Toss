using Cysharp.Threading.Tasks;
using Game.Store.Data;

namespace Game.Store.Interfaces
{
    /// <summary>
    /// Store purchase router that delegates between soft-currency and IAP flows.
    /// </summary>
    public interface IStorePurchaseRouter
    {
        bool IsPurchaseInProgress { get; }
        bool CanPurchase(PurchasableEntry entry);
        UniTask<PurchaseResult> TryPurchaseAsync(PurchasableEntry entry);
        UniTask<int> RestorePurchasesAsync();
    }
}
