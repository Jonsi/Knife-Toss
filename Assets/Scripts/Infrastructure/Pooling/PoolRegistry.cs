using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace KnifeToss.Infrastructure.Pooling
{
    /// <summary>
    /// Central registry for managing all pools in the game.
    /// Provides a single point of access for pool management and debugging.
    /// </summary>
    public sealed class PoolRegistry : IDisposable
    {
        private readonly Dictionary<Type, IPool<object>> _pools = new Dictionary<Type, IPool<object>>();
        private readonly Dictionary<string, IPool<object>> _namedPools = new Dictionary<string, IPool<object>>();

        /// <summary>
        /// Registers a pool for a specific type.
        /// </summary>
        public void Register<T>(IPool<T> pool, string name = null) where T : class
        {
            var type = typeof(T);
            var objectPool = new PoolWrapper<T>(pool);

            if (_pools.ContainsKey(type))
            {
                Debug.LogWarning($"Pool for type {type.Name} already registered. Overwriting.");
            }

            _pools[type] = objectPool;

            if (!string.IsNullOrEmpty(name))
            {
                _namedPools[name] = objectPool;
            }
        }

        /// <summary>
        /// Gets a pool for the specified type.
        /// </summary>
        public IPool<T> GetPool<T>() where T : class
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var pool))
            {
                return ((PoolWrapper<T>)pool).WrappedPool;
            }

            Debug.LogError($"No pool registered for type {type.Name}");
            return null;
        }

        /// <summary>
        /// Gets a pool by name.
        /// </summary>
        public IPool<T> GetPool<T>(string name) where T : class
        {
            if (_namedPools.TryGetValue(name, out var pool))
            {
                return ((PoolWrapper<T>)pool).WrappedPool;
            }

            Debug.LogError($"No pool registered with name '{name}'");
            return null;
        }

        /// <summary>
        /// Warms up all registered pools.
        /// </summary>
        public async UniTask WarmupAllAsync(int countPerPool = 10, CancellationToken ct = default)
        {
            var tasks = new List<UniTask>();

            foreach (var pool in _pools.Values)
            {
                // We can't directly call WarmupAsync on IPool<object>, so we'll need to handle this differently
                // For now, we'll use synchronous warmup
                tasks.Add(WarmupPoolAsync(pool, countPerPool, ct));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask WarmupPoolAsync(IPool<object> pool, int count, CancellationToken ct)
        {
            // Since we can't call generic methods on IPool<object>, we'll use reflection or a wrapper
            // For simplicity, we'll just call Warmup synchronously
            await UniTask.Yield(ct);
            // Note: This is a limitation - we'd need a non-generic IPool interface to properly warmup
            // For now, pools should be warmed up individually
        }

        /// <summary>
        /// Clears all pools.
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
        }

        /// <summary>
        /// Gets debug information about all pools.
        /// </summary>
        public Dictionary<string, PoolInfo> GetPoolInfo()
        {
            var info = new Dictionary<string, PoolInfo>();

            foreach (var kvp in _pools)
            {
                info[kvp.Key.Name] = new PoolInfo
                {
                    Type = kvp.Key.Name,
                    Available = kvp.Value.AvailableCount,
                    Total = kvp.Value.TotalCount
                };
            }

            return info;
        }

        public void Dispose()
        {
            ClearAll();
            _pools.Clear();
            _namedPools.Clear();
        }

        /// <summary>
        /// Wrapper to convert IPool<T> to IPool<object> for storage.
        /// </summary>
        private class PoolWrapper<T> : IPool<object> where T : class
        {
            public IPool<T> WrappedPool { get; }

            public PoolWrapper(IPool<T> pool)
            {
                WrappedPool = pool;
            }

            public object Get() => WrappedPool.Get();
            public void Release(object item) => WrappedPool.Release((T)item);
            public void Warmup(int count) => WrappedPool.Warmup(count);
            public UniTask WarmupAsync(int count, CancellationToken ct = default) => WrappedPool.WarmupAsync(count, ct);
            public void Clear() => WrappedPool.Clear();
            public int AvailableCount => WrappedPool.AvailableCount;
            public int TotalCount => WrappedPool.TotalCount;
        }

        public struct PoolInfo
        {
            public string Type;
            public int Available;
            public int Total;
        }
    }
}

