using System;
using KnifeToss.Core.Abstractions.Collision;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.State;
using KnifeToss.Core.Abstractions.Gameplay.Knife;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Data.Target;
using KnifeToss.Core.Domain.Knife;
using KnifeToss.Core.Domain.Knife.States;
using UnityEngine;
using VContainer;

namespace Presentation.Gameplay.Knife
{
    /// <summary>
    /// MonoBehaviour wrapper for the knife visual representation.
    /// Uses a state machine to manage knife behavior.
    /// Implements IHitable to participate in the collision system.
    /// </summary>
    public class KnifeView : MonoBehaviour, IKnifeViewContext
    {
        private static readonly Type CachedType = typeof(KnifeView);

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private StickPositionData _stickPositionData;
        [SerializeField] private Collider2D _collider;
        
        private KnifeStateMachine _stateMachine;

        private IEventChannel<CollisionData> _collisionChannel;

        /// <summary>
        /// Gets the cached type for efficient collision routing.
        /// </summary>
        public Type HitableType => CachedType;

        /// <summary>
        /// Gets the current state of the knife.
        /// </summary>
        public IState CurrentState => _stateMachine?.Current;

        /// <summary>
        /// Gets whether the knife is currently in flight.
        /// </summary>
        public bool IsInFlight => _stateMachine?.IsInFlight ?? false;

        /// <summary>
        /// Gets the stick position data for where the knife should stick.
        /// </summary>
        public StickPositionData StickPositionData => _stickPositionData;

        /// <summary>
        /// Exposes transform for state consumption.
        /// </summary>
        public Transform Transform => transform;

        [Inject]
        private void Construct(IEventChannel<CollisionData> collisionChannel)
        {
            _collisionChannel = collisionChannel;
        }

        private void Awake()
        {
            InitializeStateMachine();
        }

        private void Update()
        {
            _stateMachine?.Update(Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collisionChannel = _collisionChannel;
            if (!IsInFlight || collisionChannel == null)
                return;

            if (other is not IHitable hitable)
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
            _stateMachine.InFlight.Launch(direction, speed);
            _stateMachine.SetState(_stateMachine.InFlight);
        }

        /// <summary>
        /// Stops the knife's movement and transitions to the stuck state.
        /// The target will handle positioning the knife.
        /// </summary>
        public void StopMovement()
        {
            _stateMachine.SetState(_stateMachine.Stuck);
        }

        /// <summary>
        /// Sets the stick position data for where the knife should stick.
        /// </summary>
        /// <param name="stickPositionData">The position and rotation data relative to target center.</param>
        public void SetStickPositionData(StickPositionData stickPositionData)
        {
            _stickPositionData = stickPositionData;
        }

        /// <summary>
        /// Transitions the knife to the bounced state.
        /// </summary>
        public void Bounce()
        {
            _stateMachine.Bounced.ConfigureBounce(new Vector2(0f, 6f), 360f);
            _stateMachine.SetState(_stateMachine.Bounced);
        }

        /// <summary>
        /// Resets the knife to its initial loaded state.
        /// </summary>
        public void ResetKnife()
        {
            _stateMachine.SetState(_stateMachine.Loaded);
        }

        /// <summary>
        /// Returns the knife to the pool.
        /// </summary>
        public void ReturnToPool()
        {
            _stateMachine.SetState(_stateMachine.Pooled);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new KnifeStateMachine(
                new KnifeLoadedState(this),
                new KnifeInFlightState(this),
                new KnifeStuckState(this),
                new KnifeBouncedState(this),
                new KnifePooledState(this));
        }
        
        public void SetColliderEnabled(bool enabled)
        {
            if (_collider != null)
                _collider.enabled = enabled;
        }

    }
}