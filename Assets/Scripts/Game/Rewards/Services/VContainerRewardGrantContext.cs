using Common.Rewards.Interfaces;
using VContainer;

namespace Game.Rewards.Services
{
    /// <summary>
    /// Bridges <see cref="IRewardGrantContext"/> to VContainer's <see cref="IObjectResolver"/>.
    /// </summary>
    public sealed class VContainerRewardGrantContext : IRewardGrantContext
    {
        private readonly IObjectResolver _Resolver;

        /// <summary>
        /// Creates a context that resolves services from the given resolver.
        /// </summary>
        public VContainerRewardGrantContext(IObjectResolver resolver)
        {
            _Resolver = resolver;
        }

        /// <inheritdoc/>
        public T Resolve<T>() where T : notnull
        {
            return _Resolver.Resolve<T>();
        }
    }
}
