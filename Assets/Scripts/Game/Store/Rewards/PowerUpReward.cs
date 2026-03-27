using System;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Persistence.Services;
using Game.PowerUps.Configs;
using UnityEngine;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Grants power-up inventory for a configured power-up type.
    /// </summary>
    [Serializable]
    public class PowerUpReward : ItemReward
    {
        [field: SerializeField]
        [field: Tooltip("Power-up definition; inventory key and display come from this asset.")]
        public PowerUpConfigBase Config { get; private set; }

        [field: SerializeField]
        [field: Tooltip("How many charges to add to saved power-up inventory.")]
        public int Quantity { get; private set; } = 1;

        /// <inheritdoc/>
        public override Sprite Icon => Config != null ? Config.Icon : null;

        /// <inheritdoc/>
        public override string GetDisplayText() =>
            Config != null && Quantity > 0
                ? $"+{Quantity} {Config.DisplayName}"
                : string.Empty;

        /// <inheritdoc/>
        public override UniTask GrantAsync(IRewardGrantContext context)
        {
            if (Config == null || string.IsNullOrEmpty(Config.InventoryKey) || Quantity <= 0)
            {
                return UniTask.CompletedTask;
            }

            var playerData = context.Resolve<PlayerDataService>();
            playerData.EnsurePowerUpInventory().IncrementCount(Config.InventoryKey, Quantity);
            playerData.SavePowerUpInventory();
            return UniTask.CompletedTask;
        }
    }
}
