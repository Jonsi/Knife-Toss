using Common.Events;
using Game.Flow.Events;
using Game.Flow.Interfaces;
using Game.Flow.Stage;
using Game.Flow.Stage.States;
using Game.KnifeThrower.Interfaces;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Pooling;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs stage feature services.
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
        /// <param name="stagesConfig">Stages configuration asset.</param>
        /// <param name="stageCompletedChannel">Stage completed event channel.</param>
        /// <param name="returnToMenuChannel">Return to menu event channel.</param>
        /// <param name="retryChannel">Retry level request channel.</param>
        /// <param name="restartChannel">Restart stage request channel.</param>
        /// <param name="levelStartedChannel">Level started event channel.</param>
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
        /// Registers stage feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StagesConfig);
            builder.Register<StagesConfigCatalog>(Lifetime.Singleton).As<ILevelCatalog>().AsSelf();
            builder.Register<StageData>(Lifetime.Singleton).WithParameter(UnityEngine.PlayerPrefs.GetInt("SelectedStageIndex", 0));
            builder.Register(CreateStageStateMachine, Lifetime.Singleton);
            builder.RegisterEntryPoint<StageFlowController>().As<IStageFlowController>();
            builder.RegisterInstance(_StageCompletedChannel);
            builder.RegisterInstance(_ReturnToMenuChannel);
            builder.RegisterInstance(_LevelStartedChannel).As<IEventChannel<LevelStartedEventData>>();
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

