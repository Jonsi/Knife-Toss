using System;
using Game.Persistence.Interfaces;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs persistence services for saving player progress and settings.
    /// </summary>
    public class PersistenceInstaller : IInstaller
    {
        public PersistenceInstaller()
        {
        }

        /// <summary>
        /// Registers persistence services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.Register<FilePlayerDataStore>(Lifetime.Singleton)
                .As<IPlayerDataStore>();

            builder.RegisterEntryPoint<PlayerDataService>()
                .AsSelf()
                .As<IInitializable>();
        }
    }
}
