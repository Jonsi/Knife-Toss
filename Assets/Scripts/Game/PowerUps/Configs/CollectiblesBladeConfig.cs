using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Collectibles Blade power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectiblesBladeConfig", menuName = "Knife Toss/Power Ups/Collectibles Blade Config")]
    public class CollectiblesBladeConfig : PowerUpConfigBase
    {
        [field: SerializeField] public float TimeBetweenBlades { get; private set; } = 0.15f;
        [field: SerializeField] public float BladeSpeed { get; private set; } = 12f;
    }
}
