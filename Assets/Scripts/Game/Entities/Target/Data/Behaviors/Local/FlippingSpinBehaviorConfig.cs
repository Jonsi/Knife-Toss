using System;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.RotationBehaviors;
using UnityEngine;

namespace Game.Entities.Target.Data.Behaviors.Local
{
    /// <summary>
    /// Configuration for a flipping spin behavior.
    /// </summary>
    [Serializable]
    public class FlippingSpinBehaviorConfig : ILocalBehaviorConfig
    {
        /// <inheritdoc />
        public TargetLocalBehaviorKind Kind => TargetLocalBehaviorKind.FlippingSpin;

        [Tooltip("Base rotation speed in degrees per second.")]
        [SerializeField] private float _BaseSpeed = 50f;

        [Tooltip("Minimum seconds before flipping direction.")]
        [SerializeField] private float _MinInterval = 1f;

        [Tooltip("Maximum seconds before flipping direction.")]
        [SerializeField] private float _MaxInterval = 3f;

        public FlippingSpinBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with explicit parameters.
        /// </summary>
        /// <param name="baseSpeed">Base speed.</param>
        /// <param name="minInterval">Minimum flip interval.</param>
        /// <param name="maxInterval">Maximum flip interval.</param>
        public FlippingSpinBehaviorConfig(float baseSpeed, float minInterval, float maxInterval)
        {
            _BaseSpeed = baseSpeed;
            _MinInterval = minInterval;
            _MaxInterval = maxInterval;
        }

        /// <inheritdoc />
        public ILocalTargetBehavior CreateLocalBehavior()
        {
            return new FlippingRotationBehavior(_BaseSpeed, _MinInterval, _MaxInterval);
        }
    }
}
