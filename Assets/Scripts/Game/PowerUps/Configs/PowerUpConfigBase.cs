using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Items.Configs;
using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using Game.Persistence.Services;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Base configuration for a power-up.
    /// </summary>
    public abstract class PowerUpConfigBase : ItemConfigurationBase
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

        /// <inheritdoc/>
        public override string GetItemDisplayName() => DisplayName;

        /// <inheritdoc/>
        public override Sprite GetItemIcon() => Icon;

        /// <inheritdoc/>
        public override UniTask GrantAsync(int count, IRewardGrantContext context)
        {
            if (string.IsNullOrEmpty(InventoryKey) || count <= 0)
            {
                return UniTask.CompletedTask;
            }

            var playerData = context.Resolve<PlayerDataService>();
            playerData.EnsurePowerUpInventory().IncrementCount(InventoryKey, count);
            playerData.SavePowerUpInventory();
            return UniTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override string GetRewardDisplayText(int count) =>
            string.IsNullOrEmpty(DisplayName) || count <= 0
                ? string.Empty
                : $"+{count} {DisplayName}";

        /// <summary>
        /// Optionally prepares the loaded knives for this power-up.
        /// </summary>
        /// <param name="knives">List of knives to prepare.</param>
        public virtual void PrepareKnives(System.Collections.Generic.IReadOnlyList<Game.Entities.Knife.Views.KnifeView> knives)
        {
        }
    }
}
