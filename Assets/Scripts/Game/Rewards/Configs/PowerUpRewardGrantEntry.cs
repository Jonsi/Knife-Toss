using Common.Rewards.Interfaces;
using Game.PowerUps.Configs;
using Game.Rewards;
using UnityEngine;

namespace Game.Rewards.Configs
{
    /// <summary>
    /// Grant asset that adds power-up charges to inventory.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpRewardGrant", menuName = "Knife Toss/Rewards/Power-Up Reward Grant")]
    public class PowerUpRewardGrantEntry : RewardGrantEntryBase
    {
        [SerializeField]
        [Tooltip("Power-up definition; inventory key comes from this asset.")]
        private PowerUpConfigBase _PowerUp;

        [SerializeField]
        [Tooltip("How many charges to add.")]
        private int _Count;

        /// <summary>
        /// Gets the power-up configuration.
        /// </summary>
        public PowerUpConfigBase PowerUp => _PowerUp;

        /// <summary>
        /// Gets the number of charges to add.
        /// </summary>
        public int Count => _Count;

        /// <inheritdoc/>
        public override bool TryCreateGrantItem(out IRewardItem item)
        {
            if (_PowerUp == null ||
                string.IsNullOrEmpty(_PowerUp.InventoryKey) ||
                _Count < RewardGrantValidation.MinimumCurrencyOrPowerUpAmount)
            {
                item = null;
                return false;
            }

            item = new PowerUpRewardGrantItem(this);
            return true;
        }
    }
}
