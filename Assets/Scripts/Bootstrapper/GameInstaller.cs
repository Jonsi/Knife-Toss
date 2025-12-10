using App.Services.Collision;
using App.Services.Flow;
using App.Services.Throwing;
using Infrastructure.Events;
using Infrastructure.Input;
using Infrastructure.Leveling;
using Infrastructure.Logging;
using Infrastructure.Settings;
using KnifeToss.Core.Abstractions.Collision;
using KnifeToss.Core.Abstractions.Config;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Gameplay;
using KnifeToss.Core.Abstractions.Input;
using KnifeToss.Core.Abstractions.Throwing;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Data.Level;
using KnifeToss.Core.Data.Throwing;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Domain.Level;
using KnifeToss.Presentation.Gameplay.Collision;
using KnifeToss.Presentation.Gameplay.Knife;
using KnifeToss.Presentation.Gameplay.Leveling;
using KnifeToss.Presentation.Gameplay.Target;
using KnifeToss.Presentation.UI.Popups;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bootstrapper
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
