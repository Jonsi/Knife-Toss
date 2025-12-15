using UnityEngine;

namespace Game.Entities.Target.Logic
{
    /// <summary>
    /// Accumulates local behavior effects for the current frame.
    /// </summary>
    public sealed class LocalBehaviorRuntime
    {
        private float _RotationDelta;

        /// <summary>
        /// Gets the delta time for the current frame.
        /// </summary>
        public float DeltaTime { get; private set; }

        /// <summary>
        /// Gets the accumulated rotation delta.
        /// </summary>
        public float RotationDelta => _RotationDelta;

        /// <summary>
        /// Gets a value indicating whether a rotation change was applied.
        /// </summary>
        public bool HasRotationDelta => Mathf.Abs(_RotationDelta) > Mathf.Epsilon;

        /// <summary>
        /// Prepares the runtime object for a new frame.
        /// </summary>
        /// <param name="deltaTime">Frame delta time.</param>
        public void Begin(float deltaTime)
        {
            DeltaTime = deltaTime;
            _RotationDelta = 0f;
        }

        /// <summary>
        /// Adds a rotation delta contribution.
        /// </summary>
        /// <param name="delta">Rotation delta to accumulate.</param>
        public void AddRotationDelta(float delta)
        {
            _RotationDelta += delta;
        }
    }
}

