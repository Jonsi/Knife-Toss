using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Rewards.Interfaces
{
    /// <summary>
    /// Describes a grantable reward and its presentation (icon and text).
    /// </summary>
    public interface IRewardItem
    {
        /// <summary>
        /// Applies this reward to the player using the grant context.
        /// </summary>
        /// <param name="context">Resolves services required to apply the reward.</param>
        UniTask GrantAsync(IRewardGrantContext context);

        /// <summary>
        /// Short label for UI (e.g. "+50 COIN").
        /// </summary>
        string DisplayText { get; }

        /// <summary>
        /// Icon shown in reward UI, or null if none.
        /// </summary>
        Sprite Icon { get; }
    }
}
