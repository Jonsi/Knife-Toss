using Common.Events;
using Game.PowerUps.Data;
using UnityEngine;

namespace Game.PowerUps.Events
{
    /// <summary>
    /// Event channel raised when a power-up is consumed.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpUsedEventChannel", menuName = "Knife Toss/Power Ups/Power Up Used Event Channel")]
    public class PowerUpUsedEventChannel : EventChannel<PowerUpUsedEventData>
    {
    }
}
