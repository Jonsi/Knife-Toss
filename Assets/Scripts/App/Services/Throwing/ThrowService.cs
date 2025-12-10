using System;
using KnifeToss.Core.Abstractions.Config;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Abstractions.Input;
using KnifeToss.Core.Abstractions.Throwing;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Data.Throwing;
using VContainer.Unity;

namespace App.Services.Throwing
{
    /// <summary>
    /// Service that coordinates knife throwing actions based on player input.
    /// Handles the flow from input events to knife spawning via event channels.
    /// </summary>
    public class ThrowService : IThrowService, IStartable
    {
        private readonly IPlayerThrowInputListener _PlayerThrowInputListener;
        private readonly IConfigurationProvider<KnifeThrowConfiguration> _KnifeThrowConfigurationProvider;
        private readonly IEventChannel<KnifeSpawnRequestData> _SpawnRequestEventChannel;
        private readonly ILevelSessionService _LevelSessionService;
        private bool _IsRunning;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        /// <param name="playerThrowInputListener">The listener that provides player throw input events.</param>
        /// <param name="knifeThrowConfigurationProvider">The configuration provider for knife throw parameters.</param>
        /// <param name="spawnRequestEventChannel">The event channel for raising knife spawn requests.</param>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener, 
            IConfigurationProvider<KnifeThrowConfiguration> knifeThrowConfigurationProvider,
            IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel,
            ILevelSessionService levelSessionService)
        {
            _PlayerThrowInputListener = playerThrowInputListener;
            _KnifeThrowConfigurationProvider = knifeThrowConfigurationProvider;
            _SpawnRequestEventChannel = spawnRequestEventChannel;
            _LevelSessionService = levelSessionService;
        }

        /// <summary>
        /// Called automatically by VContainer when the LifetimeScope starts.
        /// Initializes the input listener and subscribes to throw events.
        /// </summary>
        public void Start()
        {
            if (_IsRunning)
                return;

            _PlayerThrowInputListener.OnThrowRequested += HandleThrowRequested;
            _PlayerThrowInputListener.Initialize();
            _IsRunning = true;
        }

        private void HandleThrowRequested()
        {
            if (!_LevelSessionService.TryRegisterThrow())
                return;

            KnifeThrowConfiguration config = _KnifeThrowConfigurationProvider.GetConfiguration();
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = config.SpawnPositionOffset,
                ThrowSpeed = config.ThrowSpeed
            };
            
            _SpawnRequestEventChannel.Raise(spawnData);
        }

        /// <summary>
        /// Disposes of resources and unsubscribes from input events.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Stops listening for throw input and cleans up resources.
        /// </summary>
        public void Stop()
        {
            if (!_IsRunning)
                return;

            _PlayerThrowInputListener.OnThrowRequested -= HandleThrowRequested;
            _PlayerThrowInputListener.Dispose();
            _IsRunning = false;
        }
    }
}
