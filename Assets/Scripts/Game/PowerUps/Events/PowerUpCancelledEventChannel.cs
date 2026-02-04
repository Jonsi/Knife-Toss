using Common.Events;
using Game.PowerUps.Data;
using UnityEngine;

namespace Game.PowerUps.Events
{
    [CreateAssetMenu(fileName = "PowerUpCancelledEventChannel", menuName = "Knife Toss/Events/PowerUp Cancelled Event Channel")]
    public class PowerUpCancelledEventChannel : EventChannel<PowerUpCancelledEventData>
    {
    }
}
