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
    }
}
