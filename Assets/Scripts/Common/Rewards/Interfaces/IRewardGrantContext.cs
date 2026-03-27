namespace Common.Rewards.Interfaces
{
    /// <summary>
    /// Supplies services needed to apply rewards. Implemented in the Game assembly (e.g. via a DI container).
    /// Common reward code depends only on this contract, not on a specific DI container.
    /// </summary>
    public interface IRewardGrantContext
    {
        /// <summary>
        /// Resolves a service type required to grant or validate a reward.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <returns>Resolved instance.</returns>
        T Resolve<T>() where T : notnull;
    }
}
