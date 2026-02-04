#if TEST_ENABLED
using Game.Persistence.Services;
using Game.Persistence.Storage;
using Game.PowerUps.Configs;
using UnityEngine;
using System.Linq;

namespace Game.Debugging
{
    public class PowerUpCheats
    {
        private readonly PlayerDataService _playerDataService;

        public PowerUpCheats(PlayerDataService playerDataService)
        {
            _playerDataService = playerDataService;
        }

        public int GetPowerUpCount(string inventoryKey)
        {
            if (Application.isPlaying && _playerDataService != null)
            {
                return _playerDataService.EnsurePowerUpInventory().GetCount(inventoryKey);
            }
            else
            {
                var store = new FilePlayerDataStore();
                var data = store.Load();
                return data?.Resources?.PowerUps?.GetCount(inventoryKey) ?? 0;
            }
        }

        public void SetPowerUpCount(string inventoryKey, int count)
        {
            if (Application.isPlaying && _playerDataService != null)
            {
                var inventory = _playerDataService.EnsurePowerUpInventory();
                inventory.SetCount(inventoryKey, count);
                _playerDataService.RequestSave();
            }
            else
            {
                var store = new FilePlayerDataStore();
                var data = store.Load();
                if (data == null) data = new Game.Persistence.Data.PlayerData();
                if (data.Resources == null) data.Resources = new Game.Persistence.Data.PlayerResourcesData();
                if (data.Resources.PowerUps == null) data.Resources.PowerUps = new Game.PowerUps.Data.PowerUpInventoryData();
                
                data.Resources.PowerUps.SetCount(inventoryKey, count);
                store.Save(data);
            }
        }

        public PowerUpCatalog GetCatalog()
        {
            return Resources.Load<PowerUpCatalog>("PowerUpCatalog");
        }
    }
}
#endif
