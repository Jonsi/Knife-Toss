using KnifeToss.Common.Config;
using KnifeToss.Common.Events;
using KnifeToss.Game.Collision.Data;
using KnifeToss.Game.Collision.Events;
using KnifeToss.Game.Collision.Interfaces;
using KnifeToss.Game.Collision.Logic;
using KnifeToss.Game.Collision.Logic.Handlers;
using KnifeToss.Game.Collision.Services;
using KnifeToss.Game.Knife.Data;
using KnifeToss.Game.Knife.Events;
using KnifeToss.Game.Knife.Presentation;
using KnifeToss.Game.Level.Catalogs;
using KnifeToss.Game.Level.Data;
using KnifeToss.Game.Level.Events;
using KnifeToss.Game.Level.Interfaces;
using KnifeToss.Game.Level.Spawning;
using KnifeToss.Game.Level.States;
using KnifeToss.Game.Systems.Flow;
using KnifeToss.Game.Systems.Input;
using KnifeToss.Game.Systems.Settings;
using KnifeToss.Game.Target.Factories;
using KnifeToss.Game.Target.Presentation;
using KnifeToss.Game.Throwing.Configs;
using KnifeToss.Game.Throwing.Data;
using KnifeToss.Game.Throwing.Interfaces;
using KnifeToss.Game.Throwing.Services;
using KnifeToss.Game.UI.Popups;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KnifeToss.Game.Installers
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
        
        [Header("Prefabs")]
        [SerializeField] private TargetView _DefaultTargetPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            GameSettingsAccess.Initialize(_GameSettings);
            
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
                .WithParameter(_DefaultTargetPrefab);
            
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

            builder.RegisterEntryPoint<GameFlowController>(Lifetime.Singleton);

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
            builder.RegisterEntryPoint<ThrowService>(Lifetime.Singleton).As<IThrowService>();
            builder.RegisterComponent(_KnifeThrowerView).As<IKnifeThrowerView>();
        }

        private void RegisterCollisionServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(_CollisionEventChannel).As<IEventChannel<CollisionData>>();
            builder.RegisterEntryPoint<CollisionHandlerHandlerRouter>(Lifetime.Singleton).As<ICollisionHandlerRouter>();
            builder.Register<KnifeHitTargetHandler>(Lifetime.Singleton);
            builder.Register<KnifeHitKnifeHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CollisionHandlerRegistrar>(Lifetime.Singleton);
        }

        private ILogger CreateLogger()
        {
#if TEST_ENV
            ILogHandler handler = new TestEnvLogHandler(Debug.unityLogger.logHandler, () => GameSettingsAccess.IsTestLoggingEnabled, includeStackTrace: false);
            return new Logger(handler) { logEnabled = true };
#else
            return Debug.unityLogger;
#endif
        }
    }
}
