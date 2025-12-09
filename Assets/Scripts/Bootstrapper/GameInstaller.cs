using App.Services;
using Core.Abstractions;
using Core.Data;
using Core.Domain;
using Infrastructure.Infrastructure.Events;
using Infrastructure.Infrastructure.Input;
using Infrastructure.Infrastructure.Leveling;
using Infrastructure.Input;
using Presentation.Presentation.Gameplay.Collision;
using Presentation.Presentation.Gameplay.Knife;
using Presentation.Presentation.Gameplay.Target;
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
        [SerializeField] private KnifeThrowConfigAsset knifeThrowConfig;
        [SerializeField] private KnifeSpawnRequestEventChannel knifeSpawnRequestEventChannel;
        [SerializeField] private CollisionEventChannel collisionEventChannel;
        [SerializeField] private KnifeThrowerView knifeThrowerView;
        [SerializeField] private TargetConfigAsset targetConfig;
        [SerializeField] private TargetView targetView;

        /// <summary>
        /// Configures the dependency injection container with all services and components.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterKnifeServices(builder);
            RegisterTargetServices(builder);
            RegisterCollisionServices(builder);
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
            builder.Register<TargetGeometry>(Lifetime.Singleton).As<ITargetGeometry>();
            builder.Register<TargetService>(Lifetime.Singleton).As<ITargetService>();
            builder.RegisterComponent(targetView).As<ITargetView>().As<IHitable>();
            
            RegisterRotationBehavior(builder);
        }

        private void RegisterCollisionServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(collisionEventChannel).As<IEventChannel<CollisionData>>();
            builder.Register<CollisionRouter>(Lifetime.Singleton).As<ICollisionRouter>();
            builder.Register<KnifeHitTargetHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CollisionProxy>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CollisionHandlerRegistrar>(Lifetime.Singleton);
        }

        private void RegisterRotationBehavior(IContainerBuilder builder)
        {
            TargetConfiguration config = targetConfig.GetConfiguration();
            IRotationBehavior rotationBehavior = new ConstantRotationBehavior(config.RotationSpeed);
            builder.RegisterInstance(rotationBehavior).As<IRotationBehavior>();
        }
    }
}
