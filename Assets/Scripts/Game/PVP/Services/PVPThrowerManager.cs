using Game.Entities.Knife.Views;
using Game.Entities.Knife.Data;
using UnityEngine;
using Game.PVP.Configs;
using VContainer;
using VContainer.Unity;

namespace Game.PVP.Services
{
    public class PVPThrowerManager
    {
        private readonly KnifeThrowerView _Player1Thrower; // Bottom
        private readonly KnifeThrowerView _Player2Thrower; // Top
        private readonly PVPSettingsConfig _Config;
        private readonly IObjectResolver _Container;
        
        public PVPThrowerManager(
            KnifeThrowerView player1Thrower,
            KnifeThrowerView player2Thrower,
            PVPSettingsConfig config,
            IObjectResolver container)
        {
            _Player1Thrower = player1Thrower;
            _Player2Thrower = player2Thrower;
            _Config = config;
            _Container = container;
        }

        public void LoadKnifeForPlayer(bool isPlayer1, KnifeView knifePrefab)
        {
            var knife = _Container.Instantiate(knifePrefab);
            
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = Vector2.zero
            };

            if (isPlayer1)
            {
                knife.transform.rotation = Quaternion.identity;
                _Player1Thrower.LoadKnife(knife, spawnData);
            }
            else
            {
                _Player2Thrower.LoadKnife(knife, spawnData);
                
                // Fix rotation for Player 2 (Top, pointing down)
                // LoadKnife sets it to Identity (up). We want 180 (down).
                if (_Player2Thrower.LoadedKnife != null)
                {
                     _Player2Thrower.LoadedKnife.transform.rotation = Quaternion.Euler(0, 0, 180f);
                }
            }
        }

        public void ThrowFromPlayer(bool isPlayer1)
        {
            if (isPlayer1)
            {
                _Player1Thrower.ThrowLoadedKnifeWithDirection(Vector2.up, _Config.ThrowSpeed);
            }
            else
            {
                _Player2Thrower.ThrowLoadedKnifeWithDirection(Vector2.down, _Config.ThrowSpeed);
            }
        }
    }
}
