using System;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.RotationBehaviors;
using UnityEngine;

namespace Game.Entities.Target.Data.Behaviors.Local
{
    /// <summary>
    /// Configuration for a pulsating spin behavior.
    /// </summary>
    [Serializable]
    public class PulsatingSpinBehaviorConfig : TargetBehaviorConfig, ILocalBehaviorConfig
    {
        /// <inheritdoc />
        public override TargetBehaviorKind Kind => TargetBehaviorKind.PulsatingSpin;

        [Tooltip("Base rotation speed in degrees per second.")]
        [SerializeField] private float _BaseSpeed = 50f;

        [Tooltip("Oscillation amplitude applied to the speed.")]
        [SerializeField] private float _Amplitude = 10f;

        [Tooltip("Oscillation frequency in hertz.")]
        [SerializeField] private float _Frequency = 1f;

        public PulsatingSpinBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with explicit parameters.
        /// </summary>
        /// <param name="baseSpeed">Base speed.</param>
        /// <param name="amplitude">Amplitude.</param>
        /// <param name="frequency">Frequency.</param>
        public PulsatingSpinBehaviorConfig(float baseSpeed, float amplitude, float frequency)
        {
            _BaseSpeed = baseSpeed;
            _Amplitude = amplitude;
            _Frequency = frequency;
        }

        /// <inheritdoc />
        public ILocalTargetBehavior CreateLocalBehavior()
        {
            return new PulsatingRotationBehavior(_BaseSpeed, _Amplitude, _Frequency);
        }
    }
}
