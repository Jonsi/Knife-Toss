using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Rotation
{
    /// <summary>
    /// Defines the IRotationBehaviour interface.
    /// </summary>
    public interface IRotationBehaviour
    {
        /// <summary>
        /// Performs the ApplyRotation operation.
        /// </summary>
        void ApplyRotation(Transform target, float deltaTime);
        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        void Reset();
    }
}
