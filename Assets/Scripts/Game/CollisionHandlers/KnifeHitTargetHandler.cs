using Common.Collision.Data;
using Common.Collision.Logic;
using Common.Economy.Interfaces;
using Common.Events;
using Game.Economy.Interfaces;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Views;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Views;
using UnityEngine;

namespace Game.CollisionHandlers
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Tells the knife to stop moving and passes impact point to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandlerBase<KnifeView, TargetView>
    {
        private const float CollectibleOverlapRadius = 0.08f;
        private static readonly ContactFilter2D CollectibleOverlapFilter = new() { useTriggers = true };
        private readonly Collider2D[] _CollectibleOverlapBuffer = new Collider2D[4];

        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly ICurrencyService _CurrencyService;
        private readonly ICurrencyValueProvider _ValueProvider;

        public KnifeHitTargetHandler(
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            ICurrencyService currencyService,
            ICurrencyValueProvider valueProvider)
        {
            _ImpactChannel = impactChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _CurrencyService = currencyService;
            _ValueProvider = valueProvider;
        }

        protected override void HandleCollision(CollisionData data)
        {
            if (data.Source is not KnifeView knife || data.Target is not TargetView target)
                return;

            if (!knife.IsInFlight)
                return;

            if (knife.IsSuperKnife)
            {
                knife.StopMovement();
                target.ForceDestroy();
                _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, data.ImpactPoint));
                knife.ReturnToPool();
                return;
            }

            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
            target.PlayHitFeedback();
            TryCollectEmbeddedCollectible(target, data.ImpactPoint);
            
            _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, data.ImpactPoint));
        }

        private void TryCollectEmbeddedCollectible(TargetView target, Vector2 impactPoint)
        {
            if (_CollectibleHitChannel == null)
                return;

            int hits = Physics2D.OverlapCircle(impactPoint, CollectibleOverlapRadius, CollectibleOverlapFilter, _CollectibleOverlapBuffer);
            for (int i = 0; i < hits; i++)
            {
                var collectible = _CollectibleOverlapBuffer[i].GetComponent<CollectibleView>();
                if (collectible == null)
                    continue;

                if (!collectible.transform.IsChildOf(target.Transform))
                    continue;

                ProcessCollectibleHit(collectible, impactPoint);
                break;
            }
        }

        private void ProcessCollectibleHit(CollectibleView collectible, Vector2 impactPoint)
        {
            if (!collectible.MarkAsHit())
                return;

            var collectibleCurrency = collectible.Config?.CollectibleCurrency;
            if (collectibleCurrency == null || string.IsNullOrEmpty(collectibleCurrency.TypeId) || !_ValueProvider.HasType(collectibleCurrency.TypeId))
                return;

            int value = _ValueProvider.GetValue(collectibleCurrency.TypeId);
            _CurrencyService.AddBalance(collectibleCurrency.TypeId, value);

            collectible.PlayHitFeedback();
            _CollectibleHitChannel.Raise(new CollectibleHitEventData(impactPoint, value));
            Object.Destroy(collectible.gameObject);
        }
    }
}
