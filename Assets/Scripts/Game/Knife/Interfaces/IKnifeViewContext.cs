using KnifeToss.Game.Collision.Interfaces;
using KnifeToss.Game.Target.Data;
using UnityEngine;

namespace KnifeToss.Game.Knife.Interfaces
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


