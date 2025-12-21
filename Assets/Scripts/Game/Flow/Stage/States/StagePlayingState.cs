using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.App.Flow.Events;
using Game.Flow.Level;
using Game.KnifeThrower.Interfaces;
using Game.Level.Data;
using Game.Pooling;
using UnityEngine;

namespace Game.Flow.Stage.States
{
    public class StagePlayingState : StageStateBase
    {
        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly LevelFailedEventChannel _LevelFailedChannel;
        private readonly VoidEventChannel _RetryRequestChannel;
        private readonly VoidEventChannel _RestartRequestChannel;
        private readonly LevelStartedEventChannel _LevelStartedChannel;
        private readonly IKnifeThrowerView _KnifeThrowerView;
        private readonly KnifePoolProvider _KnifePoolProvider;

        public StagePlayingState(
            StageStateMachine machine,
            LevelCompletedEventChannel levelCompletedChannel,
            LevelFailedEventChannel levelFailedChannel,
            VoidEventChannel retryRequestChannel,
            VoidEventChannel restartRequestChannel,
            LevelStartedEventChannel levelStartedChannel,
            IKnifeThrowerView knifeThrowerView,
            KnifePoolProvider knifePoolProvider) : base(machine)
        {
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
            _RetryRequestChannel = retryRequestChannel;
            _RestartRequestChannel = restartRequestChannel;
            _LevelStartedChannel = levelStartedChannel;
            _KnifeThrowerView = knifeThrowerView;
            _KnifePoolProvider = knifePoolProvider;
        }

        public override void Enter()
        {
            _LevelCompletedChannel.Subscribe(HandleLevelCompleted);
            _LevelFailedChannel.Subscribe(HandleLevelFailed);
            _RetryRequestChannel.Subscribe(OnRetryRequested);
            _RestartRequestChannel.Subscribe(OnRestartRequested);

            StartCurrentLevel(reloadLevel: false);
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
                StartCurrentLevel(reloadLevel: true);
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
            StartCurrentLevel(reloadLevel: true);
        }

        private void OnRestartRequested()
        {
            // Transition to Loading state to reset the stage
            Machine.SetState(Machine.Loading);
        }

        private void StartCurrentLevel(bool reloadLevel)
        {
            LevelConfig config = Machine.LevelCatalog.GetLevelConfig(Context.StageIndex, Context.CurrentLevelIndex);
            if (config == null)
                return;

            var stageConfig = Machine.LevelCatalog.GetStageConfig(Context.StageIndex);
            if (reloadLevel)
            {
                Machine.LevelFlowController.LoadLevel(stageConfig, config);
            }

            LoadKnifeForLevel();
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

        private void LoadKnifeForLevel()
        {
            // Get a knife from the pool and load it into the thrower
            var knife = _KnifePoolProvider.Get();
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = Vector2.zero, // Default spawn position
                ThrowSpeed = 10f // Default throw speed, could be configured
            };
            _KnifeThrowerView.LoadKnife(knife, spawnData);
        }
    }
}
