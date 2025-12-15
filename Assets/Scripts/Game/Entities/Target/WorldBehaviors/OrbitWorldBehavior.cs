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
        private readonly float _DegreesPerSecond;
        private readonly float _DistanceFromCenter;
        private float _Angle;

        /// <summary>
        /// Initializes a new instance of the orbit world behavior.
        /// </summary>
        /// <param name="startingAngle">Starting angle in degrees.</param>
        /// <param name="degreesPerSecond">Angular speed in degrees per second.</param>
        /// <param name="distanceFromCenter">Distance applied from the orbit center.</param>
        public OrbitWorldBehavior(float startingAngle, float degreesPerSecond, float distanceFromCenter)
        {
            _Angle = startingAngle;
            _DegreesPerSecond = degreesPerSecond;
            _DistanceFromCenter = Mathf.Max(0f, distanceFromCenter);
        }

        /// <summary>
        /// Updates the orbit for the current frame.
        /// </summary>
        /// <param name="runtime">Frame runtime context.</param>
        public void Tick(WorldBehaviorRuntime runtime)
        {
            _Angle += _DegreesPerSecond * runtime.DeltaTime;
            float radians = _Angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
            if (direction.sqrMagnitude < Mathf.Epsilon)
                direction = Vector3.right;

            Vector3 offset = direction.normalized * _DistanceFromCenter;
            runtime.OverridePosition(runtime.WorldCenter + offset);
        }
    }
}
