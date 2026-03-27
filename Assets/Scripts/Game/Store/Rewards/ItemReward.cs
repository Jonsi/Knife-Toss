using System;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Base class for store rewards.
    /// </summary>
    [Serializable]
    public abstract class ItemReward : IRewardItem
    {
        /// <summary>
        /// Grants this reward.
        /// </summary>
        /// <param name="context">Grant context for resolving services.</param>
        public abstract UniTask GrantAsync(IRewardGrantContext context);

        /// <summary>
        /// Short label for UI and reward popups.
        /// </summary>
        public string DisplayText => GetDisplayText();

        /// <summary>
        /// Icon for reward presentation, or null when not configured.
        /// </summary>
        public virtual Sprite Icon => null;

        /// <summary>
        /// Returns a short human-readable label describing this reward (e.g. "+50 COIN", "+1 Super Knife").
        /// Used by UI elements that need to display the reward without knowing its concrete type.
        /// </summary>
        public abstract string GetDisplayText();
    }
}
