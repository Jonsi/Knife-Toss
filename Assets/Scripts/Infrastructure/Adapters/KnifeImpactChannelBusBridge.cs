using UnityEngine;
using KnifeToss.Application;
using KnifeToss.Core.Events;
using KnifeToss.Core.Events.Gameplay;
using VContainer;

namespace KnifeToss.Infrastructure.Adapters
{
    /// <summary>
    /// MonoBehaviour that listens to legacy KnifeImpactEventChannel and publishes messages to IGameEventBus.
    /// Attach this in the scene referencing the channel.
    /// </summary>
    public sealed class KnifeImpactChannelBusBridge : MonoBehaviour
    {
        [SerializeField] private KnifeImpactEventChannel channel;
        private IGameEventBus _bus;

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            _bus = bus;
        }

        private void OnEnable()
        {
            if (channel == null || _bus == null) return;
            channel.KnifeStuck += OnKnifeStuck;
            channel.KnifeStuckAt += OnKnifeStuckAt;
        }

        private void OnDisable()
        {
            if (channel == null) return;
            channel.KnifeStuck -= OnKnifeStuck;
            channel.KnifeStuckAt -= OnKnifeStuckAt;
        }

        private void OnKnifeStuck() => _bus.Publish(new KnifeStuckEvent());
        private void OnKnifeStuckAt(Vector3 pos) => _bus.Publish(new KnifeStuckAtEvent(pos));
    }
}

