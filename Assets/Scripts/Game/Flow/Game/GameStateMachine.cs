using Common.Patterns.State;
using Game.Flow.Events;
using Game.Flow.Game.States;
using Game.Flow.Interfaces;
using Game.Level.Interfaces;
using VContainer.Unity;

namespace Game.Flow.Game
{
    public class GameStateMachine : StateMachine, ITickable
    {
        public GameData Context { get; }

        public readonly GamePlayingState Playing;
        
        public readonly IStageFlowController StageFlowController;
        public readonly ILevelCatalog LevelCatalog;

        public GameStateMachine(
            GameData context,
            IStageFlowController stageFlowController,
            ILevelCatalog levelCatalog,
            StageCompletedEventChannel stageCompletedChannel,
            ReturnToMenuEventChannel returnToMenuChannel)
        {
            Context = context;
            StageFlowController = stageFlowController;
            LevelCatalog = levelCatalog;

            Playing = new GamePlayingState(this, stageCompletedChannel, returnToMenuChannel);
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }
    }
}
