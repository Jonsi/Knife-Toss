using App.Services.Collision;
using App.Services.Flow;
using App.Services.Session;
using App.Services.Throwing;
using Infrastructure.Events;
using Infrastructure.Input;
using Infrastructure.Leveling;
using Infrastructure.Logging;
using Infrastructure.Settings;
using KnifeToss.Core.Abstractions.Collision;
using KnifeToss.Core.Abstractions.Config;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Gameplay.Target;
using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Abstractions.Input;
using KnifeToss.Core.Abstractions.Throwing;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Data.Level;
using KnifeToss.Core.Data.Target;
using KnifeToss.Core.Data.Throwing;
using KnifeToss.Core.Domain.Target;
using KnifeToss.Presentation.Gameplay.Collision;
using KnifeToss.Presentation.Gameplay.Knife;
using KnifeToss.Presentation.Gameplay.Target;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bootstrapper
{
    /// <summary>
    /// Main composition root that configures dependency injection for all game systems.
    /// This installer can reference all layers as it's specifically for DI configuration.
    /// </summary>
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private KnifeThrowConfigAsset knifeThrowConfig;
        [SerializeField] private KnifeSpawnRequestEventChannel knifeSpawnRequestEventChannel;
        [SerializeField] private CollisionEventChannel collisionEventChannel;
        [SerializeField] private KnifeImpactEventChannel knifeImpactEventChannel;
        [SerializeField] private LevelStateChangedEventChannel levelStateChangedEventChannel;
        [SerializeField] private KnifeThrowerView knifeThrowerView;
        [SerializeField] private TargetConfigAsset targetConfig;
        [SerializeField] private TargetView targetView;
        [SerializeField] private LevelRulesConfigAsset levelRulesConfig;

        /// <summary>
        /// Configures the dependency injection container with all services and components.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            GameSettingsAccess.Initialize(gameSettings);
            RegisterLevelSessionServices(builder);
            RegisterGameFlow(builder);
            RegisterKnifeServices(builder);
            RegisterTargetServices(builder);
            RegisterCollisionServices(builder);
        }

        private void RegisterGameFlow(IContainerBuilder builder)
        {
            builder.RegisterInstance(CreateLogger()).As<ILogger>();
            builder.RegisterEntryPoint<GameFlowController>(Lifetime.Singleton);
        }

        private void RegisterLevelSessionServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(levelRulesConfig).As<IConfigurationProvider<LevelRules>>();
            builder.RegisterInstance(knifeImpactEventChannel).As<IEventChannel<KnifeImpactEventData>>();
            builder.RegisterInstance(levelStateChangedEventChannel).As<IEventChannel<LevelStateChangedEventData>>();
            builder.Register<LevelSessionService>(Lifetime.Singleton).As<ILevelSessionService>();
        }

        private void RegisterKnifeServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(knifeThrowConfig).As<IConfigurationProvider<KnifeThrowConfiguration>>();
            builder.RegisterInstance(knifeSpawnRequestEventChannel).As<IEventChannel<KnifeSpawnRequestData>>();
            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.RegisterEntryPoint<ThrowService>(Lifetime.Singleton).As<IThrowService>();
            builder.RegisterComponent(knifeThrowerView).As<IKnifeThrowerView>();
        }

        private void RegisterTargetServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(targetConfig).As<IConfigurationProvider<TargetConfiguration>>();
            builder.RegisterComponent(targetView).As<ITargetView>().As<IHitable>();
            
            RegisterRotationBehavior(builder);
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

        private void RegisterRotationBehavior(IContainerBuilder builder)
        {
            TargetConfiguration config = targetConfig.GetConfiguration();
            IRotationBehavior rotationBehavior = new ConstantRotationBehavior(config.RotationSpeed);
            builder.RegisterInstance(rotationBehavior).As<IRotationBehavior>();
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
