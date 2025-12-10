using System;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Gameplay;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Data.Level;
using KnifeToss.Core.Data.Throwing;
using KnifeToss.Core.Domain.Level;
using VContainer.Unity;

namespace KnifeToss.Core.Domain
{
    /// <summary>
    /// State machine that manages the entire game flow within a stage.
    /// Owns the StageData and all states.
    /// </summary>
    public class StageStateMachine : StateMachine, ITickable
    {
        public StageData Context { get; }

        public readonly LevelLoadingState Loading;
        public readonly LevelPlayingState Playing;
        public readonly LevelCompletedState Completed;
        public readonly LevelFailedState Failed;

        private readonly IEventChannel<LevelStateChangedEventData> _stateChangedChannel;

        public StageStateMachine(
            StageData context,
            IEventChannel<LevelStateChangedEventData> stateChangedChannel,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<KnifeSpawnRequestData> spawnRequestChannel)
        {
            _stateChangedChannel = stateChangedChannel;
            Context = context;

            Loading = new LevelLoadingState(this);
            Playing = new LevelPlayingState(this, impactChannel, spawnRequestChannel);
            Completed = new LevelCompletedState(this);
            Failed = new LevelFailedState(this);
        }

        public void StartStage(int stageIndex)
        {
            Context.CurrentStageIndex = stageIndex;
            Context.CurrentLevelIndex = 0;
            
            SetState(Loading);
        }

        public new void SetState(KnifeToss.Core.Abstractions.State.IState newState)
        {
            string previousStateName = CurrentStateName;
            base.SetState(newState);
            string currentStateName = CurrentStateName;

            if (previousStateName != currentStateName)
            {
                _stateChangedChannel.Raise(new LevelStateChangedEventData(previousStateName, currentStateName));
            }
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }
    }
}
