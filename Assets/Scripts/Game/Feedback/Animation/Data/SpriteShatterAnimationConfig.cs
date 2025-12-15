using UnityEngine;

namespace Game.Feedback.Animation.Data
{
    /// <summary>
    /// Generic configuration for sprite shatter animation behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteShatterAnimationConfig", menuName = "Knife Toss/Animations/Shatter Animation Config")]
    public class SpriteShatterAnimationConfig : ScriptableObject
    {
        [field: SerializeField, Tooltip("Randomized number of shards spawned when the sprite shatters (inclusive range).")]
        public Vector2Int ShardCountRange { get; private set; } = new Vector2Int(6, 14);

        [field: SerializeField, Tooltip("Random local offset applied when spawning shards to avoid perfect overlap.")]
        public Vector2 SpawnJitter { get; private set; } = new(0.05f, 0.05f);

        [field: SerializeField, Tooltip("Inclusive min/max angular size (degrees) for radial slices. Sum always covers 360°.")]
        public Vector2 SliceAngleRange { get; private set; } = new(20f, 120f);

        [field: SerializeField, Tooltip("Normalized min/max length of linear shards relative to the sprite's largest dimension.")]
        public Vector2 LinearSliceSizeRange { get; private set; } = new(0.15f, 0.35f);

        [field: SerializeField, Tooltip("Horizontal (X axis) impulse applied to shards after spawning.")]
        public Vector2 HorizontalForceRange { get; private set; } = new(-3f, 3f);

        [field: SerializeField, Tooltip("Vertical (Y axis) impulse applied to shards after spawning.")]
        public Vector2 VerticalForceRange { get; private set; } = new(2f, 6f);

        [field: SerializeField, Tooltip("Downward acceleration applied to shards each frame.")]
        public float Gravity { get; private set; } = -20f;

        [field: SerializeField, Min(0f), Tooltip("Lerp factor that slows shards toward rest.")]
        public float Drag { get; private set; } = 3f;

        [field: SerializeField, Min(0.01f), Tooltip("Total lifetime before shards are cleaned up.")]
        public float Lifetime { get; private set; } = 1.8f;

        [field: SerializeField, Min(0f), Tooltip("Delay (seconds) before shard color starts fading.")]
        public float FadeDelay { get; private set; } = 0.3f;

        [field: SerializeField, Min(0.01f), Tooltip("Duration of the color fade from the original tint to TargetColor.")]
        public float FadeDuration { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("Target color (including alpha) when the fade completes.")]
        public Color TargetColor { get; private set; } = new(1f, 1f, 1f, 0f);
    }

    /// <summary>
    /// Backward-compatible configuration asset for target shatter animations.
    /// </summary>
    [CreateAssetMenu(fileName = "TargetShatterAnimationConfig", menuName = "Knife Toss/Animations/Target Shatter Animation Config")]
    public class TargetShatterAnimationConfig : SpriteShatterAnimationConfig
    {
    }
}
