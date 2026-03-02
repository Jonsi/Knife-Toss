using System;
using Cysharp.Threading.Tasks;
using Game.Persistence.Services;
using Game.PowerUps.Configs;
using UnityEngine;
using VContainer;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Grants power-up inventory for a configured power-up type.
    /// </summary>
    [Serializable]
    public class PowerUpReward : ItemReward
    {
        [field: SerializeField] public PowerUpConfigBase Config { get; private set; }
        [field: SerializeField] public int Quantity { get; private set; } = 1;

        public override UniTask GrantAsync(IObjectResolver resolver)
        {
            if (Config == null || string.IsNullOrEmpty(Config.InventoryKey) || Quantity <= 0)
            {
                return UniTask.CompletedTask;
            }

            var playerData = resolver.Resolve<PlayerDataService>();
            playerData.EnsurePowerUpInventory().IncrementCount(Config.InventoryKey, Quantity);
            playerData.SavePowerUpInventory();
            return UniTask.CompletedTask;
        }
    }
}
