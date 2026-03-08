using Common.Ads.Data;

namespace Common.Ads.Events
{
    /// <summary>
    /// Immutable data payload carried by all ad event channels.
    /// For AdRequestedEventChannel, Result is default (outcome not yet known).
    /// </summary>
    public readonly struct AdEventData
    {
        /// <summary>The placement that triggered the ad event.</summary>
        public AdPlacement Placement { get; }

        /// <summary>The outcome of the ad interaction. Default for requested events.</summary>
        public AdResult Result { get; }

        public AdEventData(AdPlacement placement, AdResult result)
        {
            Placement = placement;
            Result = result;
        }
    }
}
