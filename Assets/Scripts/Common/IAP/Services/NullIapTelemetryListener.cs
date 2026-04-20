using Common.IAP.Data;
using Common.IAP.Interfaces;

namespace Common.IAP.Services
{
    /// <summary>
    /// No-op telemetry listener for builds without analytics wiring.
    /// </summary>
    public class NullIapTelemetryListener : IIapTelemetryListener
    {
        public void OnPurchaseStarted(IapProduct product) { }
        public void OnPurchaseCompleted(IapProduct product, IapPurchaseResult result) { }
        public void OnPurchaseFailed(IapProduct product, string reason) { }
        public void OnPurchasesRestored(int restoredCount) { }
    }
}
