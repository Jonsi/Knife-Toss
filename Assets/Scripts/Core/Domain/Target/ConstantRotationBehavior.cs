using KnifeToss.Core.Abstractions.Gameplay.Target;

namespace KnifeToss.Core.Domain.Target
{
    /// <summary>
    /// Constant speed rotation behavior implementation.
    /// Rotates at a fixed speed in degrees per second.
    /// </summary>
    public class ConstantRotationBehavior : IRotationBehavior
    {
        private readonly float _speed;

        /// <summary>
        /// Initializes a new instance of ConstantRotationBehavior with the specified rotation speed.
        /// </summary>
        /// <param name="speed">The rotation speed in degrees per second.</param>
        public ConstantRotationBehavior(float speed)
        {
            _speed = speed;
        }

        /// <summary>
        /// Calculates the rotation delta for the current frame.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        /// <returns>The rotation delta in degrees.</returns>
        public float GetRotationDelta(float deltaTime)
        {
            return _speed * deltaTime;
        }
    }
}

