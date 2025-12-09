using System;
using KnifeToss.Core.Abstractions.Collision;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Gameplay.Knife;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Data.Target;
using KnifeToss.Core.Domain.Knife;
using KnifeToss.Core.Domain.Knife.States;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay.Knife
{
    /// <summary>
    /// MonoBehaviour wrapper for the knife visual representation.
    /// Uses a state machine to manage knife behavior.
    /// Implements IHitable to participate in the collision system.
    /// </summary>
    public class KnifeView : MonoBehaviour, IKnifeViewContext
    {
        private static readonly Type CachedType = typeof(KnifeView);

        [SerializeField] private SpriteRenderer _SpriteRenderer;
        [SerializeField] private StickPositionData _StickPositionData;
        [SerializeField] private Collider2D _Collider;
        
        private KnifeStateMachine _StateMachine;

        private IEventChannel<CollisionData> _CollisionChannel;

        /// <summary>
        /// Gets the cached type for efficient collision routing.
        /// </summary>
        public Type HitableType => CachedType;

        /// <summary>
        /// Gets whether the knife is currently in flight.
        /// </summary>
        public bool IsInFlight => _StateMachine?.IsInFlight ?? false;

        /// <summary>
        /// Gets the stick position data for where the knife should stick.
        /// </summary>
        public StickPositionData StickPositionData => _StickPositionData;

        /// <summary>
        /// Exposes transform for state consumption.
        /// </summary>
        public Transform Transform => transform;

        [Inject]
        private void Construct(IEventChannel<CollisionData> collisionChannel)
        {
            _CollisionChannel = collisionChannel;
        }

        private void Awake()
        {
            InitializeStateMachine();
        }

        private void Update()
        {
            _StateMachine?.Update(Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collisionChannel = _CollisionChannel;
            if (!IsInFlight || collisionChannel == null)
                return;
            
            var hitable = other.GetComponent<IHitable>();
            if (hitable == null)
                return;

            // Calculate impact point as the closest point on the other collider
            Vector2 impactPoint = other.ClosestPoint(transform.position);
            collisionChannel.Raise(new CollisionData(this, hitable, impactPoint));
        }

        /// <summary>
        /// Launches the knife in the specified direction with the given speed.
        /// </summary>
        /// <param name="direction">The direction vector in which to launch the knife.</param>
        /// <param name="speed">The speed at which to launch the knife.</param>
        public void Launch(Vector2 direction, float speed)
        {
            _StateMachine.InFlight.Launch(direction, speed);
            _StateMachine.SetState(_StateMachine.InFlight);
        }

        /// <summary>
        /// Stops the knife's movement and transitions to the stuck state.
        /// The target will handle positioning the knife.
        /// </summary>
        public void StopMovement()
        {
            _StateMachine.SetState(_StateMachine.Stuck);
        }

        /// <summary>
        /// Transitions the knife to the bounced state.
        /// </summary>
        public void Bounce()
        {
            _StateMachine.Bounced.ConfigureBounce(new Vector2(0f, 6f), 360f);
            _StateMachine.SetState(_StateMachine.Bounced);
        }

        /// <summary>
        /// Resets the knife to its initial loaded state.
        /// </summary>
        public void ResetKnife()
        {
            _StateMachine.SetState(_StateMachine.Loaded);
        }

        /// <summary>
        /// Returns the knife to the pool.
        /// </summary>
        public void ReturnToPool()
        {
            _StateMachine.SetState(_StateMachine.Pooled);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        private void InitializeStateMachine()
        {
            _StateMachine = new KnifeStateMachine(
                new KnifeLoadedState(this),
                new KnifeInFlightState(this),
                new KnifeStuckState(this),
                new KnifeBouncedState(this),
                new KnifePooledState(this));
        }
        
        public void SetColliderEnabled(bool enabled)
        {
            if (_Collider != null)
                _Collider.enabled = enabled;
        }
    }
}