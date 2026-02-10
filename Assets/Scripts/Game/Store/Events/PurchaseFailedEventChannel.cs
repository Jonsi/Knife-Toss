using Common.Events;
using UnityEngine;

namespace Game.Store.Events
{
    /// <summary>
    /// Event channel raised when purchase flow fails.
    /// </summary>
    [CreateAssetMenu(fileName = "PurchaseFailedEventChannel", menuName = "Knife Toss/Events/Purchase Failed Event Channel")]
    public class PurchaseFailedEventChannel : EventChannel<PurchaseFailedEventData>
    {
    }
}
