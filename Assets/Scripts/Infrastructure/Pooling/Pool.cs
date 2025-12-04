using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using KnifeToss.Core.Abstractions.Pooling;

namespace KnifeToss.Infrastructure.Pooling
{
    /// <summary>
    /// Generic object pool for non-MonoBehaviour objects that implement IPoolable.
    /// </summary>
    public class Pool<T> : IPool<T> where T : class, IPoolable, new()
    {
        private readonly Stack<T> _available = new Stack<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly HashSet<T> _allItems = new HashSet<T>();
        private int _totalCreated;

        /// <summary>
        /// Creates a new pool with optional factory and callbacks.
        /// </summary>
        public Pool(
            Func<T> factory = null,
            Action<T> onGet = null,
            Action<T> onRelease = null)
        {
            _factory = factory ?? (() => new T());
            _onGet = onGet;
            _onRelease = onRelease;
        }

        public int AvailableCount => _available.Count;
        public int TotalCount => _totalCreated;

        public T Get()
        {
            T item;

            if (_available.Count > 0)
            {
                item = _available.Pop();
            }
            else
            {
                item = _factory();
                _totalCreated++;
                _allItems.Add(item);
            }

            _onGet?.Invoke(item);
            item.OnSpawn();
            return item;
        }

        public void Release(T item)
        {
            if (item == null)
            {
                return;
            }

            if (!_allItems.Contains(item))
            {
                // Item wasn't created by this pool, ignore
                return;
            }

            item.OnDespawn();
            _onRelease?.Invoke(item);
            _available.Push(item);
        }

        public void Warmup(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = _factory();
                _totalCreated++;
                _allItems.Add(item);
                item.OnDespawn(); // Initialize in despawned state
                _available.Push(item);
            }
        }

        public async UniTask WarmupAsync(int count, CancellationToken ct = default)
        {
            for (int i = 0; i < count; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                var item = _factory();
                _totalCreated++;
                _allItems.Add(item);
                item.OnDespawn();
                _available.Push(item);

                // Yield every few items to avoid frame spikes
                if (i % 5 == 0)
                {
                    await UniTask.Yield(ct);
                }
            }
        }

        public void Clear()
        {
            _available.Clear();
            _allItems.Clear();
            _totalCreated = 0;
        }
    }
}

