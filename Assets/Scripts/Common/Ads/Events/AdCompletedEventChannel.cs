using Common.Events;
using UnityEngine;

namespace Common.Ads.Events
{
    /// <summary>Raised when an ad show completes successfully (AdResult.Success is true).</summary>
    [CreateAssetMenu(fileName = "AdCompletedEventChannel", menuName = "Knife Toss/Events/Ads/Ad Completed")]
    public class AdCompletedEventChannel : EventChannel<AdEventData> { }
}
