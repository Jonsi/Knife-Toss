using Common.Events;
using UnityEngine;

namespace Game.Economy.Events
{
    /// <summary>
    /// Event channel for currency balance changes.
    /// </summary>
    [CreateAssetMenu(fileName = "CurrencyBalanceChangedEventChannel", menuName = "Knife Toss/Events/Economy/Currency Balance Changed")]
    public class CurrencyBalanceChangedEventChannel : EventChannel<CurrencyBalanceChangedEventData>
    {
    }
}
