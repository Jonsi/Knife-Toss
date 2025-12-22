using Game.DI;
using Common.Events;
using Game.App.Flow.Events;
using Game.Flow.Interfaces;
using Game.Flow.Stage;
using Game.Flow.Stage.States;
using Game.KnifeThrower.Interfaces;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Pooling;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs stage feature services.
    /// </summary>
    [CreateAssetMenu(fileName = "StageInstaller", menuName = "Knife Toss/Installers/Stage")]
    public class StageInstallerSO : BaseInstallerSO
    {
        [Header("Configs")]
        [SerializeField] private StagesConfigAsset _StagesConfig;

        [Header("Event Channels")]
        [SerializeField] private StageCompletedEventChannel _StageCompletedChannel;
        [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuChannel;
        [SerializeField] private VoidEventChannel _RetryChannel;
        [SerializeField] private VoidEventChannel _RestartChannel;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;

        /// <summary>
        /// Registers stage feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StagesConfig);
            builder.Register<StagesConfigCatalog>(Lifetime.Singleton).As<ILevelCatalog>().AsSelf();
            builder.Register<StageData>(Lifetime.Singleton).WithParameter(0);
            builder.Register(CreateStageStateMachine, Lifetime.Singleton);
            builder.Register<StageFlowController>(Lifetime.Singleton).As<IStageFlowController>();
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

