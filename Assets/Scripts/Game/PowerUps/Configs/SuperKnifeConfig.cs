using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Super Knife power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "SuperKnifeConfig", menuName = "Knife Toss/Power Ups/Super Knife Config")]
    public class SuperKnifeConfig : PowerUpConfigBase
    {
        [field: SerializeField] public float ThrowSpeed { get; private set; } = 10f;
    }
}
