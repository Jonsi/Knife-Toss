using System;
using System.Collections.Generic;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using Game.Flow.Stage.States;
using VContainer.Unity;

namespace Game.Flow.Stage
{
    /// <summary>
    /// Loads stages into the stage state machine and tracks session indices.
    /// </summary>
    public class StageFlowController : IStageFlowController, IDisposable
    {
        private readonly StageStateMachine _StateMachine;
        private readonly IAnalyticsService _AnalyticsService;
        private readonly GameData _GameData;

        /// <summary>
        /// Creates the controller with the stage state machine and analytics.
        /// </summary>
        public StageFlowController(
            StageStateMachine stateMachine,
            IAnalyticsService analyticsService,
            GameData gameData)
        {
            _StateMachine = stateMachine;
            _AnalyticsService = analyticsService;
            _GameData = gameData;
        }

        /// <inheritdoc />
        public void LoadStage(int collectionIndex, int stageIndex)
        {
            _GameData.CurrentCollectionIndex = collectionIndex;
            _GameData.CurrentStageIndex = stageIndex;
            _StateMachine.Context.CollectionIndex = collectionIndex;
            _StateMachine.Context.StageIndex = stageIndex;

            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "stage_started",
                new Dictionary<string, object>
                {
                    ["collection_index"] = collectionIndex,
                    ["stage_index"] = stageIndex,
                    ["total_levels_in_stage"] = _StateMachine.LevelCatalog.GetLevelCount(stageIndex)
                }));
            _StateMachine.SetState(_StateMachine.Loading);
        }

        /// <summary>
        /// Releases the stage state machine.
        /// </summary>
        public void Dispose()
        {
            _StateMachine?.Shutdown();
        }
    }
}
