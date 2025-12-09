using Core.Abstractions;

namespace Core.Data
{
    /// <summary>
    /// Immutable data structure containing collision information.
    /// </summary>
    public readonly struct CollisionData
    {
        /// <summary>
        /// The object that initiated the collision (e.g., the knife).
        /// </summary>
        public readonly IHitable Source;

        /// <summary>
        /// The object that was hit (e.g., the target).
        /// </summary>
        public readonly IHitable Target;

        public CollisionData(IHitable source, IHitable target)
        {
            Source = source;
            Target = target;
        }
    }
}

