using Game.Level.States;
using VContainer.Unity;

namespace Game
{
    /// <summary>
    /// Top-level flow controller responsible for initializing the game.
    /// Delegates specific level logic to the StageStateMachine.
    /// </summary>
    public class GameFlowController : IStartable
    {
        private readonly StageStateMachine _StateMachine;

        public GameFlowController(StageStateMachine stateMachine)
        {
            _StateMachine = stateMachine;
        }

        public void Start()
        {
            _StateMachine.StartStage(0);
        }
    }
}
