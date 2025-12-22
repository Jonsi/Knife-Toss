using System;
using UnityEngine;

namespace Game.Configs.Prefabs
{
    /// <summary>
    /// Configuration entry for a typed prefab with pooling settings.
    /// </summary>
    [Serializable]
    public class FactoryPrefabEntry<T> where T : Component
    {
        private const int DefaultWarmupCount = 5;
        private const int DefaultMaxPoolSize = 20;

        [SerializeField] private T _Prefab;
        [SerializeField] private int _WarmupCount = DefaultWarmupCount;
        [SerializeField] private int _MaxSize = DefaultMaxPoolSize;

        /// <summary>
        /// Gets the typed prefab reference.
        /// </summary>
        public T Prefab => _Prefab;

        /// <summary>
        /// Gets the pool warmup count.
        /// </summary>
        public int WarmupCount => Mathf.Max(0, _WarmupCount);

        /// <summary>
        /// Gets the maximum pool size.
        /// </summary>
        public int MaxSize => Mathf.Max(1, _MaxSize);
    }
}
