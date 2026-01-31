using System;
using UnityEngine;

namespace Game.Entities.Collectible.Data
{
    /// <summary>
    /// Per-slot data for a collectible on a target: which type (prefab/sprite/currency) and placement.
    /// References one CollectibleCurrencyConfig; serialized in TargetConfig.Collectibles.
    /// </summary>
    [Serializable]
    public class CollectibleConfig
    {
        [Tooltip("Collectible type (prefab, sprite, currency). Required.")]
        [SerializeField] private CollectibleCurrencyConfig _collectibleCurrency;

        [Tooltip("Angle offset in degrees relative to the target's rotation (0 is right).")]
        [field: SerializeField] public float AngleOffset { get; private set; }

        [Tooltip("Additional distance offset from the target's radius.")]
        [field: SerializeField] public float RadiusOffset { get; private set; }

        /// <summary>
        /// Collectible type (prefab, sprite, currency).
        /// </summary>
        public CollectibleCurrencyConfig CollectibleCurrency => _collectibleCurrency;

        public CollectibleConfig() { }

        /// <summary>
        /// Creates a config for runtime (e.g. editor stage generator).
        /// </summary>
        public CollectibleConfig(CollectibleCurrencyConfig collectibleCurrency, float angleOffset, float radiusOffset)
        {
            _collectibleCurrency = collectibleCurrency;
            AngleOffset = angleOffset;
            RadiusOffset = radiusOffset;
        }
    }
}
