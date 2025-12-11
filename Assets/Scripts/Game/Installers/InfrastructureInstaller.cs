using Game.Entities.Target.Factories;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Level.Spawning;
using UnityEngine;
using VContainer;

namespace Game.Installers
{
    /// <summary>
    /// Installer responsible for registering infrastructure services including catalogs, factories, and spawners.
    /// </summary>
    public class InfrastructureInstaller : IInstaller
    {
        [SerializeField] private StagesConfigAsset _StagesConfig;
        [SerializeField] private Transform _LevelRoot;

        /// <summary>
        /// Initializes a new instance of the InfrastructureInstaller.
        /// </summary>
        /// <param name="stagesConfig">The stages configuration asset.</param>
        /// <param name="levelRoot">The root transform for level spawning.</param>
        public InfrastructureInstaller(StagesConfigAsset stagesConfig, Transform levelRoot)
        {
            _StagesConfig = stagesConfig;
            _LevelRoot = levelRoot;
        }

        /// <summary>
        /// Installs infrastructure services into the container.
        /// </summary>
        /// <param name="builder">The container builder to register services with.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StagesConfig);
            builder.Register<StagesConfigCatalog>(Lifetime.Singleton).As<ILevelCatalog>().AsSelf();

            builder.Register<RotationBehaviorFactory>(Lifetime.Singleton);

            builder.Register<CollectibleSpawner>(Lifetime.Singleton);

            builder.Register<TargetFactory>(Lifetime.Singleton)
                .WithParameter(_StagesConfig.DefaultTargetPrefab);

            builder.Register<LevelSpawner>(Lifetime.Singleton)
                .WithParameter(_LevelRoot)
                .As<ILevelSpawner>();
        }
    }
}
