using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Base configuration for a power-up.
    /// </summary>
    public abstract class PowerUpConfigBase : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public string InventoryKey { get; private set; }

        /// <summary>
        /// Executes the power-up using the provided context.
        /// </summary>
        /// <param name="context">Execution context containing required services.</param>
        /// <returns>True if execution was successful.</returns>
        public abstract bool Execute(IPowerUpExecutionContext context);
    }
}
