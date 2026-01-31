using Common.Economy.Data;
using Game.Entities.Collectible.Views;
using UnityEngine;

namespace Game.Entities.Collectible.Data
{
    /// <summary>
    /// Single config per collectible type: prefab, sprite, and currency (via CurrencyTypeConfig).
    /// One asset per type (e.g. tomato, lemon); referenced by CollectibleConfig for placement.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectibleCurrency", menuName = "Knife Toss/Collectible Currency Config")]
    public class CollectibleCurrencyConfig : ScriptableObject
    {
        [Header("Visual")]
        [Tooltip("Prefab to spawn for this collectible type.")]
        [SerializeField] private CollectibleView _prefab;

        [Tooltip("Sprite to apply when spawning (optional; prefab may have default).")]
        [SerializeField] private Sprite _sprite;

        [Header("Currency")]
        [Tooltip("Currency type (TypeId and value). Required.")]
        [SerializeField] private CurrencyTypeConfig _currencyType;

        /// <summary>
        /// Prefab to instantiate for this collectible type.
        /// </summary>
        public CollectibleView Prefab => _prefab;

        /// <summary>
        /// Optional sprite override for the spawned collectible.
        /// </summary>
        public Sprite Sprite => _sprite;

        /// <summary>
        /// Currency type for value resolution and balance.
        /// </summary>
        public CurrencyTypeConfig CurrencyType => _currencyType;
    }
}
