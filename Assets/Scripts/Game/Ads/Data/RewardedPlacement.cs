using Common.Ads.Data;
using Game.Store.Attributes;
using Game.Store.Rewards;
using UnityEngine;

namespace Game.Ads.Data
{
    /// <summary>
    /// Ad placement configuration for rewarded video ads.
    /// Extends the base placement with a polymorphic reward that can be any <see cref="ItemReward"/>
    /// subtype (currency, skin, power-up, etc.).
    /// Leave <see cref="Reward"/> null when the reward is the continuation itself
    /// (e.g. the Continue-after-failure placement) rather than an item grant.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardedPlacement", menuName = "Knife Toss/Ads/Rewarded Placement")]
    public class RewardedPlacement : AdPlacement
    {
        /// <summary>
        /// The reward granted on successful ad completion.
        /// Null means no item reward (e.g. level continuation placements).
        /// </summary>
        [field: SerializeReference]
        [field: SerializeReferenceDropdown]
        public ItemReward Reward { get; private set; }
    }
}
