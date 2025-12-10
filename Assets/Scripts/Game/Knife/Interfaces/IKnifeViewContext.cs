using Game.Collision.Interfaces;
using Game.Target.Data;
using UnityEngine;

namespace Game.Knife.Interfaces
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


