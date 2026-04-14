using Game.Flow.Interfaces;
using Game.Flow.Level.States;
using Game.Level.Configurations.Stage;
using Game.Level.Data;
using System;
using VContainer.Unity;

namespace Game.Flow.Level
{
    /// <summary>
    /// Manages the flow of a single level.
    /// Controls the LevelStateMachine and exposes commands via ILevelFlowController.
    /// </summary>
    public class LevelFlowController : ILevelFlowController, IDisposable
    {
        private readonly LevelStateMachine _StateMachine;

        public LevelFlowController(LevelStateMachine stateMachine)
        {
            _StateMachine = stateMachine;
        }

        public void LoadLevel(StageConfig stageConfig, LevelConfig config)
        {
            _StateMachine.Context.StageConfig = stageConfig;
            _StateMachine.Context.Config = config;
            _StateMachine.SetState(_StateMachine.Loading);
        }

        public void RetryLevel()
        {
            if (_StateMachine.Context.Config != null)
            {
                _StateMachine.SetState(_StateMachine.Loading);
            }
        }

        public void Dispose()
        {
            _StateMachine?.Shutdown();
        }
    }
}
