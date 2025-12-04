using DG.Tweening;
using UnityEngine;

namespace KnifeToss.Presentation.UI
{
    [CreateAssetMenu(fileName = "RetryButtonAnimationConfig", menuName = "Knife Toss/Configurations/Animations/Retry Button Animation")]
    public sealed class RetryButtonAnimationConfig : ScriptableObject
    {
        [Header("Scale Animation")]
        [Tooltip("Minimum scale for the pulse animation.")]
        public float minScale = 0.9f;

        [Tooltip("Maximum scale for the pulse animation.")]
        public float maxScale = 1.1f;

        [Tooltip("Time (seconds) for one half of the pulse (min->max or max->min).")]
        public float pulseDuration = 0.5f;

        [Tooltip("Easing for the pulse animation.")]
        public Ease pulseEase = Ease.InOutSine;
    }
}
