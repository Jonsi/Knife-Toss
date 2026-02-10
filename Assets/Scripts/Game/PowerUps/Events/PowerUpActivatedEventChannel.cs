using Common.Events;
using Game.PowerUps.Data;
using UnityEngine;

namespace Game.PowerUps.Events
{
    /// <summary>
    /// Event channel raised when a power-up is activated.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpActivatedEventChannel", menuName = "Knife Toss/Events/Power Ups/Power Up Activated")]
    public class PowerUpActivatedEventChannel : EventChannel<PowerUpActivatedEventData>
    {
    }
}
