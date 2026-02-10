using Common.Events;
using UnityEngine;

namespace Game.Store.Events
{
    /// <summary>
    /// Event channel raised when purchase flow starts.
    /// </summary>
    [CreateAssetMenu(fileName = "PurchaseStartedEventChannel", menuName = "Knife Toss/Events/Store/Purchase Started")]
    public class PurchaseStartedEventChannel : EventChannel<PurchaseStartedEventData>
    {
    }
}
