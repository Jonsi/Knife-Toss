using DG.Tweening;
using UnityEngine;

namespace Game.Feedback.Animation.Data
{
    /// <summary>
    /// Configuration for the target spawn/enter animation.
    /// Controls scale pop animation.
    /// </summary>
    [CreateAssetMenu(fileName = "TargetSpawnAnimationConfig", menuName = "Knife Toss/Animations/Target Spawn Animation Config")]
    public class TargetSpawnAnimationConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f), Tooltip("Duration of the pop-out scale-up stage (seconds).")]
        public float ScaleUpDuration { get; private set; } = 0.28f;

        [field: SerializeField, Min(0f), Tooltip("Duration of the settle-back-to-final-scale stage (seconds).")]
        public float ScaleSettleDuration { get; private set; } = 0.15f;

        [field: SerializeField, Min(1f), Tooltip("Overshoot multiplier applied during the pop to add a bounce.")]
        public float ScaleOvershootMultiplier { get; private set; } = 1.08f;

        [field: SerializeField, Tooltip("Ease used while scaling up.")]
        public Ease ScaleUpEase { get; private set; } = Ease.OutBack;

        [field: SerializeField, Tooltip("Ease used while settling to the final scale.")]
        public Ease ScaleSettleEase { get; private set; } = Ease.OutQuad;
    }
}
