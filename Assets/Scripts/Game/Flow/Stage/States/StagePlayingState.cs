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
        private readonly LevelStartedEventChannel _LevelStartedChannel;

        public StagePlayingState(
            StageStateMachine machine,
            LevelCompletedEventChannel levelCompletedChannel,
            LevelFailedEventChannel levelFailedChannel,
            VoidEventChannel retryRequestChannel,
            VoidEventChannel restartRequestChannel,
            LevelStartedEventChannel levelStartedChannel) : base(machine) 
        {
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
            _RetryRequestChannel = retryRequestChannel;
            _RestartRequestChannel = restartRequestChannel;
            _LevelStartedChannel = levelStartedChannel;
        }

        public override void Enter()
        {
            _LevelCompletedChannel.Subscribe(HandleLevelCompleted);
            _LevelFailedChannel.Subscribe(HandleLevelFailed);
            _RetryRequestChannel.Subscribe(OnRetryRequested);
            _RestartRequestChannel.Subscribe(OnRestartRequested);
            
            var config = Machine.LevelCatalog.GetLevelConfig(Context.StageIndex, Context.CurrentLevelIndex);
            RaiseLevelStarted(config);
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
            RaiseLevelStarted(config);
        }

        private void RaiseLevelStarted(LevelConfig config)
        {
            if (_LevelStartedChannel == null || config == null)
                return;

            var data = new LevelStartedEventData
            {
                StageIndex = Context.StageIndex,
                LevelIndex = Context.CurrentLevelIndex,
                TotalLevelsInStage = Machine.LevelCatalog.GetLevelCount(Context.StageIndex),
                TotalKnives = config.KnifeCount
            };
            _LevelStartedChannel.Raise(data);
        }
    }
}
