using UnityEngine;

namespace Game.Feedback.Animation.Data
{
    /// <summary>
    /// Configuration for target hit animation feedback.
    /// </summary>
    [CreateAssetMenu(fileName = "TargetHitAnimationConfig", menuName = "Knife Toss/Animations/Target Hit Animation Config")]
    public class TargetHitAnimationConfig : ScriptableObject
    {
        [field: SerializeField] public float ShakeDuration { get; private set; } = 0.25f;
        [field: SerializeField] public float ShakeStrength { get; private set; } = 15f;
        [field: SerializeField] public int ShakeVibrato { get; private set; } = 20;
        [field: SerializeField] public float ShakeRandomness { get; private set; } = 45f;
        [field: SerializeField] public bool ShakeFadeOut { get; private set; } = true;

        [field: SerializeField] public Color TintColor { get; private set; } = new Color(1f, 0.9f, 0.7f);
        [field: SerializeField] public float TintFadeIn { get; private set; } = 0.05f;
        [field: SerializeField] public float TintHold { get; private set; } = 0.05f;
        [field: SerializeField] public float TintFadeOut { get; private set; } = 0.2f;
    }
}

