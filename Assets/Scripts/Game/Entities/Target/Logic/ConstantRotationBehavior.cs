using Game.Entities.Target.Interfaces;

namespace Game.Entities.Target.Logic
{
    /// <summary>
    /// Constant speed rotation behavior implementation.
    /// Rotates at a fixed speed in degrees per second.
    /// </summary>
    public class ConstantRotationBehavior : ILocalTargetBehavior
    {
        private readonly float _Speed;

        /// <summary>
        /// Initializes a new instance of ConstantRotationBehavior with the specified rotation speed.
        /// </summary>
        /// <param name="speed">The rotation speed in degrees per second.</param>
        public ConstantRotationBehavior(float speed)
        {
            _Speed = speed;
        }

        /// <summary>
        /// Applies rotation for the current frame.
        /// </summary>
        /// <param name="runtime">Frame runtime context.</param>
        public void Tick(LocalBehaviorRuntime runtime)
        {
            runtime.AddRotationDelta(_Speed * runtime.DeltaTime);
        }
    }
}

