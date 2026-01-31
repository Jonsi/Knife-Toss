using System;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.WorldBehaviors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Entities.Target.Data.Behaviors.World
{
    /// <summary>
    /// Configuration for a sine-based movement behavior (Horizontal, Vertical, or both).
    /// </summary>
    [Serializable]
    public class SinMovementBehaviorConfig : IWorldBehaviorConfig
    {
        /// <inheritdoc />
        public TargetWorldBehaviorKind Kind => TargetWorldBehaviorKind.SinMovement;

        [Header("Horizontal Movement")]
        [Tooltip("Total horizontal distance (from left-most to right-most point).")]
        [SerializeField] [FormerlySerializedAs("_MovementDistance")] private float _HorizontalMovementDistance = 1f;

        [Tooltip("Horizontal oscillations per second.")]
        [SerializeField] [FormerlySerializedAs("_OscillationsPerSecond")] private float _HorizontalOscillationsPerSecond = 0.5f;

        [Tooltip("Initial horizontal phase offset applied to the sine wave in degrees.")]
        [SerializeField] [FormerlySerializedAs("_PhaseOffsetDegrees")] private float _HorizontalPhaseOffsetDegrees;

        [Header("Vertical Movement")]
        [Tooltip("Total vertical distance (from bottom-most to top-most point).")]
        [SerializeField] private float _VerticalMovementDistance = 0f;

        [Tooltip("Vertical oscillations per second.")]
        [SerializeField] private float _VerticalOscillationsPerSecond = 0.5f;

        [Tooltip("Initial vertical phase offset applied to the sine wave in degrees.")]
        [SerializeField] private float _VerticalPhaseOffsetDegrees;

        public SinMovementBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with specific horizontal movement parameters.
        /// </summary>
        public SinMovementBehaviorConfig(float horizontalDistance, float horizontalOscillations, float horizontalPhaseDegrees = 0f)
        {
            _HorizontalMovementDistance = horizontalDistance;
            _HorizontalOscillationsPerSecond = horizontalOscillations;
            _HorizontalPhaseOffsetDegrees = horizontalPhaseDegrees;
        }

        /// <summary>
        /// Initializes a new instance with both horizontal and vertical movement parameters.
        /// </summary>
        public SinMovementBehaviorConfig(
            float horizontalDistance, float horizontalOscillations, float horizontalPhaseDegrees,
            float verticalDistance, float verticalOscillations, float verticalPhaseDegrees)
        {
            _HorizontalMovementDistance = horizontalDistance;
            _HorizontalOscillationsPerSecond = horizontalOscillations;
            _HorizontalPhaseOffsetDegrees = horizontalPhaseDegrees;
            _VerticalMovementDistance = verticalDistance;
            _VerticalOscillationsPerSecond = verticalOscillations;
            _VerticalPhaseOffsetDegrees = verticalPhaseDegrees;
        }

        /// <inheritdoc />
        public IWorldTargetBehavior CreateWorldBehavior()
        {
            return new SinMovementWorldBehavior(
                _HorizontalMovementDistance, _HorizontalOscillationsPerSecond, _HorizontalPhaseOffsetDegrees * Mathf.Deg2Rad,
                _VerticalMovementDistance, _VerticalOscillationsPerSecond, _VerticalPhaseOffsetDegrees * Mathf.Deg2Rad);
        }
    }
}
