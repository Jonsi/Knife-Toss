using System;
using UnityEngine;

namespace Game.Pooling
{
    /// <summary>
    /// Base configuration for a pooled prefab.
    /// </summary>
    [Serializable]
    public class PrefabEntry
    {
        [SerializeField] private GameObject _Prefab;
        [SerializeField] private int _WarmupCount = 5;
        [SerializeField] private int _MaxSize = 20;

        public GameObject Prefab => _Prefab;
        public int WarmupCount => Mathf.Max(0, _WarmupCount);
        public int MaxSize => Mathf.Max(1, _MaxSize);
    }
}
