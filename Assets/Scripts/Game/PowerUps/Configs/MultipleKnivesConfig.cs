using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Multiple Knives power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "MultipleKnivesConfig", menuName = "Knife Toss/Power Ups/Multiple Knives Config")]
    public class MultipleKnivesConfig : PowerUpConfigBase
    {
        [field: SerializeField] public float[] Angles { get; private set; } = new float[] { -15f, 0f, 15f };
        [field: SerializeField] public float ThrowSpeed { get; private set; } = 10f;
    }
}
