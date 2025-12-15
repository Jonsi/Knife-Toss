using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using UnityEngine;

namespace Game.Entities.Target.RotationBehaviors
{
    /// <summary>
    /// Rotation behavior that flips direction at random intervals.
    /// </summary>
    public class FlippingRotationBehavior : ILocalTargetBehavior
    {
        private readonly float _SpeedMagnitude;
        private readonly float _MinInterval;
        private readonly float _MaxInterval;
        private float _TimeUntilFlip;
        private int _Direction;

        /// <summary>
        /// Initializes a new instance of the flipping rotation behavior.
        /// </summary>
        /// <param name="baseSpeed">Base rotation speed in degrees per second.</param>
        /// <param name="minInterval">Minimum seconds before a flip.</param>
        /// <param name="maxInterval">Maximum seconds before a flip.</param>
        public FlippingRotationBehavior(float baseSpeed, float minInterval, float maxInterval)
        {
            _SpeedMagnitude = Mathf.Abs(baseSpeed);
            _Direction = Mathf.Sign(baseSpeed) >= 0f ? 1 : -1;
            _MinInterval = Mathf.Max(0.01f, minInterval);
            _MaxInterval = Mathf.Max(_MinInterval, maxInterval);
            _TimeUntilFlip = GetNextInterval();
        }

        /// <summary>
        /// Applies the flipping rotation for the current frame.
        /// </summary>
        /// <param name="runtime">Frame runtime context.</param>
        public void Tick(LocalBehaviorRuntime runtime)
        {
            _TimeUntilFlip -= runtime.DeltaTime;
            if (_TimeUntilFlip <= 0f)
            {
                _Direction = _Direction == 0 ? 1 : -_Direction;
                _TimeUntilFlip = GetNextInterval();
            }

            float speed = _SpeedMagnitude * _Direction;
            runtime.AddRotationDelta(speed * runtime.DeltaTime);
        }

        private float GetNextInterval()
        {
            return Random.Range(_MinInterval, _MaxInterval);
        }
    }
}
