using Common.Rewards.Interfaces;
using Game.Store.Configs;
using Game.Rewards;
using UnityEngine;

namespace Game.Rewards.Configs
{
    /// <summary>
    /// Grant asset that unlocks a knife skin once.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeSkinRewardGrant", menuName = "Knife Toss/Rewards/Knife Skin Reward Grant")]
    public class KnifeSkinRewardGrantEntry : RewardGrantEntryBase
    {
        [SerializeField]
        [Tooltip("Skin definition to unlock; persisted id comes from this asset.")]
        private KnifeSkinConfig _KnifeSkin;

        /// <summary>
        /// Gets the knife skin configuration.
        /// </summary>
        public KnifeSkinConfig KnifeSkin => _KnifeSkin;

        /// <inheritdoc/>
        public override bool TryCreateGrantItem(out IRewardItem item)
        {
            if (_KnifeSkin == null || string.IsNullOrEmpty(_KnifeSkin.Id))
            {
                item = null;
                return false;
            }

            item = new KnifeSkinRewardGrantItem(this);
            return true;
        }
    }
}
