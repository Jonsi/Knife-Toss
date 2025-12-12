using Common.Events;
using Common.Patterns.State;
using Game.Flow.Events;
using Game.Flow.Interfaces;
using Game.KnifeThrower.Interfaces;
using Game.Level.Interfaces;
using Game.Pooling;
using VContainer.Unity;

namespace Game.Flow.Stage.States
{
    public class StageStateMachine : StateMachine, ITickable
    {
        public StageData Context { get; }

        public readonly StageLoadingState Loading;
        public readonly StagePlayingState Playing;
        public readonly StageCompletedState Completed;

        public readonly ILevelFlowController LevelFlowController;
        public readonly ILevelCatalog LevelCatalog;
        public readonly StageCompletedEventChannel StageCompletedChannel;

        public StageStateMachine(
            StageData context,
            ILevelFlowController levelFlowController,
            ILevelCatalog levelCatalog,
            LevelCompletedEventChannel levelCompletedChannel,
            LevelFailedEventChannel levelFailedChannel,
            StageCompletedEventChannel stageCompletedChannel,
            VoidEventChannel retryRequestChannel,
            VoidEventChannel restartRequestChannel,
            LevelStartedEventChannel levelStartedChannel,
            IKnifeThrowerView knifeThrowerView,
            KnifePoolProvider knifePoolProvider)
        {
            Context = context;
            LevelFlowController = levelFlowController;
            LevelCatalog = levelCatalog;
            StageCompletedChannel = stageCompletedChannel;

            Loading = new StageLoadingState(this);
            Playing = new StagePlayingState(
                this,
                levelCompletedChannel,
                levelFailedChannel,
                retryRequestChannel,
                restartRequestChannel,
                levelStartedChannel,
                knifeThrowerView,
                knifePoolProvider);
            Completed = new StageCompletedState(this);
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }
    }
}
