using Game.Flow.Events;
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
            Machine.StageFlowController.LoadStage(Context.CurrentCollectionIndex, Context.CurrentStageIndex);
        }

        public override void Exit()
        {
            _StageCompletedChannel.Unsubscribe(HandleStageCompleted);
            _ReturnToMenuChannel.Unsubscribe(HandleReturnToMenu);
        }

        private void HandleStageCompleted(StageData data)
        {
            int stageCount = Machine.LevelCatalog.StageCount;
            int collectionsCount = Machine.StagesCollectionComposite?.CollectionsCount ?? 0;

            if (Context.CurrentStageIndex + 1 < stageCount)
            {
                Context.CurrentStageIndex++;
            }
            else if (Context.CurrentCollectionIndex + 1 < collectionsCount)
            {
                Context.CurrentCollectionIndex++;
                Context.CurrentStageIndex = 0;
            }
            else
            {
                Context.CurrentCollectionIndex = 0;
                Context.CurrentStageIndex = 0;
            }

            Machine.StageFlowController.LoadStage(Context.CurrentCollectionIndex, Context.CurrentStageIndex);
        }

        private void HandleReturnToMenu(StageData data)
        {
        }
    }
}

