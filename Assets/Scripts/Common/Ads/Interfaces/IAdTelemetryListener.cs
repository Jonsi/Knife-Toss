using Common.Ads.Data;

namespace Common.Ads.Interfaces
{
    /// <summary>
    /// Optional analytics callback interface injected into AdService.
    /// Implement in game code (Phase 2) or with Firebase Analytics (Phase 3).
    /// </summary>
    public interface IAdTelemetryListener
    {
        /// <summary>Called when an ad show is requested before the provider is contacted.</summary>
        void OnAdRequested(AdPlacement placement);

        /// <summary>Called immediately before the provider begins displaying the ad.</summary>
        void OnAdShown(AdPlacement placement);

        /// <summary>Called when an ad show resolves successfully or with a non-failure outcome.</summary>
        void OnAdCompleted(AdPlacement placement, AdResult result);

        /// <summary>Called when an ad show fails, times out, or is blocked by a gate.</summary>
        void OnAdFailed(AdPlacement placement, string error);
    }
}
