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
using Game.Input;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.KnifeThrower.Services;
using Game.Level.Catalogs;
using Game.Level.Data;
using Game.Level.Events;
using Game.Level.Interfaces;
using Game.Level.Spawning;
using Game.Level.States;
using Game.Settings;
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
        [SerializeField] private LevelStateChangedEventChannel _LevelStateChangedEventChannel;
        
        [Header("Scene References")]
        [SerializeField] private KnifeThrowerView _KnifeThrowerView;
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private StageFailPopup _StageFailPopup;

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

            builder.RegisterInstance(_LevelStateChangedEventChannel).As<IEventChannel<LevelStateChangedEventData>>();
            builder.RegisterInstance(_KnifeImpactEventChannel).As<IEventChannel<KnifeImpactEventData>>();

            builder.Register<StageData>(Lifetime.Singleton);
            builder.Register<StageStateMachine>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.RegisterEntryPoint<GameFlowController>();

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
