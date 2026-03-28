using System.Collections.Generic;
using Common.Analytics;
using Common.Analytics.Interfaces;

namespace Game.Flow.Stage.States
{
    public class StageCompletedState : StageStateBase
    {
        private readonly IAnalyticsService _AnalyticsService;

        public StageCompletedState(
            StageStateMachine machine,
            IAnalyticsService analyticsService) : base(machine)
        {
            _AnalyticsService = analyticsService;
        }

        public override void Enter()
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "stage_completed",
                new Dictionary<string, object>
                {
                    ["stage_index"] = Context.StageIndex,
                    ["total_levels_in_stage"] = Machine.LevelCatalog.GetLevelCount(Context.StageIndex)
                }));
            Machine.StageCompletedChannel.Raise(Context);
        }
    }
}
