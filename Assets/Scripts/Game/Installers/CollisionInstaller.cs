using Common.Collision.Data;
using Common.Collision.Events;
using Common.Collision.Interfaces;
using Common.Collision.Logic;
using Common.Events;
using Game.CollisionHandlers;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs collision handling services.
    /// </summary>
    public class CollisionInstaller : IInstaller
    {
        private readonly CollisionEventChannel _CollisionChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollisionInstaller"/> class.
        /// </summary>
        /// <param name="collisionChannel">Collision event channel.</param>
        public CollisionInstaller(CollisionEventChannel collisionChannel)
        {
            _CollisionChannel = collisionChannel;
        }

        /// <summary>
        /// Registers collision handling services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_CollisionChannel).As<IEventChannel<CollisionData>>();
            builder.RegisterEntryPoint<CollisionHandlerHandlerRouter>().As<ICollisionHandlerRouter>();
            builder.Register<KnifeHitTargetHandler>(Lifetime.Singleton);
            builder.Register<KnifeHitKnifeHandler>(Lifetime.Singleton);
            builder.Register<KnifeHitCollectibleHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CollisionHandlerRegistrar>();
        }
    }
}

