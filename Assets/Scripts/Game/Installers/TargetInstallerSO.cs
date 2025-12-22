using Game.DI;
using Game.Entities.Target.Factories;
using Game.Pooling;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs target-related factories.
    /// </summary>
    [CreateAssetMenu(fileName = "TargetInstaller", menuName = "Knife Toss/Installers/Target")]
    public class TargetInstallerSO : BaseInstallerSO
    {
        [Header("Prefabs / Pooling")]
        [SerializeField] private PrefabsConfiguration _PrefabsConfiguration;

        /// <summary>
        /// Registers target feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<TargetBehaviorFactory>(Lifetime.Singleton);
            builder.Register<TargetFactory>(Lifetime.Singleton)
                .WithParameter(_PrefabsConfiguration);
        }
    }
}

