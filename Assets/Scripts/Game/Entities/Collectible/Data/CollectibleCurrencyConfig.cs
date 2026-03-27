using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
using Game.Economy.Interfaces;
using Game.Items.Configs;
using Game.Entities.Collectible.Views;
using UnityEngine;

namespace Game.Entities.Collectible.Data
{
    /// <summary>
    /// Single config per collectible type: prefab, sprite, and currency data (TypeId and default value).
    /// One asset per type (e.g. tomato, lemon); referenced by CollectibleConfig for placement.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectibleCurrency", menuName = "Knife Toss/Systems/Economy/Collectible Currency Config")]
    public class CollectibleCurrencyConfig : ItemConfigurationBase
    {
        [Header("Visual")]
        [Tooltip("Prefab to spawn for this collectible type.")]
        [SerializeField] private CollectibleView _prefab;

        [Tooltip("Sprite to apply when spawning (optional; prefab may have default).")]
        [SerializeField] private Sprite _sprite;

        [Header("Currency")]
        [Tooltip("Unique identifier for this currency type (e.g., 'tier_1')")]
        [SerializeField] private string _typeId;

        [Tooltip("Default value/worth of this currency unit if no remote override exists")]
        [SerializeField] private int _defaultValue;

        /// <summary>
        /// Prefab to instantiate for this collectible type.
        /// </summary>
        public CollectibleView Prefab => _prefab;

        /// <summary>
        /// Optional sprite override for the spawned collectible.
        /// </summary>
        public Sprite Sprite => _sprite;

        /// <summary>
        /// Gets the unique identifier for this currency type.
        /// </summary>
        public string TypeId => _typeId;

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public int DefaultValue => _defaultValue;

        /// <inheritdoc/>
        public override string GetItemDisplayName()
        {
            var currencyType = CurrencyTypeExtensions.FromTypeId(_typeId);
            if (currencyType != CurrencyType.None)
            {
                return currencyType.ToDisplayLabel();
            }

            return string.IsNullOrEmpty(name) ? "CURRENCY" : name.ToUpperInvariant();
        }

        /// <inheritdoc/>
        public override Sprite GetItemIcon() => _sprite;

        /// <inheritdoc/>
        public override UniTask GrantAsync(int count, IRewardGrantContext context)
        {
            if (string.IsNullOrEmpty(_typeId) || count <= 0)
            {
                return UniTask.CompletedTask;
            }

            var currencyService = context.Resolve<ICurrencyService>();
            currencyService.AddBalance(_typeId, count);
            return UniTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override string GetRewardDisplayText(int count) =>
            count <= 0
                ? string.Empty
                : $"+{count} {GetItemDisplayName()}";
    }
}
