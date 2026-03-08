using System.Threading;
using Common.Ads.Data;
using Cysharp.Threading.Tasks;

namespace Common.Ads.Interfaces
{
    /// <summary>
    /// High-level facade for the ad system consumed by game code.
    /// Encapsulates all policy decisions: cooldown, frequency gating, consent, and availability checks.
    /// </summary>
    public interface IAdService
    {
        /// <summary>Returns true if the provider has a loaded ad ready for the given placement.</summary>
        bool IsAdAvailable(AdPlacement placement);

        /// <summary>
        /// Returns true if policy gates allow showing an ad: globally enabled, not once-per-run consumed,
        /// not on cooldown, and frequency gate satisfied. Does not check provider load state — use
        /// <see cref="IsAdAvailable"/> for that.
        /// </summary>
        bool CanShowAd(AdPlacement placement);

        /// <summary>Shows an interstitial ad and returns the outcome.</summary>
        UniTask<AdResult> ShowInterstitialAsync(AdPlacement placement, CancellationToken ct = default);

        /// <summary>Shows a rewarded ad and returns the outcome including reward eligibility.</summary>
        UniTask<AdResult> ShowRewardedAsync(AdPlacement placement, CancellationToken ct = default);

        /// <summary>Clears all once-per-run flags. Call at session or run start.</summary>
        void ResetRunState();

        /// <summary>Clears all cooldown timestamps and attempt counters. Used by debug tools.</summary>
        void ResetCooldowns();

        /// <summary>
        /// Increments the attempt counter for the given placement without showing an ad.
        /// Call on relevant game events (e.g. stage attempt) to advance the frequency gate.
        /// </summary>
        void IncrementAttemptCounter(AdPlacement placement);
    }
}
