using Game.DI;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Events;
using Game.Entities.Collectible.Spawning;
using Game.PowerUps.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs collectible feature services.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectibleInstaller", menuName = "Knife Toss/Installers/Collectible")]
    public class CollectibleInstallerSO : BaseInstallerSO
    {
        [Header("Event Channels")]
        [SerializeField] private CollectibleHitEventChannel _HitChannel;

        /// <summary>
        /// Registers collectible feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<CollectibleRegistry>(Lifetime.Singleton);
            builder.Register<CollectibleSpawner>(Lifetime.Singleton);
            builder.RegisterInstance(_HitChannel).As<IEventChannel<CollectibleHitEventData>>();
        }
    }
}

