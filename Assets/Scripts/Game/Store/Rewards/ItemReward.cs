using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Base class for store rewards.
    /// </summary>
    [Serializable]
    public abstract class ItemReward
    {
        /// <summary>
        /// Grants this reward.
        /// </summary>
        /// <param name="resolver">Container resolver for dependency access.</param>
        public abstract UniTask GrantAsync(IObjectResolver resolver);

        /// <summary>
        /// Returns a short human-readable label describing this reward (e.g. "+50 COIN", "+1 Super Knife").
        /// Used by UI elements that need to display the reward without knowing its concrete type.
        /// </summary>
        public abstract string GetDisplayText();
    }
}
