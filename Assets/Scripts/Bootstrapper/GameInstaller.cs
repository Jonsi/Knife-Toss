using App.Services;
using Core.Abstractions;
using Core.Data;
using Core.Domain;
using Infrastructure.Events;
using Infrastructure.Input;
using Infrastructure.Leveling;
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
        [SerializeField] private KnifeThrowerView knifeThrowerView;
        [SerializeField] private TargetConfigAsset targetConfig;
        [SerializeField] private TargetView targetView;

        /// <summary>
        /// Configures the dependency injection container with all services and components.
        /// </summary>
        /// <param name="builder">The container builder used to register dependencies.</param>
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterKnifeServices(builder);
            RegisterTargetServices(builder);
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
            builder.RegisterComponent(targetView).As<ITargetView>().As<ICollisionTarget>();
            
            RegisterRotationBehavior(builder);
        }

        private void RegisterRotationBehavior(IContainerBuilder builder)
        {
            TargetConfiguration config = targetConfig.GetConfiguration();
            IRotationBehavior rotationBehavior = new ConstantRotationBehavior(config.RotationSpeed);
            builder.RegisterInstance(rotationBehavior).As<IRotationBehavior>();
        }
    }
}
