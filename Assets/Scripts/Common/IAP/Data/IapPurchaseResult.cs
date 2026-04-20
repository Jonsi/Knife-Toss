namespace Common.IAP.Data
{
    /// <summary>
    /// Result payload for IAP purchase attempts.
    /// </summary>
    public readonly struct IapPurchaseResult
    {
        public bool Success { get; }
        public bool Cancelled { get; }
        public bool Deferred { get; }
        public bool AlreadyOwned { get; }
        public string FailureReason { get; }
        public string Receipt { get; }

        private IapPurchaseResult(bool success, bool cancelled, bool deferred, bool alreadyOwned, string failureReason, string receipt)
        {
            Success = success;
            Cancelled = cancelled;
            Deferred = deferred;
            AlreadyOwned = alreadyOwned;
            FailureReason = failureReason;
            Receipt = receipt;
        }

        public static IapPurchaseResult Purchased(string receipt) => new IapPurchaseResult(true, false, false, false, null, receipt);
        public static IapPurchaseResult Restored(string receipt) => new IapPurchaseResult(true, false, false, false, null, receipt);
        public static IapPurchaseResult CancelledByUser => new IapPurchaseResult(false, true, false, false, "Purchase cancelled.", null);
        public static IapPurchaseResult DeferredByStore => new IapPurchaseResult(false, false, true, false, "Purchase is deferred.", null);
        public static IapPurchaseResult ProductAlreadyOwned => new IapPurchaseResult(false, false, false, true, "Item already owned.", null);
        public static IapPurchaseResult Disabled => new IapPurchaseResult(false, false, false, false, "IAP is disabled.", null);
        public static IapPurchaseResult InProgress => new IapPurchaseResult(false, false, false, false, "Purchase already in progress.", null);
        public static IapPurchaseResult InvalidProduct => new IapPurchaseResult(false, false, false, false, "Invalid product.", null);
        public static IapPurchaseResult Timeout => new IapPurchaseResult(false, false, false, false, "Purchase timed out.", null);

        public static IapPurchaseResult Failed(string reason) =>
            new IapPurchaseResult(false, false, false, false, string.IsNullOrEmpty(reason) ? "Purchase failed." : reason, null);
    }
}
