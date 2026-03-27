using Common.Rewards.Interfaces;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Optional reward contract for rewards that can only be granted once.
    /// </summary>
    public interface IUniqueReward
    {
        /// <summary>
        /// Returns whether the player already owns this reward.
        /// </summary>
        bool IsOwned(IRewardGrantContext context);

        /// <summary>
        /// Message shown when a purchase is rejected because the item is already owned.
        /// </summary>
        string AlreadyOwnedReason { get; }
    }
}
