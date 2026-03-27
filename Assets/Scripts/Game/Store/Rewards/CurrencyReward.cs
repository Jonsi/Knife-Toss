using System;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
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
        [Tooltip("Optional icon for reward popups and UI; leave empty if not shown.")]
        private Sprite _Icon;

        public CurrencyType CurrencyType => _CurrencyType;

        public int Amount => _Amount;

        /// <inheritdoc/>
        public override Sprite Icon => _Icon;

        /// <inheritdoc/>
        public override string GetDisplayText() =>
            _CurrencyType == CurrencyType.None || _Amount <= 0
                ? string.Empty
                : $"+{_Amount} {_CurrencyType.ToDisplayLabel()}";

        /// <inheritdoc/>
        public override UniTask GrantAsync(IRewardGrantContext context)
        {
            if (_CurrencyType == CurrencyType.None || _Amount <= 0)
            {
                return UniTask.CompletedTask;
            }

            var currencyService = context.Resolve<ICurrencyService>();
            currencyService.AddBalance(_CurrencyType.ToTypeId(), _Amount);
            return UniTask.CompletedTask;
        }
    }
}
