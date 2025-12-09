namespace Core.Abstractions
{
    /// <summary>
    /// Interface for handling collisions between two IHitable objects.
    /// </summary>
    public interface ICollisionHandler
    {
        /// <summary>
        /// Handles a collision between source and target.
        /// </summary>
        /// <param name="source">The object that initiated the collision.</param>
        /// <param name="target">The object that was hit.</param>
        void Handle(IHitable source, IHitable target);
    }
}

