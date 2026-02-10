using Common.Events;
using Game.PowerUps.Data;
using UnityEngine;

namespace Game.PowerUps.Events
{
    /// <summary>
    /// Event channel raised when a power-up is consumed.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpUsedEventChannel", menuName = "Knife Toss/Events/Power Ups/Power Up Used")]
    public class PowerUpUsedEventChannel : EventChannel<PowerUpUsedEventData>
    {
    }
}
