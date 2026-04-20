using System;
using UnityEngine;

namespace Common.IAP.Data
{
    /// <summary>
    /// Serializable mock result descriptor for simulated purchases.
    /// </summary>
    [Serializable]
    public struct IapMockOutcome
    {
        [field: SerializeField] public IapMockOutcomeType Type { get; private set; }
        [field: SerializeField] public string FailureReason { get; private set; }

        public static IapMockOutcome Success => new IapMockOutcome(IapMockOutcomeType.Success, null);
        public static IapMockOutcome Cancelled => new IapMockOutcome(IapMockOutcomeType.Cancelled, null);
        public static IapMockOutcome Deferred => new IapMockOutcome(IapMockOutcomeType.Deferred, null);
        public static IapMockOutcome AlreadyOwned => new IapMockOutcome(IapMockOutcomeType.AlreadyOwned, null);
        public static IapMockOutcome Failed(string reason) => new IapMockOutcome(IapMockOutcomeType.Failed, reason);

        public IapMockOutcome(IapMockOutcomeType type, string failureReason)
        {
            Type = type;
            FailureReason = failureReason;
        }

        public IapPurchaseResult ToPurchaseResult(string receipt)
        {
            return Type switch
            {
                IapMockOutcomeType.Success => IapPurchaseResult.Purchased(receipt),
                IapMockOutcomeType.Cancelled => IapPurchaseResult.CancelledByUser,
                IapMockOutcomeType.Deferred => IapPurchaseResult.DeferredByStore,
                IapMockOutcomeType.AlreadyOwned => IapPurchaseResult.ProductAlreadyOwned,
                IapMockOutcomeType.Failed => IapPurchaseResult.Failed(FailureReason),
                _ => IapPurchaseResult.Failed("Unknown purchase outcome.")
            };
        }
    }
}
