namespace KnifeToss.Core.Abstractions.Pooling
{
    /// <summary>
    /// Interface for objects that can be pooled. Provides lifecycle hooks for spawn/despawn.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is retrieved from the pool (spawned).
        /// Use this to reset state, enable components, etc.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when the object is returned to the pool (despawned).
        /// Use this to clean up, disable components, stop animations, etc.
        /// </summary>
        void OnDespawn();
    }
}

