using Game.Entities.Target.Interfaces;
using Game.PowerUps.Interfaces;
using Game.PowerUps.Services;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Time Dilator power-up that slows target rotation.
    /// </summary>
    [CreateAssetMenu(fileName = "TimeDilatorConfig", menuName = "Knife Toss/Systems/Power Ups/Time Dilator Config")]
    public class TimeDilatorConfig : PowerUpConfigBase, IPowerUpEffect, IWorldEffectPowerUp
    {
        [field: SerializeField, Min(0.01f)] public float Duration { get; private set; } = 5f;
        [field: SerializeField, Range(0f, 1f)] public float SpeedMultiplier { get; private set; } = 0.3f;

        public float EffectDuration => Duration;

        /// <summary>
        /// Executes the time dilator power-up by applying the slow motion effect.
        /// Note: This power-up should be activated immediately, not executed on throw.
        /// </summary>
        public override bool Execute(IPowerUpExecutionContext context)
        {
            return false;
        }

        public void ApplyWorldEffect(PowerUpEffectService effectService)
        {
            effectService?.ApplyEffect(this, Duration);
        }

        /// <summary>
        /// Applies the speed reduction effect to the target.
        /// </summary>
        public void Apply(ITargetView target)
        {
            if (target != null)
            {
                target.SetSpeedMultiplier(SpeedMultiplier);
            }
        }

        /// <summary>
        /// Removes the speed reduction effect from the target.
        /// </summary>
        public void Remove(ITargetView target)
        {
            if (target != null)
            {
                target.ClearRotationModifiers();
            }
        }
    }
}
