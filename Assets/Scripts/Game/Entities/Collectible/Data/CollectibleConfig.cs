using System;
using UnityEngine;

namespace Game.Entities.Collectible.Data
{
    /// <summary>
    /// Configuration data for a collectible instance on a target.
    /// </summary>
    [Serializable]
    public class CollectibleConfig
    {
        [Tooltip("Currency value awarded when collected.")]
        [field: SerializeField] public int Value { get; private set; } = 1;

        [Tooltip("Angle offset in degrees relative to the target's rotation (0 is right).")]
        [field: SerializeField] public float AngleOffset { get; private set; }

        [Tooltip("Additional distance offset from the target's radius.")]
        [field: SerializeField] public float RadiusOffset { get; private set; }
    }
}

