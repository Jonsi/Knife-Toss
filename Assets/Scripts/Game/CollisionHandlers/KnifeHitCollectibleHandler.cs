using Common.Collision.Data;
using Common.Collision.Logic;
using Common.Economy.Data;
using Common.Economy.Interfaces;
using Common.Events;
using Game.Economy.Interfaces;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Views;
using Game.Entities.Knife.Views;
using UnityEngine;

namespace Game.CollisionHandlers
{
    /// <summary>
    /// Handles collisions between knives and collectibles.
    /// Adds currency via ICurrencyService (using collectible's CollectibleCurrencyConfig) and raises collectible hit events.
    /// </summary>
    public class KnifeHitCollectibleHandler : CollisionHandlerBase<KnifeView, CollectibleView>
    {
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly ICurrencyService _CurrencyService;
        private readonly ICurrencyValueProvider _ValueProvider;

        /// <summary>
        /// Initializes a new instance of the KnifeHitCollectibleHandler.
        /// </summary>
        /// <param name="collectibleHitChannel">Event channel for reporting hits.</param>
        /// <param name="currencyService">Service for adding collected currency.</param>
        /// <param name="valueProvider">Provider for resolving currency values.</param>
        public KnifeHitCollectibleHandler(
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            ICurrencyService currencyService,
            ICurrencyValueProvider valueProvider)
        {
            _CollectibleHitChannel = collectibleHitChannel;
            _CurrencyService = currencyService;
            _ValueProvider = valueProvider;
        }

        protected override void HandleCollision(CollisionData data)
        {
            if (data.Source is not KnifeView knife || data.Target is not CollectibleView collectible)
                return;

            if (!knife.IsInFlight)
                return;

            if (!collectible.MarkAsHit())
                return;

            CurrencyTypeConfig currencyType = collectible.Config?.CollectibleCurrency?.CurrencyType;
            if (currencyType == null || !_ValueProvider.HasType(currencyType.TypeId))
                return;

            int value = _ValueProvider.GetValue(currencyType.TypeId);
            _CurrencyService.AddBalance(currencyType.TypeId, value);

            collectible.PlayHitFeedback();
            _CollectibleHitChannel.Raise(new CollectibleHitEventData(data.ImpactPoint, value));

            Object.Destroy(collectible.gameObject);
        }
    }
}

