using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using KnifeToss.Core.Abstractions.Pooling;

namespace KnifeToss.Infrastructure.Pooling
{
    /// <summary>
    /// Pool for Unity Component types (MonoBehaviour, Component, etc.).
    /// Handles GameObject lifecycle automatically.
    /// </summary>
    public class ComponentPool<T> : IPool<T> where T : Component
    {
        private readonly Stack<T> _available = new Stack<T>();
        private readonly Func<T> _factory;
        private readonly Transform _parent;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly HashSet<T> _allItems = new HashSet<T>();
        private int _totalCreated;

        /// <summary>
        /// Creates a new component pool.
        /// </summary>
        /// <param name="factory">Factory function to create new instances</param>
        /// <param name="parent">Parent transform for pooled objects (optional)</param>
        /// <param name="onGet">Callback when item is retrieved from pool</param>
        /// <param name="onRelease">Callback when item is returned to pool</param>
        public ComponentPool(
            Func<T> factory,
            Transform parent = null,
            Action<T> onGet = null,
            Action<T> onRelease = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _parent = parent;
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

                if (_parent != null && item.transform.parent != _parent)
                {
                    item.transform.SetParent(_parent, false);
                }
            }

            // Set active before calling OnSpawn
            if (!item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(true);
            }

            _onGet?.Invoke(item);

            // Call IPoolable if implemented
            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

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

            // Call IPoolable if implemented
            if (item is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            _onRelease?.Invoke(item);

            // Deactivate GameObject
            if (item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(false);
            }

            // Reset transform if parent is set
            if (_parent != null && item.transform.parent != _parent)
            {
                item.transform.SetParent(_parent, false);
            }

            _available.Push(item);
        }

        public void Warmup(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = _factory();
                _totalCreated++;
                _allItems.Add(item);

                if (_parent != null)
                {
                    item.transform.SetParent(_parent, false);
                }

                // Initialize in despawned state
                item.gameObject.SetActive(false);

                if (item is IPoolable poolable)
                {
                    poolable.OnDespawn();
                }

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

                if (_parent != null)
                {
                    item.transform.SetParent(_parent, false);
                }

                item.gameObject.SetActive(false);

                if (item is IPoolable poolable)
                {
                    poolable.OnDespawn();
                }

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
            // Destroy all GameObjects
            foreach (var item in _allItems)
            {
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }

            _available.Clear();
            _allItems.Clear();
            _totalCreated = 0;
        }
    }
}

