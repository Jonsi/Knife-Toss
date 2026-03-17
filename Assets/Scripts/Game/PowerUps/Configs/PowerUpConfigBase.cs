using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Base configuration for a power-up.
    /// </summary>
    public abstract class PowerUpConfigBase : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public string InventoryKey { get; private set; }
        [field: SerializeField] public GameObject VfxPrefab { get; private set; }
        [field: SerializeField] public Vector3 VfxLocalOffset { get; private set; } = Vector3.zero;
        [field: SerializeField] public Vector3 VfxLocalScale { get; private set; } = Vector3.one;
        [field: SerializeField] public int VfxSortingOrderOffset { get; private set; } = 2;
        [field: SerializeField] public KnifePowerUpVfxSpawnMode VfxSpawnMode { get; private set; } = KnifePowerUpVfxSpawnMode.Child;
        [field: SerializeField] public bool DestroyVfxOnHit { get; private set; } = false;

        /// <summary>
        /// Executes the power-up using the provided context.
        /// </summary>
        /// <param name="context">Execution context containing required services.</param>
        /// <returns>True if execution was successful.</returns>
        public abstract bool Execute(IPowerUpExecutionContext context);

        /// <summary>
        /// Optionally prepares the loaded knives for this power-up.
        /// </summary>
        /// <param name="knives">List of knives to prepare.</param>
        public virtual void PrepareKnives(System.Collections.Generic.IReadOnlyList<Game.Entities.Knife.Views.KnifeView> knives)
        {
        }
    }
}
