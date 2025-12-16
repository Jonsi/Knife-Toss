using UnityEngine;

namespace Common.Interfaces
{
    public interface IStickable
    {
        Transform Transform { get; }
        float RadiusOffset { get; }
        float RotationOffset { get; }
        void OnAttachedToTarget();
    }
}

