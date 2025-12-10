using System;

namespace KnifeToss.Game.Collision.Interfaces
{
    /// <summary>
    /// Marker interface for objects that can participate in collision detection.
    /// Implement this interface on any gameplay object that can hit or be hit.
    /// </summary>
    public interface IHitable
    {
        /// <summary>
        /// Gets the cached type of this hitable for efficient collision routing.
        /// Implementations should cache this value to avoid repeated GetType() calls.
        /// </summary>
        Type HitableType { get; }
    }
}
