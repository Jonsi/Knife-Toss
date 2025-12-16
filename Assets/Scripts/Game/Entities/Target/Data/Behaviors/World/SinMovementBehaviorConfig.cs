using System;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.WorldBehaviors;
using UnityEngine;

namespace Game.Entities.Target.Data.Behaviors.World
{
    /// <summary>
    /// Configuration for a sine-based horizontal movement behavior.
    /// </summary>
    [Serializable]
    public class SinMovementBehaviorConfig : IWorldBehaviorConfig
    {
        /// <inheritdoc />
        public TargetWorldBehaviorKind Kind => TargetWorldBehaviorKind.SinMovement;

        [Tooltip("Total horizontal distance (from left-most to right-most point).")]
        [SerializeField] private float _MovementDistance = 1f;

        [Tooltip("Horizontal oscillations per second.")]
        [SerializeField] private float _OscillationsPerSecond = 0.5f;

        [Tooltip("Initial phase offset applied to the sine wave in degrees.")]
        [SerializeField] private float _PhaseOffsetDegrees;

        public SinMovementBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with specific movement parameters.
        /// </summary>
        public SinMovementBehaviorConfig(float movementDistance, float oscillationsPerSecond, float phaseOffsetDegrees = 0f)
        {
            _MovementDistance = movementDistance;
            _OscillationsPerSecond = oscillationsPerSecond;
            _PhaseOffsetDegrees = phaseOffsetDegrees;
        }

        /// <inheritdoc />
        public IWorldTargetBehavior CreateWorldBehavior()
        {
            return new SinMovementWorldBehavior(_MovementDistance, _OscillationsPerSecond, _PhaseOffsetDegrees * Mathf.Deg2Rad);
        }
    }
}
