using Common.Events;
using UnityEngine;

namespace Common.Ads.Events
{
    /// <summary>
    /// Raised when an ad show is requested and all gates pass, immediately before the provider is called.
    /// AdEventData.Result is default at this point — the outcome is not yet known.
    /// </summary>
    [CreateAssetMenu(fileName = "AdRequestedEventChannel", menuName = "Knife Toss/Events/Ads/Ad Requested")]
    public class AdRequestedEventChannel : EventChannel<AdEventData> { }
}
