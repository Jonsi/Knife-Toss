using Common.Events;
using Game.PowerUps.Data;
using UnityEngine;

namespace Game.PowerUps.Events
{
    [CreateAssetMenu(fileName = "PowerUpCancelledEventChannel", menuName = "Knife Toss/Events/Power Ups/Power Up Cancelled")]
    public class PowerUpCancelledEventChannel : EventChannel<PowerUpCancelledEventData>
    {
    }
}
