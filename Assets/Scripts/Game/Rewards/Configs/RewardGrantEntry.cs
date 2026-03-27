using Common.Rewards.Interfaces;
using Game.Items.Configs;
using UnityEngine;

namespace Game.Rewards.Configs
{
    /// <summary>
    /// Generic reward-grant asset that references a shared item configuration plus count.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardGrant", menuName = "Knife Toss/Rewards/Reward Grant")]
    public class RewardGrantEntry : RewardGrantEntryBase
    {
        [field: SerializeField]
        [field: Tooltip("Shared item configuration used for grant behavior and popup presentation.")]
        public ItemConfigurationBase Item { get; private set; }

        [field: SerializeField]
        [field: Min(1)]
        [field: Tooltip("How many units of this item to grant. Unique items can ignore this.")]
        public int Count { get; private set; } = 1;

        [field: SerializeField]
        [field: Tooltip("Optional icon override used only when presenting this reward in UI.")]
        public Sprite PresentationIconOverride { get; private set; }

        /// <inheritdoc/>
        public override bool TryCreateGrantItem(out IRewardItem item)
        {
            if (Item == null || !Item.IsValidRewardCount(Count))
            {
                item = null;
                return false;
            }

            item = new ItemConfigRewardGrantItem(this);
            return true;
        }
    }
}
