using Common.Rewards.Interfaces;
using UnityEngine;

namespace Game.Rewards.Configs
{
    /// <summary>
    /// Base for reward grant assets. Each concrete asset type exposes only the fields it needs.
    /// </summary>
    public abstract class RewardGrantEntryBase : ScriptableObject
    {
        /// <summary>
        /// Builds a runtime grant item when this asset is valid; otherwise returns false.
        /// </summary>
        /// <param name="item">The grant item to pass to <see cref="IRewardGrantService"/>.</param>
        /// <returns>True when <paramref name="item"/> was created.</returns>
        public abstract bool TryCreateGrantItem(out IRewardItem item);
    }
}
