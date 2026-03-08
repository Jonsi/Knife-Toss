using Common.Events;
using UnityEngine;

namespace Common.Ads.Events
{
    /// <summary>
    /// Raised when an ad show fails, times out, is cancelled, or is blocked by a gate.
    /// AdEventData.Result carries the specific failure reason.
    /// </summary>
    [CreateAssetMenu(fileName = "AdFailedEventChannel", menuName = "Knife Toss/Events/Ads/Ad Failed")]
    public class AdFailedEventChannel : EventChannel<AdEventData> { }
}
