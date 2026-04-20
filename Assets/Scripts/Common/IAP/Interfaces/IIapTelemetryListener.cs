using Common.IAP.Data;

namespace Common.IAP.Interfaces
{
    /// <summary>
    /// Optional analytics callback interface for IAP lifecycle events.
    /// </summary>
    public interface IIapTelemetryListener
    {
        void OnPurchaseStarted(IapProduct product);
        void OnPurchaseCompleted(IapProduct product, IapPurchaseResult result);
        void OnPurchaseFailed(IapProduct product, string reason);
        void OnPurchasesRestored(int restoredCount);
    }
}
