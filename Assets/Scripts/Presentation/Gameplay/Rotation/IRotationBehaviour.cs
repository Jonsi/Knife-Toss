using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Rotation
{
    public interface IRotationBehaviour
    {
        void ApplyRotation(Transform target, float deltaTime);
        void Reset();
    }
}
