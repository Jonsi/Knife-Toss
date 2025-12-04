using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace KnifeToss.Infrastructure.Pooling
{
    /// <summary>
    /// MonoBehaviour-based pool for Unity Components. Use ComponentPool<T> for non-MonoBehaviour pools.
    /// </summary>
    public class GenericMonoPool<T> : MonoBehaviour, IPool<T> where T : Component
    {
        [SerializeField] private T prefab;
        [SerializeField] private int initialSize = 10;
        private readonly Queue<T> _queue = new();
        private int _totalCreated;

        public int AvailableCount => _queue.Count;
        public int TotalCount => _totalCreated;

        private void Awake()
        {
            Warmup(initialSize);
        }

        private T CreateInstance()
        {
            var inst = Instantiate(prefab, transform);
            inst.gameObject.SetActive(false);
            _totalCreated++;
            return inst;
        }

        public T Get()
        {
            T item;
            if (_queue.Count == 0)
            {
                item = CreateInstance();
            }
            else
            {
                item = _queue.Dequeue();
            }
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            _queue.Enqueue(item);
        }

        public void Warmup(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _queue.Enqueue(CreateInstance());
            }
        }

        public async UniTask WarmupAsync(int count, CancellationToken ct = default)
        {
            for (int i = 0; i < count; i++)
            {
                if (ct.IsCancellationRequested) break;
                _queue.Enqueue(CreateInstance());
                if (i % 5 == 0)
                {
                    await UniTask.Yield(ct);
                }
            }
        }

        public void Clear()
        {
            while (_queue.Count > 0)
            {
                var item = _queue.Dequeue();
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _totalCreated = 0;
        }
    }
}

