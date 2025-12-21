using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Multiple Knives power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "MultipleKnivesConfig", menuName = "Knife Toss/Power Ups/Multiple Knives Config")]
    public class MultipleKnivesConfig : PowerUpConfigBase
    {
        [field: SerializeField, Min(1)] public int KnifeCount { get; private set; } = 3;
        [field: SerializeField, Min(0f)] public float KnifeSpacing { get; private set; } = 0.35f;
        [field: SerializeField] public float ThrowSpeed { get; private set; } = 10f;
    }
}
