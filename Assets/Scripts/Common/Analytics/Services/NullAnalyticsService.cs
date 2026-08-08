using Common.Analytics.Interfaces;

namespace Common.Analytics.Services
{
    /// <summary>
    /// Silent non-test fallback used until a production analytics adapter is configured.
    /// </summary>
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public void TrackEvent(AnalyticsEvent analyticsEvent)
        {
        }
    }
}
