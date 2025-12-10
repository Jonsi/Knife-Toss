using Common.Config;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.Level.Data;
using VContainer.Unity;

namespace Game.KnifeThrower.Services
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
        private readonly StageData _StageData;
        private bool _IsRunning;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener, 
            IConfigurationProvider<KnifeThrowConfiguration> knifeThrowConfigurationProvider,
            IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel,
            StageData stageData)
        {
            _PlayerThrowInputListener = playerThrowInputListener;
            _KnifeThrowConfigurationProvider = knifeThrowConfigurationProvider;
            _SpawnRequestEventChannel = spawnRequestEventChannel;
            _StageData = stageData;
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
            if (!_StageData.IsThrowingActive || !_StageData.HasKnivesRemaining)
                return;

            var config = _KnifeThrowConfigurationProvider.GetConfiguration();
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
