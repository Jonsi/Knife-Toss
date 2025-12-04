using UnityEngine;

namespace KnifeToss.Core.Data
{
    /// <summary>
    /// Configuration for knife magazine (queue) settings.
    /// </summary>
    [CreateAssetMenu(fileName = "MagazineConfig", menuName = "Knife Toss/Configurations/Magazine Config")]
    public class MagazineConfig : ScriptableObject
    {
        [Header("Pool Settings")]
        [Tooltip("Number of knives to pre-warm in the pool")]
        public int poolWarmupCount = 10;

        [Header("Queue Settings")]
        [Tooltip("Number of knives visible in the queue")]
        public int visibleQueueCount = 5;

        [Tooltip("Spacing between queued knives")]
        public float queueSpacing = 0.5f;

        [Tooltip("Duration of slide animation when queue moves forward")]
        public float slideAnimationDuration = 0.3f;

        [Tooltip("Scale multiplier for the last visible knife in queue")]
        [Range(0f, 1f)]
        public float queueScaleEnd = 0.5f;

        [Tooltip("Opacity multiplier for the last visible knife in queue")]
        [Range(0f, 1f)]
        public float queueOpacityEnd = 0.3f;
    }
}

