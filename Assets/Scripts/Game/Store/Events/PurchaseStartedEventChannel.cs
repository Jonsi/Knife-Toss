using Common.Events;
using UnityEngine;

namespace Game.Store.Events
{
    /// <summary>
    /// Event channel raised when purchase flow starts.
    /// </summary>
    [CreateAssetMenu(fileName = "PurchaseStartedEventChannel", menuName = "Knife Toss/Events/Purchase Started Event Channel")]
    public class PurchaseStartedEventChannel : EventChannel<PurchaseStartedEventData>
    {
    }
}
