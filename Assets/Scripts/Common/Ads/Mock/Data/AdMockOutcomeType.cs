#if TEST_ENABLED
namespace Common.Ads.Data
{
    /// <summary>
    /// Defines the possible outcomes a mock ad provider can simulate.
    /// </summary>
    public enum AdMockOutcomeType
    {
        Completed,
        Skipped,
        Unavailable,
        Timeout,
        Failed
    }
}
#endif
