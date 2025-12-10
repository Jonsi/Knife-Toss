using System;

namespace Common.Patterns.State
{
    /// <summary>
    /// Simple state machine that manages enter/exit/update of states.
    /// </summary>
    public abstract class StateMachine
    {
        /// <summary>
        /// Gets the current active state.
        /// </summary>
        public IState Current => CurrentState;

        /// <summary>
        /// Gets the current state's type name or an empty string if unset.
        /// </summary>
        public string CurrentStateName => CurrentState?.GetType().Name ?? string.Empty;

        protected IState CurrentState { get; private set; }

        /// <summary>
        /// Switches to the provided state, invoking exit/enter as needed.
        /// </summary>
        /// <param name="newState">Target state instance.</param>
        public void SetState(IState newState)
        {
            if (newState == null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            if (ReferenceEquals(CurrentState, newState))
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        /// <summary>
        /// Updates the current state with the elapsed time.
        /// </summary>
        /// <param name="deltaTime">Frame delta time.</param>
        public void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }
    }
}

