using Common.Collision.Data;
using Common.Collision.Events;
using Common.Collision.Interfaces;
using Common.Collision.Logic;
using Game.DI;
using Common.Events;
using Game.CollisionHandlers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs collision handling services.
    /// </summary>
    [CreateAssetMenu(fileName = "CollisionInstaller", menuName = "Knife Toss/Installers/Collision")]
    public class CollisionInstallerSO : BaseInstallerSO
    {
        [Header("Event Channels")]
        [SerializeField] private CollisionEventChannel _CollisionChannel;

        /// <summary>
        /// Registers collision handling services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_CollisionChannel).As<IEventChannel<CollisionData>>();
            builder.Register<CollisionHandlerHandlerRouter>(Lifetime.Singleton)
                .As<ICollisionHandlerRouter>()
                .AsSelf();
            builder.Register<KnifeHitTargetHandler>(Lifetime.Singleton);
            builder.Register<KnifeHitKnifeHandler>(Lifetime.Singleton);
            builder.Register<KnifeHitCollectibleHandler>(Lifetime.Singleton);
            builder.Register<CollisionHandlerRegistrar>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}

