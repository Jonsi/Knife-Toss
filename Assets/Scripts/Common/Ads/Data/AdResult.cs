namespace Common.Ads.Data
{
    /// <summary>
    /// Result payload for all ad show attempts. Mirrors the PurchaseResult pattern.
    /// Use static factory properties for named outcomes; use <see cref="Failed"/> for dynamic error messages.
    /// </summary>
    public readonly struct AdResult
    {
        /// <summary>True when the ad completed successfully and a reward (if any) should be granted.</summary>
        public bool Success { get; }

        /// <summary>Human-readable reason for non-successful outcomes. Null on success.</summary>
        public string FailureReason { get; }

        private AdResult(bool success, string failureReason)
        {
            Success = success;
            FailureReason = failureReason;
        }

        /// <summary>Ad played to completion.</summary>
        public static AdResult Completed => new AdResult(true, null);

        /// <summary>User dismissed the ad before completion.</summary>
        public static AdResult Skipped => new AdResult(false, "Ad skipped by user.");

        /// <summary>No ad was loaded or network is unavailable.</summary>
        public static AdResult Unavailable => new AdResult(false, "No ad available.");

        /// <summary>Blocked by cooldown timer or frequency gate.</summary>
        public static AdResult OnCooldown => new AdResult(false, "Ad on cooldown.");

        /// <summary>Blocked by the global kill switch in AdSettings.</summary>
        public static AdResult Disabled => new AdResult(false, "Ads are disabled.");

        /// <summary>Provider exceeded the configured show deadline.</summary>
        public static AdResult Timeout => new AdResult(false, "Ad request timed out.");

        /// <summary>Provider-level failure with a dynamic reason.</summary>
        public static AdResult Failed(string reason) =>
            new AdResult(false, string.IsNullOrEmpty(reason) ? "Ad failed." : reason);
    }
}
