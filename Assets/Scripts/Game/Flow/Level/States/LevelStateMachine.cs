using System.Collections.Generic;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Common.Events;
using Common.Patterns.State;
using Game.Entities.Collectible.Data;
using Game.Entities.Knife.Data;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Stage;
using Game.KnifeThrower.Interfaces;
using VContainer.Unity;

namespace Game.Flow.Level.States
{
    public class LevelStateMachine : StateMachine, ITickable
    {
        public LevelData Context { get; }

        public readonly LevelLoadingState Loading;
        public readonly LevelPlayingState Playing;
        public readonly LevelCompletedState Completed;
        public readonly LevelFailedState Failed;

        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly LevelFailedEventChannel _LevelFailedChannel;
        private readonly VoidEventChannel _LevelFailFlowStartedChannel;
        private readonly StageData _StageData;
        private readonly IAnalyticsService _AnalyticsService;

        /// <summary>
        /// Initializes a new instance of the LevelStateMachine.
        /// </summary>
        public LevelStateMachine(
            LevelData context,
            LevelCompletedEventChannel levelCompletedChannel,
            LevelFailedEventChannel levelFailedChannel,
            VoidEventChannel levelFailFlowStartedChannel,
            StageData stageData,
            IAnalyticsService analyticsService,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            IEventChannel knifeThrownChannel,
            IKnifeThrowerView knifeThrowerView)
        {
            Context = context;
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
            _LevelFailFlowStartedChannel = levelFailFlowStartedChannel;
            _StageData = stageData;
            _AnalyticsService = analyticsService;

            Loading = new LevelLoadingState(this);
            Playing = new LevelPlayingState(this, impactChannel, collectibleHitChannel, knifeThrownChannel, knifeThrowerView);
            Completed = new LevelCompletedState(this);
            Failed = new LevelFailedState(this);
        }

        public void NotifyLevelCompleted()
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "level_completed",
                CreateLevelParameters(_StageData.StageIndex, _StageData.CurrentLevelIndex, Context)));
            _LevelCompletedChannel.Raise(Context);
        }

        public void NotifyLevelFailed()
        {
            _AnalyticsService?.TrackEvent(new AnalyticsEvent(
                "level_failed",
                CreateLevelParameters(_StageData.StageIndex, _StageData.CurrentLevelIndex, Context)));
            _LevelFailedChannel.Raise(Context);
        }

        public void NotifyLevelFailFlowStarted()
        {
            _LevelFailFlowStartedChannel?.Raise();
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }

        private static Dictionary<string, object> CreateLevelParameters(int stageIndex, int levelIndex, LevelData levelData)
        {
            var parameters = new Dictionary<string, object>
            {
                ["stage_index"] = stageIndex,
                ["level_index"] = levelIndex
            };

            int? totalLevels = levelData?.StageConfig?.Levels?.Count;
            if (totalLevels.HasValue)
            {
                parameters["total_levels_in_stage"] = totalLevels.Value;
            }

            if (levelData?.Config != null)
            {
                parameters["total_knives"] = levelData.Config.KnifeCount;
                parameters["target_count"] = levelData.Config.Targets != null ? levelData.Config.Targets.Count : 0;
            }

            if (levelData != null)
            {
                parameters["knives_thrown"] = levelData.KnivesThrown;
                parameters["knives_stuck"] = levelData.KnivesStuck;
                parameters["knives_in_flight"] = levelData.KnivesInFlight;
                parameters["collected_count"] = levelData.CollectedCount;
                parameters["has_bounced"] = levelData.HasBounced;
            }

            return parameters;
        }
    }
}
