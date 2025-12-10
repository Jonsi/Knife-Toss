using Game.Flow.Interfaces;
using VContainer.Unity;

namespace Game
{
    /// <summary>
    /// Top-level flow controller.
    /// Manages the GameStateMachine.
    /// </summary>
    public class GameFlowController : IStartable
    {
        private readonly IGameStateMachine _StateMachine;

        /// <summary>
        /// Initializes a new instance of the GameFlowController.
        /// </summary>
        /// <param name="stateMachine">The game state machine.</param>
        public GameFlowController(IGameStateMachine stateMachine)
        {
            _StateMachine = stateMachine;
        }

        public void Start()
        {
            _StateMachine.SetState(_StateMachine.Playing);
        }
    }
}
