using Common.Events;
using Common.Patterns.State;
using Game.Knife.Data;
using Game.Level.Data;
using VContainer.Unity;

namespace Game.Level.States
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

        private readonly IEventChannel<LevelStateChangedEventData> _StateChangedChannel;

        public StageStateMachine(
            StageData context,
            IEventChannel<LevelStateChangedEventData> stateChangedChannel,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<KnifeSpawnRequestData> spawnRequestChannel)
        {
            _StateChangedChannel = stateChangedChannel;
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

        public new void SetState(IState newState)
        {
            string previousStateName = CurrentStateName;
            base.SetState(newState);
            string currentStateName = CurrentStateName;

            if (previousStateName != currentStateName)
            {
                _StateChangedChannel.Raise(new LevelStateChangedEventData(previousStateName, currentStateName));
            }
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }
    }
}
