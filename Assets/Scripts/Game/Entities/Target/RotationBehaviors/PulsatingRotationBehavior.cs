using Game.Entities.Target.Interfaces;
using UnityEngine;

namespace Game.Entities.Target.RotationBehaviors
{
    /// <summary>
    /// Rotation behavior that oscillates speed with a sine wave.
    /// </summary>
    public class PulsatingRotationBehavior : IRotationBehavior
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
        /// Calculates the rotation delta for the current frame.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame.</param>
        /// <returns>Rotation delta in degrees.</returns>
        public float GetRotationDelta(float deltaTime)
        {
            _Elapsed += deltaTime;
            float oscillation = Mathf.Sin(_Elapsed * _Frequency * Mathf.PI * 2f) * _Amplitude;
            float speed = _BaseSpeed + oscillation;
            return speed * deltaTime;
        }
    }
}
