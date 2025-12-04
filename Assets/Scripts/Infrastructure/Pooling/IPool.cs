using Cysharp.Threading.Tasks;
using System.Threading;

namespace KnifeToss.Infrastructure.Pooling
{
    /// <summary>
    /// Generic pool interface for object pooling.
    /// </summary>
    public interface IPool<T>
    {
        /// <summary>
        /// Gets an item from the pool. Creates a new one if pool is empty.
        /// </summary>
        T Get();

        /// <summary>
        /// Returns an item to the pool for reuse.
        /// </summary>
        void Release(T item);

        /// <summary>
        /// Pre-warms the pool by creating the specified number of items.
        /// </summary>
        void Warmup(int count);

        /// <summary>
        /// Pre-warms the pool asynchronously.
        /// </summary>
        UniTask WarmupAsync(int count, CancellationToken ct = default);

        /// <summary>
        /// Clears all items from the pool.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the current number of available items in the pool.
        /// </summary>
        int AvailableCount { get; }

        /// <summary>
        /// Gets the total number of items created by this pool (active + available).
        /// </summary>
        int TotalCount { get; }
    }
}

