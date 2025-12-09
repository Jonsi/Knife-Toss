using System;
using Core.Abstractions;
using Core.Data;
using Presentation.Presentation.Gameplay.Knife.States;
using UnityEngine;
using VContainer;

namespace Presentation.Presentation.Gameplay.Knife
{
    /// <summary>
    /// MonoBehaviour wrapper for the knife visual representation.
    /// Uses a state machine to manage knife behavior.
    /// Implements IHitable to participate in the collision system.
    /// </summary>
    public class KnifeView : MonoBehaviour, IHitable
    {
        private static readonly Type CachedType = typeof(KnifeView);

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private StickPositionData _stickPositionData;
        
        private IKnifeState _currentState;
        private readonly KnifeLoadedState _loadedState;
        private readonly KnifeInFlightState _inFlightState;
        private readonly KnifeStuckState _stuckState;
        private readonly KnifeBouncedState _bouncedState;
        private readonly KnifePooledState _pooledState;

        private IEventChannel<CollisionData> _collisionChannel;

        /// <summary>
        /// Gets the cached type for efficient collision routing.
        /// </summary>
        public Type HitableType => CachedType;

        /// <summary>
        /// Gets the current state of the knife.
        /// </summary>
        public IKnifeState CurrentState => _currentState;

        /// <summary>
        /// Gets whether the knife is currently in flight.
        /// </summary>
        public bool IsInFlight => _currentState == _inFlightState;

        /// <summary>
        /// Gets the stick position data for where the knife should stick.
        /// </summary>
        public StickPositionData StickPositionData => _stickPositionData;

        public KnifeView()
        {
            _loadedState = new KnifeLoadedState(this);
            _inFlightState = new KnifeInFlightState(this);
            _stuckState = new KnifeStuckState(this);
            _bouncedState = new KnifeBouncedState(this);
            _pooledState = new KnifePooledState(this);
        }

        [Inject]
        private void Construct(IEventChannel<CollisionData> collisionChannel)
        {
            _collisionChannel = collisionChannel;
        }

        private void Awake()
        {
            TransitionToState(_loadedState);
        }

        private void Update()
        {
            _currentState?.Update(Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsInFlight || _collisionChannel == null)
                return;

            var otherHitable = other.GetComponent<IHitable>();
            if (otherHitable == null)
                return;

            // Calculate impact point as the closest point on the other collider
            Vector2 impactPoint = other.ClosestPoint(transform.position);
            _collisionChannel.Raise(new CollisionData(this, otherHitable, impactPoint));
        }

        /// <summary>
        /// Launches the knife in the specified direction with the given speed.
        /// </summary>
        /// <param name="direction">The direction vector in which to launch the knife.</param>
        /// <param name="speed">The speed at which to launch the knife.</param>
        public void Launch(Vector2 direction, float speed)
        {
            TransitionToState(_inFlightState);
            _inFlightState.Launch(direction, speed);
        }

        /// <summary>
        /// Stops the knife's movement and transitions to the stuck state.
        /// The target will handle positioning the knife.
        /// </summary>
        public void StopMovement()
        {
            TransitionToState(_stuckState);
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
            TransitionToState(_bouncedState);
        }

        /// <summary>
        /// Resets the knife to its initial loaded state.
        /// </summary>
        public void ResetKnife()
        {
            TransitionToState(_loadedState);
        }

        /// <summary>
        /// Returns the knife to the pool.
        /// </summary>
        public void ReturnToPool()
        {
            TransitionToState(_pooledState);
        }

        private void TransitionToState(IKnifeState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }
    }
}
