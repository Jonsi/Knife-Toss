using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using UnityEngine;

namespace Game.Entities.Target.WorldBehaviors
{
    /// <summary>
    /// Moves the target horizontally following a sine wave.
    /// </summary>
    public class SinMovementWorldBehavior : IWorldTargetBehavior
    {
        private readonly float _MovementDistance;
        private readonly float _AngularFrequency;
        private readonly float _PhaseOffset;
        private float _Elapsed;

        /// <summary>
        /// Initializes the sine movement behavior.
        /// </summary>
        /// <param name="movementDistance">Total horizontal distance covered by the movement.</param>
        /// <param name="oscillationsPerSecond">Number of oscillations performed each second.</param>
        /// <param name="phaseOffset">Phase offset (in radians).</param>
        public SinMovementWorldBehavior(float movementDistance, float oscillationsPerSecond, float phaseOffset)
        {
            _MovementDistance = Mathf.Max(0f, movementDistance);
            _AngularFrequency = Mathf.Max(0f, oscillationsPerSecond) * Mathf.PI * 2f;
            _PhaseOffset = phaseOffset;
        }

        /// <inheritdoc />
        public void Tick(WorldBehaviorRuntime runtime)
        {
            if (_MovementDistance <= Mathf.Epsilon || _AngularFrequency <= Mathf.Epsilon)
            {
                Vector3 anchor = runtime.HasWorldCenter ? runtime.WorldCenter : runtime.InitialPosition;
                runtime.OverridePosition(anchor);
                return;
            }

            _Elapsed += runtime.DeltaTime;
            float halfRange = _MovementDistance * 0.5f;
            float angle = _PhaseOffset + _AngularFrequency * _Elapsed;
            float offsetX = Mathf.Sin(angle) * halfRange;

            Vector3 anchorPoint = runtime.HasWorldCenter ? runtime.WorldCenter : runtime.InitialPosition;
            Vector3 position = new Vector3(anchorPoint.x + offsetX, anchorPoint.y, anchorPoint.z);
            runtime.OverridePosition(position);
        }
    }
}
