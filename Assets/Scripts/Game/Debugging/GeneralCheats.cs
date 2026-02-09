#if TEST_ENABLED
using Game.Flow.Game;
using Game.Persistence.Data;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using UnityEngine;

namespace Game.Debugging
{
    public class GeneralCheats
    {
        private readonly GameData _gameData;
        private readonly PlayerDataService _playerDataService;

        public GeneralCheats(GameData gameData, PlayerDataService playerDataService)
        {
            _gameData = gameData;
            _playerDataService = playerDataService;
        }

        public void ClearProgress()
        {
            if (Application.isPlaying)
            {
                if (_gameData == null) return;
                _gameData.Reset();
                _playerDataService?.ResetAllData();
            }
            else
            {
                var store = new FilePlayerDataStore();
                store.Save(new PlayerData());
            }
        }
    }
}
#endif
