namespace Game.Store.Data
{
    /// <summary>
    /// Result payload for purchase attempts.
    /// </summary>
    public readonly struct PurchaseResult
    {
        public bool Success { get; }
        public string FailureReason { get; }

        private PurchaseResult(bool success, string failureReason)
        {
            Success = success;
            FailureReason = failureReason;
        }

        public static PurchaseResult Succeeded => new PurchaseResult(true, null);
        public static PurchaseResult InsufficientFunds => new PurchaseResult(false, "Insufficient funds.");
        public static PurchaseResult InvalidItem => new PurchaseResult(false, "Invalid item.");
        public static PurchaseResult AlreadyOwned => new PurchaseResult(false, "Item already owned.");
        public static PurchaseResult PurchaseInProgress => new PurchaseResult(false, "Purchase already in progress.");
        public static PurchaseResult Cancelled => new PurchaseResult(false, "Purchase cancelled.");

        public static PurchaseResult Error(string message)
        {
            return new PurchaseResult(false, string.IsNullOrEmpty(message) ? "Purchase failed." : message);
        }
    }
}
