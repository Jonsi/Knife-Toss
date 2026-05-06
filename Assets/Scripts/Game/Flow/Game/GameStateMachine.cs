using Common.Patterns.State;
using Game.Flow.Events;
using Game.Flow.Game.States;
using Game.Flow.Interfaces;
using Game.Level.Interfaces;
using VContainer.Unity;

namespace Game.Flow.Game
{
    /// <summary>
    /// State machine that manages high-level game states such as playing, paused, etc.
    /// </summary>
    public class GameStateMachine : StateMachine, IGameStateMachine, ITickable
    {
        /// <summary>
        /// Gets the shared game data context.
        /// </summary>
        public GameData Context { get; }

        /// <summary>
        /// Ordered worlds manifest for progression after a collection is completed.
        /// </summary>
        public IStagesCollectionCatalog StagesCollectionCatalog { get; }

        private readonly GamePlayingState _Playing;

        /// <summary>
        /// Gets the playing state instance.
        /// </summary>
        public IState Playing => _Playing;
        
        public readonly IStageFlowController StageFlowController;
        public readonly ILevelCatalog LevelCatalog;

        /// <summary>
        /// Creates the game state machine with flow dependencies.
        /// </summary>
        public GameStateMachine(
            GameData context,
            IStageFlowController stageFlowController,
            ILevelCatalog levelCatalog,
            IStagesCollectionCatalog stagesCollectionCatalog,
            StageCompletedEventChannel stageCompletedChannel,
            ReturnToMenuEventChannel returnToMenuChannel)
        {
            Context = context;
            StageFlowController = stageFlowController;
            LevelCatalog = levelCatalog;
            StagesCollectionCatalog = stagesCollectionCatalog;

            _Playing = new GamePlayingState(this, stageCompletedChannel, returnToMenuChannel);
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }
    }
}
