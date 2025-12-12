using Common.Config;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Events;
using Game.Entities.Knife.Views;
using Game.Input;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.KnifeThrower.Events;
using Game.KnifeThrower.Services;
using Game.Pooling;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs knife throw feature services and configuration.
    /// </summary>
    public class KnifeInstaller : IInstaller
    {
        private readonly KnifeThrowConfigAsset _Config;
        private readonly KnifeThrowerView _View;
        private readonly KnifeImpactEventChannel _ImpactChannel;
        private readonly KnifeThrownEventChannel _ThrownChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnifeInstaller"/> class.
        /// </summary>
        /// <param name="config">Knife throw configuration asset.</param>
        /// <param name="view">Knife thrower view component.</param>
        /// <param name="impactChannel">Knife impact event channel.</param>
        /// <param name="thrownChannel">Knife thrown event channel.</param>
        public KnifeInstaller(
            KnifeThrowConfigAsset config,
            KnifeThrowerView view,
            KnifeImpactEventChannel impactChannel,
            KnifeThrownEventChannel thrownChannel)
        {
            _Config = config;
            _View = view;
            _ImpactChannel = impactChannel;
            _ThrownChannel = thrownChannel;
        }

        /// <summary>
        /// Registers knife throw feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Config).As<IConfigurationProvider<KnifeThrowConfiguration>>();
            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.RegisterEntryPoint<ThrowService>().As<IThrowService>();
            builder.RegisterComponent(_View).As<IKnifeThrowerView>();
            builder.RegisterInstance(_View.KnifePrefab);
            builder.Register<KnifePoolProvider>(Lifetime.Singleton);
            builder.RegisterInstance(_ImpactChannel).As<IEventChannel<KnifeImpactEventData>>();
            builder.RegisterInstance(_ThrownChannel).As<IEventChannel>();
        }
    }
}

