using Common.Collision.Data;
using Common.Collision.Logic;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Views;
using Game.Entities.Knife.Views;
using UnityEngine;

namespace Game.CollisionHandlers
{
    /// <summary>
    /// Handles collisions between knives and collectibles.
    /// Triggers collection logic and effects.
    /// </summary>
    public class KnifeHitCollectibleHandler : CollisionHandlerBase<KnifeView, CollectibleView>
    {
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;

        /// <summary>
        /// Initializes a new instance of the KnifeHitCollectibleHandler.
        /// </summary>
        /// <param name="collectibleHitChannel">Event channel for reporting hits.</param>
        public KnifeHitCollectibleHandler(IEventChannel<CollectibleHitEventData> collectibleHitChannel)
        {
            _CollectibleHitChannel = collectibleHitChannel;
        }

        protected override void HandleCollision(CollisionData data)
        {
            if (data.Source is not KnifeView knife || data.Target is not CollectibleView collectible)
                return;

            if (!collectible.MarkAsHit())
                return;

            collectible.PlayHitFeedback();
            _CollectibleHitChannel.Raise(new CollectibleHitEventData(data.ImpactPoint, collectible.Value));

            Object.Destroy(collectible.gameObject);
        }
    }
}

