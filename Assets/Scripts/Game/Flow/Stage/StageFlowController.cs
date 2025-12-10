using Game.Flow.Interfaces;
using Game.Flow.Stage.States;
using VContainer.Unity;

namespace Game.Flow.Stage
{
    public class StageFlowController : IStageFlowController
    {
        private readonly StageStateMachine _StateMachine;

        public StageFlowController(StageStateMachine stateMachine)
        {
            _StateMachine = stateMachine;
        }

        public void LoadStage(int stageIndex)
        {
            _StateMachine.Context.StageIndex = stageIndex;
            _StateMachine.SetState(_StateMachine.Loading);
        }
    }
}
