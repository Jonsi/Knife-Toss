using System;
using UnityEngine;

namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Serializable sound data that can trim leading silence via a start offset.
    /// </summary>
    [Serializable]
    public sealed class SoundEffect
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Min(0f)] private float startOffsetSeconds;

        public AudioClip Clip => clip;
        public float StartOffsetSeconds => Mathf.Max(0f, startOffsetSeconds);
        public bool HasClip => clip != null;
    }
}
