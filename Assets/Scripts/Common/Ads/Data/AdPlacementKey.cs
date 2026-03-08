namespace Common.Ads.Data
{
    /// <summary>
    /// Identifies each distinct ad surface in the game.
    /// Used as dictionary keys for cooldown, frequency, and once-per-run tracking.
    /// </summary>
    public enum AdPlacementKey
    {
        ContinueAfterFail,
        StoreRewarded,
        InterstitialRetry
    }
}
