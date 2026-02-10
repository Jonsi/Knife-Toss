using Common.Events;
using UnityEngine;

namespace Game.Store.Events
{
    /// <summary>
    /// Event channel raised when purchase flow fails.
    /// </summary>
    [CreateAssetMenu(fileName = "PurchaseFailedEventChannel", menuName = "Knife Toss/Events/Store/Purchase Failed")]
    public class PurchaseFailedEventChannel : EventChannel<PurchaseFailedEventData>
    {
    }
}
