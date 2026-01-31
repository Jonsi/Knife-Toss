using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using UnityEngine;

namespace Game.Entities.Target.WorldBehaviors
{
    /// <summary>
    /// Moves the target following a sine wave on horizontal and/or vertical axes.
    /// </summary>
    public class SinMovementWorldBehavior : IWorldTargetBehavior
    {
        private readonly float _HorizontalDistance;
        private readonly float _HorizontalAngularFrequency;
        private readonly float _HorizontalPhaseOffset;

        private readonly float _VerticalDistance;
        private readonly float _VerticalAngularFrequency;
        private readonly float _VerticalPhaseOffset;

        private float _Elapsed;

        /// <summary>
        /// Initializes the sine movement behavior.
        /// </summary>
        public SinMovementWorldBehavior(
            float horizontalDistance, float horizontalOscillationsPerSecond, float horizontalPhaseOffset,
            float verticalDistance, float verticalOscillationsPerSecond, float verticalPhaseOffset)
        {
            _HorizontalDistance = Mathf.Max(0f, horizontalDistance);
            _HorizontalAngularFrequency = Mathf.Max(0f, horizontalOscillationsPerSecond) * Mathf.PI * 2f;
            _HorizontalPhaseOffset = horizontalPhaseOffset;

            _VerticalDistance = Mathf.Max(0f, verticalDistance);
            _VerticalAngularFrequency = Mathf.Max(0f, verticalOscillationsPerSecond) * Mathf.PI * 2f;
            _VerticalPhaseOffset = verticalPhaseOffset;
        }

        /// <inheritdoc />
        public void Tick(WorldBehaviorRuntime runtime)
        {
            _Elapsed += runtime.DeltaTime;

            float offsetX = 0f;
            if (_HorizontalDistance > Mathf.Epsilon && _HorizontalAngularFrequency > Mathf.Epsilon)
            {
                float angle = _HorizontalPhaseOffset + _HorizontalAngularFrequency * _Elapsed;
                offsetX = Mathf.Sin(angle) * (_HorizontalDistance * 0.5f);
            }

            float offsetY = 0f;
            if (_VerticalDistance > Mathf.Epsilon && _VerticalAngularFrequency > Mathf.Epsilon)
            {
                float angle = _VerticalPhaseOffset + _VerticalAngularFrequency * _Elapsed;
                offsetY = Mathf.Sin(angle) * (_VerticalDistance * 0.5f);
            }

            Vector3 anchorPoint = runtime.HasWorldCenter ? runtime.WorldCenter : runtime.InitialPosition;
            Vector3 position = new Vector3(anchorPoint.x + offsetX, anchorPoint.y + offsetY, anchorPoint.z);
            runtime.OverridePosition(position);
        }
    }
}
