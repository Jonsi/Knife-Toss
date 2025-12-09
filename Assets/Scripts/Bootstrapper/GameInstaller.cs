using App.Services;
using Core.Abstractions;
using Core.Data;
using Infrastructure.Events;
using Infrastructure.Input;
using Infrastructure.Leveling;
using Presentation.Gameplay.Knife;
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

        /// <summary>
        /// Configures the dependency injection container with all services and components.
        /// </summary>
        /// <param name="builder">The container builder used to register dependencies.</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(knifeThrowConfig).As<IKnifeThrowConfigurationProvider>();
            builder.RegisterInstance(knifeSpawnRequestEventChannel).As<IEventChannel<KnifeSpawnRequestData>>();
            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.RegisterEntryPoint<ThrowService>(Lifetime.Singleton).As<IThrowService>();
            builder.RegisterComponent(knifeThrowerView).As<IKnifeThrowerView>();
        }
    }
}

