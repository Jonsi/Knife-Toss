using Common.Collision.Interfaces;
using Game.Entities.Target.Data;
using UnityEngine;

namespace Game.Entities.Knife.Interfaces
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


