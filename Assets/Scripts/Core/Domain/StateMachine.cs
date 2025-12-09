using System;
using KnifeToss.Core.Abstractions.State;

namespace KnifeToss.Core.Domain
{
    /// <summary>
    /// Simple state machine that manages enter/exit/update of states.
    /// </summary>
    public abstract class StateMachine
    {
        protected IState CurrentState { get; private set; }
        public IState Current => CurrentState;
        public string CurrentStateName => CurrentState?.GetType().Name ?? string.Empty;

        public void SetState(IState newState)
        {
            if (newState == null)
                throw new ArgumentNullException(nameof(newState));

            if (ReferenceEquals(CurrentState, newState))
                return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }
    }
}

