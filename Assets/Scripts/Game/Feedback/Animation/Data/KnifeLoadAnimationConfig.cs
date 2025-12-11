using UnityEngine;

namespace Game.Feedback.Animation.Data
{
    /// <summary>
    /// Configuration for knife spawn/load animation.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeLoadAnimationConfig", menuName = "Knife Toss/Animations/Knife Load Animation Config")]
    public class KnifeLoadAnimationConfig : ScriptableObject
    {
        [field: SerializeField] public float Duration { get; private set; } = 0.3f;
        [field: SerializeField] public float VerticalOffset { get; private set; } = 0.5f;
        [field: SerializeField] public float FadeFromAlpha { get; private set; } = 0f;
        [field: SerializeField] public float TargetAlpha { get; private set; } = 1f;
    }
}

