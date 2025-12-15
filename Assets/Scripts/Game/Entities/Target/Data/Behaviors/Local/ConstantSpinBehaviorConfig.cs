using System;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;

namespace Game.Entities.Target.Data.Behaviors.Local
{
    /// <summary>
    /// Configuration for a constant spin local behavior.
    /// </summary>
    [Serializable]
    public class ConstantSpinBehaviorConfig : ILocalBehaviorConfig
    {
        /// <inheritdoc />
        public TargetLocalBehaviorKind Kind => TargetLocalBehaviorKind.ConstantSpin;

        [UnityEngine.Tooltip("Rotation speed in degrees per second.")]
        [UnityEngine.SerializeField] private float _Speed = 50f;

        public ConstantSpinBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with a custom speed.
        /// </summary>
        /// <param name="speed">Speed in degrees per second.</param>
        public ConstantSpinBehaviorConfig(float speed)
        {
            _Speed = speed;
        }

        /// <inheritdoc />
        public ILocalTargetBehavior CreateLocalBehavior()
        {
            return new ConstantRotationBehavior(_Speed);
        }
    }
}
