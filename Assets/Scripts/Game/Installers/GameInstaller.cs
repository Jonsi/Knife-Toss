using Common.Collision.Data;
using Common.Collision.Events;
using Common.Collision.Interfaces;
using Common.Collision.Logic;
using Common.Config;
using Common.Events;
using Game.CollisionHandlers;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Events;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Factories;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using Game.Flow.Level;
using Game.Flow.Level.States;
using Game.Flow.Stage;
using Game.Flow.Stage.States;
using Game.Input;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.KnifeThrower.Services;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Level.Spawning;
using Game.Settings;
using Game.UI.Controllers;
using Game.UI.Popups;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Main composition root that configures dependency injection for all game systems.
    /// </summary>
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] private GameSettings _GameSettings;
        [SerializeField] private KnifeThrowConfigAsset _KnifeThrowConfig;
        [SerializeField] private StagesConfigAsset _StagesConfig;
        
        [Header("Event Channels")]
        [SerializeField] private KnifeSpawnRequestEventChannel _KnifeSpawnRequestEventChannel;
        [SerializeField] private CollisionEventChannel _CollisionEventChannel;
        [SerializeField] private KnifeImpactEventChannel _KnifeImpactEventChannel;
        
        [Header("Flow Event Channels")]
        [SerializeField] private LevelCompletedEventChannel _LevelCompletedEventChannel;
        [SerializeField] private LevelFailedEventChannel _LevelFailedEventChannel;
        [SerializeField] private StageCompletedEventChannel _StageCompletedEventChannel;
        [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuEventChannel;
        [SerializeField] private VoidEventChannel _StageRetryRequestChannel;
        [SerializeField] private VoidEventChannel _StageRestartRequestChannel;

        [Header("UI Event Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;
        
        [Header("Scene References")]
        [SerializeField] private KnifeThrowerView _KnifeThrowerView;
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private StageFailPopup _StageFailPopup;
        [SerializeField] private UIController _UIController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_GameSettings);
            RegisterInfrastructure(builder);
            RegisterGameCore(builder);
            RegisterKnifeServices(builder);
            RegisterCollisionServices(builder);
        }

        private void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StagesConfig);
            builder.Register<StagesConfigCatalog>(Lifetime.Singleton).As<ILevelCatalog>().AsSelf();
            
            builder.Register<RotationBehaviorFactory>(Lifetime.Singleton);
            
            builder.Register<TargetFactory>(Lifetime.Singleton)
                .WithParameter(_StagesConfig.DefaultTargetPrefab);
            
            builder.Register<LevelSpawner>(Lifetime.Singleton)
                .WithParameter(_LevelRoot)
                .As<ILevelSpawner>();
        }

        private void RegisterGameCore(IContainerBuilder builder)
        {
            builder.RegisterInstance(CreateLogger()).As<ILogger>();

            // Event Channels
            builder.RegisterInstance(_KnifeImpactEventChannel).As<IEventChannel<KnifeImpactEventData>>();
            builder.RegisterInstance(_LevelCompletedEventChannel);
            builder.RegisterInstance(_LevelFailedEventChannel);
            builder.RegisterInstance(_StageCompletedEventChannel);
            builder.RegisterInstance(_ReturnToMenuEventChannel);
            
            // NOTE: We do not register VoidEventChannel instances generically because there are multiple.
            // We pass them explicitly to consumers.

            // Level Flow
            builder.Register<LevelData>(Lifetime.Singleton);
            builder.Register<LevelStateMachine>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LevelFlowController>().As<ILevelFlowController>();

            // Stage Flow
            builder.Register<StageData>(Lifetime.Singleton).WithParameter(0);
            
            // Manual construction for StageStateMachine to inject specific VoidEventChannels
            builder.Register(c => new StageStateMachine(
                c.Resolve<StageData>(),
                c.Resolve<ILevelFlowController>(),
                c.Resolve<ILevelCatalog>(),
                c.Resolve<LevelCompletedEventChannel>(),
                c.Resolve<LevelFailedEventChannel>(),
                c.Resolve<StageCompletedEventChannel>(),
                _StageRetryRequestChannel,
                _StageRestartRequestChannel
            ), Lifetime.Singleton);

            builder.RegisterEntryPoint<StageFlowController>().As<IStageFlowController>();

            // Game Flow
            builder.Register<GameData>(Lifetime.Singleton);
            builder.Register<GameStateMachine>(Lifetime.Singleton);
            
            // Manual construction for GameFlowController to inject specific VoidEventChannels
            builder.Register(c => new GameFlowController(
                c.Resolve<GameStateMachine>(),
                c.Resolve<IStageFlowController>(),
                c.Resolve<StageCompletedEventChannel>(),
                c.Resolve<ReturnToMenuEventChannel>(),
                _RetryLevelChannel,
                _RestartStageChannel,
                _StageRetryRequestChannel,
                _StageRestartRequestChannel,
                c.Resolve<ILevelCatalog>()
            ), Lifetime.Singleton).AsImplementedInterfaces();

            if (_UIController != null)
            {
                builder.RegisterComponent(_UIController);
            }
            
            if (_StageFailPopup != null)
            {
                builder.RegisterComponent(_StageFailPopup);
            }
        }

        private void RegisterKnifeServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(_KnifeThrowConfig).As<IConfigurationProvider<KnifeThrowConfiguration>>();
            builder.RegisterInstance(_KnifeSpawnRequestEventChannel).As<IEventChannel<KnifeSpawnRequestData>>();
            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.RegisterEntryPoint<ThrowService>().As<IThrowService>();
            builder.RegisterComponent(_KnifeThrowerView).As<IKnifeThrowerView>();
        }

        private void RegisterCollisionServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(_CollisionEventChannel).As<IEventChannel<CollisionData>>();
            builder.RegisterEntryPoint<CollisionHandlerHandlerRouter>().As<ICollisionHandlerRouter>();
            builder.Register<KnifeHitTargetHandler>(Lifetime.Singleton);
            builder.Register<KnifeHitKnifeHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CollisionHandlerRegistrar>();
        }

        private ILogger CreateLogger()
        {
#if TEST_ENV
            ILogHandler handlerBase = new TestEnvLogHandler(Debug.unityLogger.logHandler, () => GameSettingsAccess.IsTestLoggingEnabled, includeStackTrace: false);
            return new Logger(handlerBase) { logEnabled = true };
#else
            return Debug.unityLogger;
#endif
        }
    }
}
