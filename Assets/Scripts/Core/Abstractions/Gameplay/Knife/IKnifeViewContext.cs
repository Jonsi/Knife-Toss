using KnifeToss.Core.Abstractions.Collision;
using KnifeToss.Core.Data.Target;
using UnityEngine;

namespace KnifeToss.Core.Abstractions.Gameplay.Knife
{
    /// <summary>
    /// Minimal contract exposed by the knife view for state logic.
    /// </summary>
    public interface IKnifeViewContext : IHitable
    {
        Transform Transform { get; }
        StickPositionData StickPositionData { get; }
        void SetColliderEnabled(bool enabled);
        void SetActive(bool isActive);
    }
}


