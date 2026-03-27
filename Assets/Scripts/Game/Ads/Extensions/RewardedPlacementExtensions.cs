using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Ads.Data;

namespace Game.Ads.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="RewardedPlacement"/> that bridge ad placements
    /// with the game's polymorphic reward system.
    /// </summary>
    public static class RewardedPlacementExtensions
    {
        /// <summary>
        /// Grants the placement's configured reward via <see cref="Game.Store.Rewards.ItemReward.GrantAsync"/>.
        /// Returns true if a reward was granted, false if the placement has no reward configured.
        /// </summary>
        public static async UniTask<bool> TryGrantRewardAsync(this RewardedPlacement placement, IRewardGrantContext context)
        {
            if (placement.Reward == null)
                return false;

            await placement.Reward.GrantAsync(context);
            return true;
        }
    }
}
