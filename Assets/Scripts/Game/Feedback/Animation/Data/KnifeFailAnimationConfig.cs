using UnityEngine;

namespace Game.Feedback.Animation.Data
{
    /// <summary>
    /// Configuration for the knife fail feedback sequence after a knife-to-knife hit.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeFailAnimationConfig", menuName = "Knife Toss/Systems/Animation/Knife Fail")]
    public class KnifeFailAnimationConfig : ScriptableObject
    {
        [field: Header("Flash")]
        [field: SerializeField, Min(0f)] public float FlashDuration { get; private set; } = 0.08f;
        [field: SerializeField] public AnimationCurve FlashAlphaCurve { get; private set; } =
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 1f),
                new Keyframe(1f, 0f));

        [field: Header("Sequence")]
        [field: SerializeField, Min(0.01f)] public float SequenceDuration { get; private set; } = 0.65f;
        [field: SerializeField, Tooltip("Normalized timeline (0-1 over the full fail sequence) mapped directly to Time.timeScale.")]
        public AnimationCurve TimeScaleOverSequence { get; private set; } =
            new AnimationCurve(
                new Keyframe(0f, 0.45f),
                new Keyframe(0.7f, 0.32f),
                new Keyframe(1f, 1f));
        [field: SerializeField, Tooltip("Knife alpha over the full fail sequence.")]
        public AnimationCurve KnifeAlphaOverSequence { get; private set; } =
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.55f, 1f),
                new Keyframe(1f, 0f));

        [field: Header("Bounce Physics")]
        [field: SerializeField] public Vector2 InitialVelocity { get; private set; } = new Vector2(0f, 6f);
        [field: SerializeField] public float AngularVelocity { get; private set; } = 360f;
        [field: SerializeField] public float Gravity { get; private set; } = -20f;
    }
}
