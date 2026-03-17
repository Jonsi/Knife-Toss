using Common.Events;
using Game.Flow.Events;
using Game.Flow.Interfaces;
using Game.Flow.Level;
using Game.Flow.Level.States;
using Game.Level.Data;
using Game.Level.Interfaces;
using Game.Level.Spawning;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs level feature services.
    /// </summary>
    public class LevelInstaller : IInstaller
    {
        private readonly Transform _LevelRoot;
        private readonly LevelCompletedEventChannel _CompletedChannel;
        private readonly LevelFailedEventChannel _FailedChannel;
        private readonly VoidEventChannel _FailFlowStartedChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelInstaller"/> class.
        /// </summary>
        /// <param name="levelRoot">Root transform for level spawning.</param>
        /// <param name="completedChannel">Level completed event channel.</param>
        /// <param name="failedChannel">Level failed event channel.</param>
        public LevelInstaller(
            Transform levelRoot,
            LevelCompletedEventChannel completedChannel,
            LevelFailedEventChannel failedChannel,
            VoidEventChannel failFlowStartedChannel)
        {
            _LevelRoot = levelRoot;
            _CompletedChannel = completedChannel;
            _FailedChannel = failedChannel;
            _FailFlowStartedChannel = failFlowStartedChannel;
        }

        /// <summary>
        /// Registers level feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.Register<LevelSpawner>(Lifetime.Singleton)
                .WithParameter(_LevelRoot)
                .As<ILevelSpawner>();
            builder.Register<LevelData>(Lifetime.Singleton);
            builder.RegisterInstance(_FailFlowStartedChannel);
            builder.Register<LevelStateMachine>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LevelFlowController>().As<ILevelFlowController>();
            builder.RegisterInstance(_CompletedChannel).AsSelf().As<IEventChannel<LevelData>>();
            builder.RegisterInstance(_FailedChannel).AsSelf().As<IEventChannel<LevelData>>();
        }
    }
}
