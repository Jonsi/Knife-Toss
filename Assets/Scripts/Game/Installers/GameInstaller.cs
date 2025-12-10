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
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private KnifeThrowConfigAsset knifeThrowConfig;
        [SerializeField] private StagesConfigAsset stagesConfig;
        
        [Header("Event Channels")]
        [SerializeField] private KnifeSpawnRequestEventChannel knifeSpawnRequestEventChannel;
        [SerializeField] private CollisionEventChannel collisionEventChannel;
        [SerializeField] private KnifeImpactEventChannel knifeImpactEventChannel;
        [SerializeField] private LevelStateChangedEventChannel levelStateChangedEventChannel;
        
        [Header("Scene References")]
        [SerializeField] private KnifeThrowerView knifeThrowerView;
        [SerializeField] private Transform levelRoot;
        [SerializeField] private StageFailPopup stageFailPopup;
        
        [Header("Prefabs")]
        [SerializeField] private TargetView defaultTargetPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            GameSettingsAccess.Initialize(gameSettings);
            
            RegisterInfrastructure(builder);
            RegisterGameCore(builder);
            RegisterKnifeServices(builder);
            RegisterCollisionServices(builder);
        }

        private void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.RegisterInstance(stagesConfig);
            builder.Register<StagesConfigCatalog>(Lifetime.Singleton).As<ILevelCatalog>().AsSelf();
            
            builder.Register<RotationBehaviorFactory>(Lifetime.Singleton);
            
            builder.Register<TargetFactory>(Lifetime.Singleton)
                .WithParameter(defaultTargetPrefab);
            
            builder.Register<LevelSpawner>(Lifetime.Singleton)
                .WithParameter(levelRoot)
                .As<ILevelSpawner>();
        }

        private void RegisterGameCore(IContainerBuilder builder)
        {
            builder.RegisterInstance(CreateLogger()).As<ILogger>();

            builder.RegisterInstance(levelStateChangedEventChannel).As<IEventChannel<LevelStateChangedEventData>>();
            builder.RegisterInstance(knifeImpactEventChannel).As<IEventChannel<KnifeImpactEventData>>();

            builder.Register<StageData>(Lifetime.Singleton);
            builder.Register<StageStateMachine>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.RegisterEntryPoint<GameFlowController>(Lifetime.Singleton);

            if (stageFailPopup != null)
            {
                builder.RegisterComponent(stageFailPopup);
            }
        }

        private void RegisterKnifeServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(knifeThrowConfig).As<IConfigurationProvider<KnifeThrowConfiguration>>();
            builder.RegisterInstance(knifeSpawnRequestEventChannel).As<IEventChannel<KnifeSpawnRequestData>>();
            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.RegisterEntryPoint<ThrowService>(Lifetime.Singleton).As<IThrowService>();
            builder.RegisterComponent(knifeThrowerView).As<IKnifeThrowerView>();
        }

        private void RegisterCollisionServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(collisionEventChannel).As<IEventChannel<CollisionData>>();
            builder.Register<CollisionRouter>(Lifetime.Singleton).As<ICollisionRouter>();
            builder.Register<KnifeHitTargetHandler>(Lifetime.Singleton);
            builder.Register<KnifeHitKnifeHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CollisionProxy>(Lifetime.Singleton);
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
