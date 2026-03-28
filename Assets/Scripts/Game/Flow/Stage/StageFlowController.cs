using System.Collections.Generic;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Game.Flow.Interfaces;
using Game.Flow.Stage.States;
using VContainer.Unity;

namespace Game.Flow.Stage
{
    public class StageFlowController : IStageFlowController
    {
        private readonly StageStateMachine _StateMachine;
        private readonly IAnalyticsService _AnalyticsService;

        public StageFlowController(
            StageStateMachine stateMachine,
            IAnalyticsService analyticsService)
        {
            _StateMachine = stateMachine;
            _AnalyticsService = analyticsService;
        }

        public void LoadStage(int stageIndex)
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "stage_started",
                new Dictionary<string, object>
                {
                    ["stage_index"] = stageIndex,
                    ["total_levels_in_stage"] = _StateMachine.LevelCatalog.GetLevelCount(stageIndex)
                }));
            _StateMachine.Context.StageIndex = stageIndex;
            _StateMachine.SetState(_StateMachine.Loading);
        }
    }
}
