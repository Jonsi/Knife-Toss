using Cysharp.Threading.Tasks;
using Game.Store.Data;

namespace Game.Store.Interfaces
{
    /// <summary>
    /// Coordinates store purchases.
    /// </summary>
    public interface IPurchaseService
    {
        bool IsPurchaseInProgress { get; }
        bool CanPurchase(PurchasableEntry entry);
        UniTask<PurchaseResult> TryPurchaseAsync(PurchasableEntry entry);
    }
}
