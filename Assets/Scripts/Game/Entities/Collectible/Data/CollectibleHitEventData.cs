using UnityEngine;

namespace Game.Entities.Collectible.Data
{
    /// <summary>
    /// Payload describing a collectible hit event.
    /// </summary>
    public struct CollectibleHitEventData
    {
        public readonly Vector2 Position;
        public readonly int Value;

        public CollectibleHitEventData(Vector2 position, int value)
        {
            Position = position;
            Value = value;
        }
    }
}

