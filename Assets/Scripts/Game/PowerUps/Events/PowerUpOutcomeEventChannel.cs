using Common.Events;
using Game.PowerUps.Data;
using UnityEngine;

namespace Game.PowerUps.Events
{
    /// <summary>
    /// Unified domain outcome channel for power-up UI, VFX, audio, haptics, and analytics.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpOutcomeEventChannel", menuName = "Knife Toss/Events/Power Ups/Power Up Outcome")]
    public class PowerUpOutcomeEventChannel : EventChannel<PowerUpOutcomeEventData>
    {
    }
}
