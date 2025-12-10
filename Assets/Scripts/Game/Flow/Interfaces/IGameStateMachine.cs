using Common.Patterns.State;

namespace Game.Flow.Interfaces
{
    /// <summary>
    /// Interface for the game state machine that manages high-level game states.
    /// </summary>
    public interface IGameStateMachine
    {
        /// <summary>
        /// Gets the playing state instance.
        /// </summary>
        IState Playing { get; }

        /// <summary>
        /// Switches to the provided state.
        /// </summary>
        /// <param name="newState">Target state instance.</param>
        void SetState(IState newState);
    }
}

