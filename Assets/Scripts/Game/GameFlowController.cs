using Common.Events;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using Game.Flow.Stage;
using Game.Level.Interfaces;
using VContainer.Unity;

namespace Game
{
    /// <summary>
    /// Top-level flow controller.
    /// Manages the GameStateMachine and coordinates stages.
    /// Bridges UI events to Flow commands.
    /// </summary>
    public class GameFlowController : IStartable, System.IDisposable
    {
        private readonly GameStateMachine _StateMachine;
        private readonly IStageFlowController _StageFlowController;
        private readonly StageCompletedEventChannel _StageCompletedChannel;
        private readonly ReturnToMenuEventChannel _ReturnToMenuChannel;
        
        // UI Inputs
        private readonly VoidEventChannel _RetryLevelChannel;
        private readonly VoidEventChannel _RestartStageChannel;
        
        // Flow Outputs
        private readonly VoidEventChannel _StageRetryRequestChannel;
        private readonly VoidEventChannel _StageRestartRequestChannel;
        
        private readonly ILevelCatalog _LevelCatalog;

        private int _CurrentStageIndex = 0;

        public GameFlowController(
            GameStateMachine stateMachine,
            IStageFlowController stageFlowController,
            StageCompletedEventChannel stageCompletedChannel,
            ReturnToMenuEventChannel returnToMenuChannel,
            VoidEventChannel retryLevelChannel,
            VoidEventChannel restartStageChannel,
            VoidEventChannel stageRetryRequestChannel,
            VoidEventChannel stageRestartRequestChannel,
            ILevelCatalog levelCatalog)
        {
            _StateMachine = stateMachine;
            _StageFlowController = stageFlowController;
            _StageCompletedChannel = stageCompletedChannel;
            _ReturnToMenuChannel = returnToMenuChannel;
            _RetryLevelChannel = retryLevelChannel;
            _RestartStageChannel = restartStageChannel;
            _StageRetryRequestChannel = stageRetryRequestChannel;
            _StageRestartRequestChannel = stageRestartRequestChannel;
            _LevelCatalog = levelCatalog;
        }

        public void Start()
        {
            _StageCompletedChannel.Subscribe(HandleStageCompleted);
            _ReturnToMenuChannel.Subscribe(HandleReturnToMenu);
            _RetryLevelChannel.Subscribe(HandleRetryLevelUI);
            _RestartStageChannel.Subscribe(HandleRestartStageUI);
            
            // Start the game loop
            _StateMachine.SetState(_StateMachine.Playing);
            
            // Start first stage
            _CurrentStageIndex = 0;
            _StageFlowController.LoadStage(_CurrentStageIndex);
        }

        public void Dispose()
        {
            _StageCompletedChannel.Unsubscribe(HandleStageCompleted);
            _ReturnToMenuChannel.Unsubscribe(HandleReturnToMenu);
            _RetryLevelChannel.Unsubscribe(HandleRetryLevelUI);
            _RestartStageChannel.Unsubscribe(HandleRestartStageUI);
        }

        private void HandleRetryLevelUI()
        {
            // UI Button -> Flow Command
            _StageRetryRequestChannel.Raise();
        }

        private void HandleRestartStageUI()
        {
            // UI Button -> Flow Command
            _StageRestartRequestChannel.Raise();
        }

        private void HandleReturnToMenu(StageData data)
        {
            // Future implementation
        }

        private void HandleStageCompleted(StageData data)
        {
            if (_CurrentStageIndex + 1 < _LevelCatalog.StageCount)
            {
                _CurrentStageIndex++;
                _StageFlowController.LoadStage(_CurrentStageIndex);
            }
            else
            {
                // All stages completed. Loop back or show credits?
                _CurrentStageIndex = 0;
                _StageFlowController.LoadStage(_CurrentStageIndex);
            }
        }
    }
}
