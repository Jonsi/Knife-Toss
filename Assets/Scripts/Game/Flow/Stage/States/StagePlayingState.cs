using Common.Events;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.Level.Data;

namespace Game.Flow.Stage.States
{
    public class StagePlayingState : StageStateBase
    {
        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly LevelFailedEventChannel _LevelFailedChannel;
        private readonly VoidEventChannel _RetryRequestChannel;
        private readonly VoidEventChannel _RestartRequestChannel;

        public StagePlayingState(
            StageStateMachine machine,
            LevelCompletedEventChannel levelCompletedChannel,
            LevelFailedEventChannel levelFailedChannel,
            VoidEventChannel retryRequestChannel,
            VoidEventChannel restartRequestChannel) : base(machine) 
        {
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
            _RetryRequestChannel = retryRequestChannel;
            _RestartRequestChannel = restartRequestChannel;
        }

        public override void Enter()
        {
            _LevelCompletedChannel.Subscribe(HandleLevelCompleted);
            _LevelFailedChannel.Subscribe(HandleLevelFailed);
            _RetryRequestChannel.Subscribe(OnRetryRequested);
            _RestartRequestChannel.Subscribe(OnRestartRequested);
        }

        public override void Exit()
        {
            _LevelCompletedChannel.Unsubscribe(HandleLevelCompleted);
            _LevelFailedChannel.Unsubscribe(HandleLevelFailed);
            _RetryRequestChannel.Unsubscribe(OnRetryRequested);
            _RestartRequestChannel.Unsubscribe(OnRestartRequested);
        }

        private void HandleLevelCompleted(LevelData data)
        {
            int levelsInStage = Machine.LevelCatalog.GetLevelCount(Context.StageIndex);
            
            if (Context.CurrentLevelIndex + 1 < levelsInStage)
            {
                // Advance to next level
                Context.CurrentLevelIndex++;
                LoadCurrentLevel();
            }
            else
            {
                // Stage completed
                Machine.SetState(Machine.Completed);
            }
        }

        private void HandleLevelFailed(LevelData data)
        {
            // Popup is handled by UIController listening to LevelFailedEventChannel
        }

        private void OnRetryRequested()
        {
            Machine.LevelFlowController.RetryLevel();
        }

        private void OnRestartRequested()
        {
            // Transition to Loading state to reset the stage
            Machine.SetState(Machine.Loading);
        }

        private void LoadCurrentLevel()
        {
            LevelConfig config = Machine.LevelCatalog.GetLevelConfig(Context.StageIndex, Context.CurrentLevelIndex);
            Machine.LevelFlowController.LoadLevel(config);
        }
    }
}
