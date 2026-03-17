using System;
using Common.Events;
using Game.Entities.Collectible.Data;
using VContainer.Unity;

namespace Game.UI.HUD
{
    /// <summary>
    /// Bridges collectible hit events to the HUD coin accumulation view.
    /// </summary>
    public sealed class CoinAccumulationController : IStartable, IDisposable
    {
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly CoinsUiDisplay _CoinsUiDisplay;

        public CoinAccumulationController(
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            CoinsUiDisplay coinsUiDisplay)
        {
            _CollectibleHitChannel = collectibleHitChannel;
            _CoinsUiDisplay = coinsUiDisplay;
        }

        public void Start()
        {
            _CollectibleHitChannel?.Subscribe(OnCollectibleHit);
        }

        public void Dispose()
        {
            _CollectibleHitChannel?.Unsubscribe(OnCollectibleHit);
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            _CoinsUiDisplay?.PlayAccumulationFromWorld(data.Position, data.Value);
        }
    }
}
