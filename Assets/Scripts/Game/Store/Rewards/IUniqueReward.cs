using VContainer;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Optional reward contract for rewards that can only be granted once.
    /// </summary>
    public interface IUniqueReward
    {
        bool IsOwned(IObjectResolver resolver);
        string AlreadyOwnedReason { get; }
    }
}
