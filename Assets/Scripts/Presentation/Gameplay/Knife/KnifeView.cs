using Core.Abstractions;
using Core.Data;
using Presentation.Presentation.Gameplay.Knife.States;
using UnityEngine;
using VContainer;

namespace Presentation.Presentation.Gameplay.Knife
{
    /// <summary>
    /// MonoBehaviour wrapper for the knife visual representation.
    /// Uses a state machine to manage knife behavior without polling.
    /// </summary>
    public class KnifeView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private IKnifeState _currentState;
        private readonly KnifeLoadedState _loadedState;
        private readonly KnifeInFlightState _inFlightState;
        private readonly KnifeStuckState _stuckState;
        private readonly KnifeBouncedState _bouncedState;
        private readonly KnifePooledState _pooledState;

        private ITargetService _targetService;

        /// <summary>
        /// Gets the current state of the knife.
        /// </summary>
        public IKnifeState CurrentState => _currentState;

        public KnifeView()
        {
            _loadedState = new KnifeLoadedState(this);
            _inFlightState = new KnifeInFlightState(this);
            _stuckState = new KnifeStuckState(this);
            _bouncedState = new KnifeBouncedState(this);
            _pooledState = new KnifePooledState(this);
        }

        [Inject]
        private void Construct(ITargetService targetService)
        {
            _targetService = targetService;
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
            if (_currentState != _inFlightState)
                return;

            var collisionTarget = other.GetComponentInParent<ICollisionTarget>();
            if (collisionTarget != null)
            {
                HandleTargetCollision(collisionTarget);
            }
        }

        private void HandleTargetCollision(ICollisionTarget target)
        {
            if (_targetService == null)
                return;

            Vector2 knifePosition = transform.position;
            StickPositionData stickData = _targetService.GetStickPosition(knifePosition, target.Center, target.Radius);

            transform.SetParent(target.Transform);
            transform.localPosition = new Vector3(stickData.Position.x, stickData.Position.y, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, stickData.RotationAngle);

            Stick();
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
        /// Transitions the knife to the stuck state.
        /// </summary>
        public void Stick()
        {
            TransitionToState(_stuckState);
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
