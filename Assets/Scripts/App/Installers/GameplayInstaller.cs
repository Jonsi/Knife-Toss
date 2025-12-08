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

namespace App.Installers
{
    /// <summary>
    /// VContainer LifetimeScope that configures dependency injection for gameplay systems.
    /// Registers all gameplay services and their dependencies.
    /// </summary>
    public class GameplayInstaller : LifetimeScope
    {
        [SerializeField] private KnifeThrowConfigAsset knifeThrowConfig;
        [SerializeField] private KnifeSpawnRequestEventChannel knifeSpawnRequestEventChannel;

        /// <summary>
        /// Configures the dependency injection container with all gameplay services.
        /// </summary>
        /// <param name="builder">The container builder used to register dependencies.</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(knifeThrowConfig).As<IKnifeThrowConfigurationProvider>();
            builder.RegisterInstance(knifeSpawnRequestEventChannel).As<IEventChannel<KnifeSpawnRequestData>>();

            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.Register<ThrowService>(Lifetime.Singleton).As<IThrowService>();
            
            builder.RegisterComponentInHierarchy<KnifeThrowerView>();
        }
    }
}

