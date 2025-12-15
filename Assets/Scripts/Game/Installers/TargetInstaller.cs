using Game.Entities.Target.Factories;
using Game.Entities.Target.Views;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs target-related factories.
    /// </summary>
    public class TargetInstaller : IInstaller
    {
        private readonly TargetView _DefaultTargetPrefab;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetInstaller"/> class.
        /// </summary>
        /// <param name="defaultTargetPrefab">Prefab used as the default target.</param>
        public TargetInstaller(TargetView defaultTargetPrefab)
        {
            _DefaultTargetPrefab = defaultTargetPrefab;
        }

        /// <summary>
        /// Registers target feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.Register<TargetBehaviorFactory>(Lifetime.Singleton);
            builder.Register<TargetFactory>(Lifetime.Singleton)
                .WithParameter(_DefaultTargetPrefab);
        }
    }
}

