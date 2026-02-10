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
        [SerializeField] private PowerUpConfigBase _Config;
        [SerializeField] private int _Quantity = 1;

        public override UniTask GrantAsync(IObjectResolver resolver)
        {
            if (_Config == null || string.IsNullOrEmpty(_Config.InventoryKey) || _Quantity <= 0)
            {
                return UniTask.CompletedTask;
            }

            var playerData = resolver.Resolve<PlayerDataService>();
            playerData.EnsurePowerUpInventory().IncrementCount(_Config.InventoryKey, _Quantity);
            playerData.SavePowerUpInventory();
            return UniTask.CompletedTask;
        }
    }
}
