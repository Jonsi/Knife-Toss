using System;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.WorldBehaviors;
using UnityEngine;

namespace Game.Entities.Target.Data.Behaviors.World
{
    /// <summary>
    /// Configuration for an orbit world behavior.
    /// </summary>
    [Serializable]
    public class OrbitBehaviorConfig : IWorldBehaviorConfig
    {
        /// <inheritdoc />
        public TargetWorldBehaviorKind Kind => TargetWorldBehaviorKind.Orbit;

        [Tooltip("Starting angle in degrees.")]
        [SerializeField] private float _StartingAngle;

        [Tooltip("Angular speed in degrees per second.")]
        [SerializeField] private float _DegreesPerSecond;

        [Tooltip("Distance from the orbit center applied to the target.")]
        [SerializeField] private float _DistanceFromCenter = 1f;

        public OrbitBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with explicit parameters.
        /// </summary>
        /// <param name="startingAngle">Starting angle in degrees.</param>
        /// <param name="degreesPerSecond">Angular speed.</param>
        /// <param name="distanceFromCenter">Distance from the orbit center.</param>
        public OrbitBehaviorConfig(float startingAngle, float degreesPerSecond, float distanceFromCenter = 1f)
        {
            _StartingAngle = startingAngle;
            _DegreesPerSecond = degreesPerSecond;
            _DistanceFromCenter = distanceFromCenter;
        }

        /// <inheritdoc />
        public IWorldTargetBehavior CreateWorldBehavior()
        {
            return new OrbitWorldBehavior(_StartingAngle, _DegreesPerSecond, _DistanceFromCenter);
        }
    }
}
