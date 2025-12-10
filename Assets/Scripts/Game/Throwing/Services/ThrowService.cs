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
                spawnOffset = config.spawnPositionOffset,
                throwSpeed = config.throwSpeed
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
