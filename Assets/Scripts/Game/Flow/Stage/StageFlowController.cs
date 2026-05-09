using System;
using System.Collections.Generic;
using System.Threading;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using Game.Flow.Stage.States;
using Game.Level.Services;
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
        private readonly WorldAssetService _WorldAssetService;

        /// <summary>
        /// Creates the controller with the stage state machine and analytics.
        /// </summary>
        public StageFlowController(
            StageStateMachine stateMachine,
            IAnalyticsService analyticsService,
            GameData gameData,
            WorldAssetService worldAssetService)
        {
            _StateMachine = stateMachine;
            _AnalyticsService = analyticsService;
            _GameData = gameData;
            _WorldAssetService = worldAssetService;
        }

        /// <inheritdoc />
        public async UniTask LoadStage(int collectionIndex, int stageIndex, CancellationToken cancellationToken = default)
        {
            _GameData.CurrentCollectionIndex = collectionIndex;
            _GameData.CurrentStageIndex = stageIndex;
            _StateMachine.Context.CollectionIndex = collectionIndex;
            _StateMachine.Context.StageIndex = stageIndex;

            bool collectionReady = await _WorldAssetService.LoadCollectionAsync(collectionIndex, cancellationToken);
            if (!collectionReady)
            {
                return;
            }

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
