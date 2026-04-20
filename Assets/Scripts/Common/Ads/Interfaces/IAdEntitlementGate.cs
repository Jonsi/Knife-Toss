namespace Common.Ads.Interfaces
{
    /// <summary>
    /// Optional gate for entitlement-based ad suppression.
    /// </summary>
    public interface IAdEntitlementGate
    {
        bool CanShowInterstitial();
    }
}
