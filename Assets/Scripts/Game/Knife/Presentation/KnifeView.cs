using System;
using KnifeToss.Common.Events;
using KnifeToss.Game.Collision.Data;
using KnifeToss.Game.Collision.Interfaces;
using KnifeToss.Game.Knife.Interfaces;
using KnifeToss.Game.Knife.States;
using KnifeToss.Game.Target.Data;
using UnityEngine;
using VContainer;

namespace KnifeToss.Game.Knife.Presentation
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
            _StateMachine.inFlight.Launch(direction, speed);
            _StateMachine.SetState(_StateMachine.inFlight);
        }

        /// <summary>
        /// Stops the knife's movement and transitions to the stuck state.
        /// The target will handle positioning the knife.
        /// </summary>
        public void StopMovement()
        {
            _StateMachine.SetState(_StateMachine.stuck);
        }

        /// <summary>
        /// Transitions the knife to the bounced state.
        /// </summary>
        public void Bounce()
        {
            _StateMachine.bounced.ConfigureBounce(new Vector2(0f, 6f), 360f);
            _StateMachine.SetState(_StateMachine.bounced);
        }

        /// <summary>
        /// Resets the knife to its initial loaded state.
        /// </summary>
        public void ResetKnife()
        {
            _StateMachine.SetState(_StateMachine.loaded);
        }

        /// <summary>
        /// Returns the knife to the pool.
        /// </summary>
        public void ReturnToPool()
        {
            _StateMachine.SetState(_StateMachine.pooled);
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