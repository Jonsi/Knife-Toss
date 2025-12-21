using Game.App.Flow.Events;
using Game.Flow.Stage;

namespace Game.Flow.Game.States
{
    public class GamePlayingState : GameStateBase
    {
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly ReturnToMenuEventChannel _ReturnToMenuChannel;

        public GamePlayingState(
            GameStateMachine machine,
            StageCompletedEventChannel stageCompletedChannel,
            ReturnToMenuEventChannel returnToMenuChannel) : base(machine)
        {
            _StageCompletedChannel = stageCompletedChannel;
            _ReturnToMenuChannel = returnToMenuChannel;
        }

        public override void Enter()
        {
            _StageCompletedChannel.Subscribe(HandleStageCompleted);
            _ReturnToMenuChannel.Subscribe(HandleReturnToMenu);
            Machine.StageFlowController.LoadStage(Context.CurrentStageIndex);
        }

        public override void Exit()
        {
            _StageCompletedChannel.Unsubscribe(HandleStageCompleted);
            _ReturnToMenuChannel.Unsubscribe(HandleReturnToMenu);
        }

        private void HandleStageCompleted(StageData data)
        {
            if (Context.CurrentStageIndex + 1 < Machine.LevelCatalog.StageCount)
            {
                Context.CurrentStageIndex++;
                Machine.StageFlowController.LoadStage(Context.CurrentStageIndex);
            }
            else
            {
                Context.CurrentStageIndex = 0;
                Machine.StageFlowController.LoadStage(Context.CurrentStageIndex);
            }
        }

        private void HandleReturnToMenu(StageData data)
        {
            // Transition to Menu state (when implemented)
            // Machine.SetState(Machine.Menu);
        }
    }
}

