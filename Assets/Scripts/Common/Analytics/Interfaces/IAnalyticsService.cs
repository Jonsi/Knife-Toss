using Common.Analytics;

namespace Common.Analytics.Interfaces
{
    /// <summary>
    /// Tracks analytics events through an interchangeable backend implementation.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Sends a single analytics event.
        /// </summary>
        void TrackEvent(AnalyticsEvent analyticsEvent);
    }
}
