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
    public class OrbitBehaviorConfig : TargetBehaviorConfig, IWorldBehaviorConfig
    {
        /// <inheritdoc />
        public override TargetBehaviorKind Kind => TargetBehaviorKind.Orbit;

        [Tooltip("Radius of the orbit path.")]
        [SerializeField] private float _Radius;

        [Tooltip("Starting angle in degrees.")]
        [SerializeField] private float _StartingAngle;

        [Tooltip("Angular speed in degrees per second.")]
        [SerializeField] private float _DegreesPerSecond;

        public OrbitBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with explicit parameters.
        /// </summary>
        /// <param name="radius">Orbit radius.</param>
        /// <param name="startingAngle">Starting angle in degrees.</param>
        /// <param name="degreesPerSecond">Angular speed.</param>
        public OrbitBehaviorConfig(float radius, float startingAngle, float degreesPerSecond)
        {
            _Radius = radius;
            _StartingAngle = startingAngle;
            _DegreesPerSecond = degreesPerSecond;
        }

        /// <inheritdoc />
        public IWorldTargetBehavior CreateWorldBehavior()
        {
            return new OrbitWorldBehavior(_Radius, _StartingAngle, _DegreesPerSecond);
        }
    }
}
