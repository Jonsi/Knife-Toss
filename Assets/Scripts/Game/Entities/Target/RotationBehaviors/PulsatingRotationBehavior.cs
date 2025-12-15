using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using UnityEngine;

namespace Game.Entities.Target.RotationBehaviors
{
    /// <summary>
    /// Rotation behavior that oscillates speed with a sine wave.
    /// </summary>
    public class PulsatingRotationBehavior : ILocalTargetBehavior
    {
        private readonly float _BaseSpeed;
        private readonly float _Amplitude;
        private readonly float _Frequency;
        private float _Elapsed;

        /// <summary>
        /// Initializes a new instance of the pulsating rotation behavior.
        /// </summary>
        /// <param name="baseSpeed">Base rotation speed in degrees per second.</param>
        /// <param name="amplitude">Amplitude applied to the oscillation.</param>
        /// <param name="frequency">Oscillation frequency in hertz.</param>
        public PulsatingRotationBehavior(float baseSpeed, float amplitude, float frequency)
        {
            _BaseSpeed = baseSpeed;
            _Amplitude = amplitude;
            _Frequency = frequency;
        }

        /// <summary>
        /// Applies the pulsating rotation for the current frame.
        /// </summary>
        /// <param name="runtime">Frame runtime context.</param>
        public void Tick(LocalBehaviorRuntime runtime)
        {
            _Elapsed += runtime.DeltaTime;
            float oscillation = Mathf.Sin(_Elapsed * _Frequency * Mathf.PI * 2f) * _Amplitude;
            float speed = _BaseSpeed + oscillation;
            runtime.AddRotationDelta(speed * runtime.DeltaTime);
        }
    }
}
