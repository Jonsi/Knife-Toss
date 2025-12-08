using Core.Abstractions;
using Presentation.Gameplay.Knife.States;
using UnityEngine;
using VContainer;

namespace Presentation.Gameplay.Knife
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

        private void Awake()
        {
            TransitionToState(_loadedState);
        }

        private void Update()
        {
            _currentState?.Update(Time.deltaTime);
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

