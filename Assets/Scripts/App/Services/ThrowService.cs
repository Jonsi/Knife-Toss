using System;
using Core.Abstractions;
using Core.Data;

namespace App.Services
{
    /// <summary>
    /// Service that coordinates knife throwing actions based on player input.
    /// Handles the flow from input events to knife spawning via event channels.
    /// </summary>
    public class ThrowService : IThrowService
    {
        private readonly IPlayerThrowInputListener _playerThrowInputListener;
        private readonly IKnifeThrowConfigurationProvider _knifeThrowConfigurationProvider;
        private readonly IEventChannel<KnifeSpawnRequestData> _spawnRequestEventChannel;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        /// <param name="playerThrowInputListener">The listener that provides player throw input events.</param>
        /// <param name="knifeThrowConfigurationProvider">The configuration provider for knife throw parameters.</param>
        /// <param name="spawnRequestEventChannel">The event channel for raising knife spawn requests.</param>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener, 
            IKnifeThrowConfigurationProvider knifeThrowConfigurationProvider,
            IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel)
        {
            _playerThrowInputListener = playerThrowInputListener;
            _knifeThrowConfigurationProvider = knifeThrowConfigurationProvider;
            _spawnRequestEventChannel = spawnRequestEventChannel;
            
            _playerThrowInputListener.OnThrowRequested += HandleThrowRequested;
            _playerThrowInputListener.Initialize();
        }

        private void HandleThrowRequested()
        {
            KnifeThrowConfiguration config = _knifeThrowConfigurationProvider.GetConfiguration();
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffsetX = config.SpawnPositionOffsetX,
                SpawnOffsetY = config.SpawnPositionOffsetY,
                ThrowSpeed = config.ThrowSpeed
            };
            
            _spawnRequestEventChannel.Raise(spawnData);
        }

        /// <summary>
        /// Disposes of resources and unsubscribes from input events.
        /// </summary>
        public void Dispose()
        {
            _playerThrowInputListener.OnThrowRequested -= HandleThrowRequested;
        }
    }
}
