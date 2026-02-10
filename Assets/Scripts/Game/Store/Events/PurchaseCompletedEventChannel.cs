using Common.Events;
using UnityEngine;

namespace Game.Store.Events
{
    /// <summary>
    /// Event channel raised when purchase flow completes.
    /// </summary>
    [CreateAssetMenu(fileName = "PurchaseCompletedEventChannel", menuName = "Knife Toss/Events/Store/Purchase Completed")]
    public class PurchaseCompletedEventChannel : EventChannel<PurchaseCompletedEventData>
    {
    }
}
