using KnifeToss.Common.Config;
using KnifeToss.Common.Events;
using KnifeToss.Game.Knife.Data;
using KnifeToss.Game.Level.Data;
using KnifeToss.Game.Throwing.Data;
using KnifeToss.Game.Throwing.Interfaces;
using VContainer.Unity;

namespace KnifeToss.Game.Throwing.Services
{
    /// <summary>
    /// Service that coordinates knife throwing actions based on player input.
    /// Handles the flow from input events to knife spawning via event channels.
    /// </summary>
    public class ThrowService : IThrowService, IStartable
    {
        private readonly IPlayerThrowInputListener _playerThrowInputListener;
        private readonly IConfigurationProvider<KnifeThrowConfiguration> _knifeThrowConfigurationProvider;
        private readonly IEventChannel<KnifeSpawnRequestData> _spawnRequestEventChannel;
        private readonly StageData _stageData;
        private bool _isRunning;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener, 
            IConfigurationProvider<KnifeThrowConfiguration> knifeThrowConfigurationProvider,
            IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel,
            StageData stageData)
        {
            _playerThrowInputListener = playerThrowInputListener;
            _knifeThrowConfigurationProvider = knifeThrowConfigurationProvider;
            _spawnRequestEventChannel = spawnRequestEventChannel;
            _stageData = stageData;
        }

        /// <summary>
        /// Called automatically by VContainer when the LifetimeScope starts.
        /// Initializes the input listener and subscribes to throw events.
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            _playerThrowInputListener.OnThrowRequested += HandleThrowRequested;
            _playerThrowInputListener.Initialize();
            _isRunning = true;
        }

        private void HandleThrowRequested()
        {
            if (!_stageData.IsThrowingActive || !_stageData.HasKnivesRemaining)
                return;

            var config = _knifeThrowConfigurationProvider.GetConfiguration();
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = config.SpawnPositionOffset,
                ThrowSpeed = config.ThrowSpeed
            };
            
            _spawnRequestEventChannel.Raise(spawnData);
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
            if (!_isRunning)
                return;

            _playerThrowInputListener.OnThrowRequested -= HandleThrowRequested;
            _playerThrowInputListener.Dispose();
            _isRunning = false;
        }
    }
}
