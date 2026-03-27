using System;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
using Game.Entities.Collectible.Data;
using Game.Economy.Interfaces;
using UnityEngine;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Grants currency to the player's balance.
    /// </summary>
    [Serializable]
    public class CurrencyReward : ItemReward
    {
        [SerializeField]
        [Tooltip("Which soft currency balance to credit.")]
        private CurrencyType _CurrencyType;

        [SerializeField]
        [Tooltip("Amount added to the player balance when granted.")]
        private int _Amount;

        [SerializeField]
        [Tooltip("Shared currency item config. When set, reward visuals come from this asset instead of a duplicated sprite.")]
        private CollectibleCurrencyConfig _CurrencyConfig;

        [SerializeField]
        [Tooltip("Legacy icon fallback. Prefer configuring Currency Config instead.")]
        private Sprite _Icon;

        public CurrencyType CurrencyType => _CurrencyType;

        public int Amount => _Amount;

        /// <inheritdoc/>
        public override Sprite Icon => _CurrencyConfig != null && _CurrencyConfig.Sprite != null ? _CurrencyConfig.Sprite : _Icon;

        /// <inheritdoc/>
        public override string GetDisplayText() =>
            string.IsNullOrEmpty(GetCurrencyTypeId()) || _Amount <= 0
                ? string.Empty
                : $"+{_Amount} {GetDisplayLabel()}";

        /// <inheritdoc/>
        public override UniTask GrantAsync(IRewardGrantContext context)
        {
            var currencyTypeId = GetCurrencyTypeId();
            if (string.IsNullOrEmpty(currencyTypeId) || _Amount <= 0)
            {
                return UniTask.CompletedTask;
            }

            var currencyService = context.Resolve<ICurrencyService>();
            currencyService.AddBalance(currencyTypeId, _Amount);
            return UniTask.CompletedTask;
        }

        private string GetCurrencyTypeId()
        {
            if (_CurrencyConfig != null && !string.IsNullOrEmpty(_CurrencyConfig.TypeId))
            {
                return _CurrencyConfig.TypeId;
            }

            return _CurrencyType.ToTypeId();
        }

        private string GetDisplayLabel()
        {
            if (_CurrencyConfig != null && !string.IsNullOrEmpty(_CurrencyConfig.TypeId))
            {
                var currencyType = CurrencyTypeExtensions.FromTypeId(_CurrencyConfig.TypeId);
                if (currencyType != CurrencyType.None)
                {
                    return currencyType.ToDisplayLabel();
                }

                return _CurrencyConfig.name.ToUpperInvariant();
            }

            return _CurrencyType.ToDisplayLabel();
        }
    }
}
