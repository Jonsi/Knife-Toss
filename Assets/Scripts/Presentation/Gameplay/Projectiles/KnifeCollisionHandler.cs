using UnityEngine;
using KnifeToss.Application;
using KnifeToss.Core.Events;
using KnifeToss.Core.Events.Gameplay;
using VContainer;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Legacy collision handler. This is deprecated - use Knife component with KnifeCollision component instead.
    /// </summary>
    [System.Obsolete("Use Knife component with KnifeCollision component instead")]
    public class KnifeCollisionHandler : MonoBehaviour
    {
        [SerializeField] private KnifeImpactEventChannel legacyChannel;
        private IGameEventBus _bus;
        private Knife _knife;

        private void Awake()
        {
            _knife = GetComponent<Knife>();
        }

        [VContainer.Inject]
        public void Construct(IGameEventBus bus)
        {
            _bus = bus;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Legacy handler - collisions are now handled by KnifeCollision component
            PublishStuck(transform.position);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Legacy handler - collisions are now handled by KnifeCollision component
            PublishStuck(collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
        }

        public void PublishStuck(Vector3 point)
        {
            _bus?.Publish(new KnifeStuckAtEvent(point));
            legacyChannel?.RaiseKnifeStuck(point);
        }
    }
}

