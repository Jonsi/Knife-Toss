using Game.Entities.Target.Factories;
using Game.Entities.Target.Views;
using Game.Pooling;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs target-related factories.
    /// </summary>
    public class TargetInstaller : IInstaller
    {
        private readonly PrefabsConfiguration _PrefabsConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetInstaller"/> class.
        /// </summary>
        /// <param name="prefabsConfiguration">Configuration containing target prefab.</param>
        public TargetInstaller(PrefabsConfiguration prefabsConfiguration)
        {
            _PrefabsConfiguration = prefabsConfiguration;
        }

        /// <summary>
        /// Registers target feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.Register<TargetBehaviorFactory>(Lifetime.Singleton);
            builder.Register<TargetFactory>(Lifetime.Singleton)
                .WithParameter(_PrefabsConfiguration);
        }
    }
}

