using Common.Rewards.Interfaces;
using Game.Economy.Data;
using Game.Rewards;
using UnityEngine;

namespace Game.Rewards.Configs
{
    /// <summary>
    /// Grant asset that credits soft currency.
    /// </summary>
    [CreateAssetMenu(fileName = "CurrencyRewardGrant", menuName = "Knife Toss/Rewards/Currency Reward Grant")]
    public class CurrencyRewardGrantEntry : RewardGrantEntryBase
    {
        [SerializeField]
        [Tooltip("Which balance to credit.")]
        private CurrencyType _CurrencyType;

        [SerializeField]
        [Tooltip("Amount added when this grant runs.")]
        private int _Amount;

        /// <summary>
        /// Gets the currency type to credit.
        /// </summary>
        public CurrencyType CurrencyType => _CurrencyType;

        /// <summary>
        /// Gets the amount to add.
        /// </summary>
        public int Amount => _Amount;

        /// <inheritdoc/>
        public override bool TryCreateGrantItem(out IRewardItem item)
        {
            if (_CurrencyType == CurrencyType.None || _Amount < RewardGrantValidation.MinimumCurrencyOrPowerUpAmount)
            {
                item = null;
                return false;
            }

            item = new CurrencyRewardGrantItem(this);
            return true;
        }
    }
}
