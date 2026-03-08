using Common.Ads.Data;
using Common.Ads.Interfaces;

namespace Common.Ads.Services
{
    /// <summary>
    /// No-op implementation of <see cref="IAdTelemetryListener"/>.
    /// Used when no analytics listener is registered (pre-Phase-3, unit tests).
    /// </summary>
    public class NullAdTelemetryListener : IAdTelemetryListener
    {
        /// <inheritdoc/>
        public void OnAdRequested(AdPlacement placement) { }

        /// <inheritdoc/>
        public void OnAdShown(AdPlacement placement) { }

        /// <inheritdoc/>
        public void OnAdCompleted(AdPlacement placement, AdResult result) { }

        /// <inheritdoc/>
        public void OnAdFailed(AdPlacement placement, string error) { }
    }
}
