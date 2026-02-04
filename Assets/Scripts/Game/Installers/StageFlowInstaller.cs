using Common.Events;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using Game.Flow.Stage;
using Game.Flow.Stage.States;
using Game.KnifeThrower.Interfaces;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Pooling;
using VContainer;
using VContainer.Unity;
using GameFlowController = Game.GameFlowController;

namespace Game.Installers
{
    /// <summary>
    /// Installs stage and gameplay flow services.
    /// Includes GameStateMachine which depends on stage-specific services.
    /// </summary>
    public class StageFlowInstaller : IInstaller
    {
        private readonly StagesCollection _StagesConfig;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly ReturnToMenuEventChannel _ReturnToMenuChannel;
        private readonly VoidEventChannel _RetryChannel;
        private readonly VoidEventChannel _RestartChannel;
        private readonly LevelStartedEventChannel _LevelStartedChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageFlowInstaller"/> class.
        /// </summary>
        public StageFlowInstaller(
            StagesCollection stagesConfig,
            StageCompletedEventChannel stageCompletedChannel,
            ReturnToMenuEventChannel returnToMenuChannel,
            VoidEventChannel retryChannel,
            VoidEventChannel restartChannel,
            LevelStartedEventChannel levelStartedChannel)
        {
            _StagesConfig = stagesConfig;
            _StageCompletedChannel = stageCompletedChannel;
            _ReturnToMenuChannel = returnToMenuChannel;
            _RetryChannel = retryChannel;
            _RestartChannel = restartChannel;
            _LevelStartedChannel = levelStartedChannel;
        }

        /// <summary>
        /// Registers stage and gameplay flow services.
        /// </summary>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StagesConfig);
            builder.Register<StagesConfigCatalog>(Lifetime.Singleton).As<ILevelCatalog>().AsSelf();
            builder.Register<StageData>(Lifetime.Singleton).WithParameter(0);
            builder.Register(CreateStageStateMachine, Lifetime.Singleton);
            builder.RegisterEntryPoint<StageFlowController>().As<IStageFlowController>();
            builder.RegisterInstance(_StageCompletedChannel);
            builder.RegisterInstance(_ReturnToMenuChannel);
            builder.RegisterInstance(_LevelStartedChannel).As<IEventChannel<LevelStartedEventData>>();

            builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();
            builder.RegisterEntryPoint<GameFlowController>();

#if TEST_ENABLED
            builder.Register<Game.Debugging.StageLevelCheats>(Lifetime.Singleton).AsSelf();
#endif
        }

        private StageStateMachine CreateStageStateMachine(IObjectResolver resolver)
        {
            return new StageStateMachine(
                resolver.Resolve<StageData>(),
                resolver.Resolve<ILevelFlowController>(),
                resolver.Resolve<ILevelCatalog>(),
                resolver.Resolve<Game.UI.Controllers.UIController>(),
                resolver.Resolve<LevelCompletedEventChannel>(),
                resolver.Resolve<LevelFailedEventChannel>(),
                _StageCompletedChannel,
                _RetryChannel,
                _RestartChannel,
                _LevelStartedChannel,
                resolver.Resolve<IKnifeThrowerView>(),
                resolver.Resolve<KnifePoolProvider>());
        }
    }
}

