using Common.Config;
using Game.DI;
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
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs knife throw feature services and configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeInstaller", menuName = "Knife Toss/Installers/Knife")]
    public class KnifeInstallerSO : BaseInstallerSO
    {
        [Header("Configs")]
        [SerializeField] private KnifeThrowConfigAsset _Config;

        [Header("Prefabs / Pooling")]
        [SerializeField] private PrefabsConfiguration _PrefabsConfiguration;

        [Header("Event Channels")]
        [SerializeField] private KnifeImpactEventChannel _ImpactChannel;
        [SerializeField] private KnifeThrownEventChannel _ThrownChannel;

        /// <summary>
        /// Registers knife throw feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Config).As<IConfigurationProvider<KnifeThrowConfiguration>>();
            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.Register<ThrowService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<KnifePoolProvider>(Lifetime.Singleton)
                .WithParameter(_PrefabsConfiguration);
            builder.RegisterInstance(_ImpactChannel).As<IEventChannel<KnifeImpactEventData>>();
            builder.RegisterInstance(_ThrownChannel).As<IEventChannel>();
            if (_PrefabsConfiguration != null)
            {
                builder.RegisterInstance(_PrefabsConfiguration).AsSelf();
            }
        }
    }
}

