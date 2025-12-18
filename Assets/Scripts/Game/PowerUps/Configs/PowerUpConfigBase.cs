using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Base configuration for a power-up.
    /// </summary>
    public abstract class PowerUpConfigBase : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
    }
}
