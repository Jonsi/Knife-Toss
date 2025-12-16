using UnityEngine;

namespace Game.Entities.Target.Logic
{
    /// <summary>
    /// Accumulates world-space behavior effects for the current frame.
    /// </summary>
    public sealed class WorldBehaviorRuntime
    {
        private Vector3 _Position;

        /// <summary>
        /// Gets the current frame delta time.
        /// </summary>
        public float DeltaTime { get; private set; }

        /// <summary>
        /// Gets the world-space center the behavior should operate around.
        /// </summary>
        public Vector3 WorldCenter { get; private set; }

        /// <summary>
        /// Gets the resulting world position.
        /// </summary>
        public Vector3 Position => _Position;

        /// <summary>
        /// Gets the initial world position at the beginning of the frame.
        /// </summary>
        public Vector3 InitialPosition { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a world center reference was provided.
        /// </summary>
        public bool HasWorldCenter { get; private set; }

        /// <summary>
        /// Initializes the runtime object for a new frame.
        /// </summary>
        /// <param name="deltaTime">Frame delta time.</param>
        /// <param name="worldCenter">Center point provided by the factory.</param>
        /// <param name="currentPosition">Current world position.</param>
        /// <param name="hasWorldCenter">Whether the caller provided a valid center reference.</param>
        public void Begin(float deltaTime, Vector3 worldCenter, Vector3 currentPosition, bool hasWorldCenter)
        {
            DeltaTime = deltaTime;
            WorldCenter = worldCenter;
            InitialPosition = currentPosition;
            HasWorldCenter = hasWorldCenter;
            _Position = currentPosition;
        }

        /// <summary>
        /// Overrides the accumulated position.
        /// </summary>
        /// <param name="position">New world position.</param>
        public void OverridePosition(Vector3 position)
        {
            _Position = position;
        }

        /// <summary>
        /// Adds an offset to the accumulated position.
        /// </summary>
        /// <param name="offset">Offset to apply.</param>
        public void AddOffset(Vector3 offset)
        {
            _Position += offset;
        }
    }
}
