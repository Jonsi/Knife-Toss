using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using UnityEngine;

namespace Game.Entities.Target.WorldBehaviors
{
    /// <summary>
    /// World-space behavior that orbits the target around a center point.
    /// </summary>
    public class OrbitWorldBehavior : IWorldTargetBehavior
    {
        private readonly float _Radius;
        private readonly float _DegreesPerSecond;
        private float _Angle;

        /// <summary>
        /// Initializes a new instance of the orbit world behavior.
        /// </summary>
        /// <param name="radius">Orbit radius.</param>
        /// <param name="startingAngle">Starting angle in degrees.</param>
        /// <param name="degreesPerSecond">Angular speed in degrees per second.</param>
        public OrbitWorldBehavior(float radius, float startingAngle, float degreesPerSecond)
        {
            _Radius = Mathf.Max(0f, radius);
            _Angle = startingAngle;
            _DegreesPerSecond = degreesPerSecond;
        }

        /// <summary>
        /// Updates the orbit for the current frame.
        /// </summary>
        /// <param name="runtime">Frame runtime context.</param>
        public void Tick(WorldBehaviorRuntime runtime)
        {
            _Angle += _DegreesPerSecond * runtime.DeltaTime;
            float radians = _Angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * _Radius;
            runtime.OverridePosition(runtime.WorldCenter + offset);
        }
    }
}

