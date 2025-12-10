using System;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Flow;
using KnifeToss.Core.Abstractions.Leveling;
using KnifeToss.Core.Data.Level;
using KnifeToss.Core.Domain.Level;
using VContainer.Unity;

namespace App.Services.Flow
{
    /// <summary>
    /// Coordinates level progression based on state changes.
    /// </summary>
    public class LevelFlowCoordinator : IStartable, IDisposable
    {
        private readonly IEventChannel<LevelStateChangedEventData> _StateChangedChannel;
        private readonly IStageNavigator _Navigator;
        private readonly IStageProgressTracker _Tracker;
        private readonly ILevelSetupService _LevelSetupService;

        public LevelFlowCoordinator(
            IEventChannel<LevelStateChangedEventData> stateChangedChannel,
            IStageNavigator navigator,
            IStageProgressTracker tracker,
            ILevelSetupService levelSetupService)
        {
            _StateChangedChannel = stateChangedChannel;
            _Navigator = navigator;
            _Tracker = tracker;
            _LevelSetupService = levelSetupService;
        }

        public void Start()
        {
            _StateChangedChannel.Subscribe(OnLevelStateChanged);
        }

        public void Dispose()
        {
            _StateChangedChannel.Unsubscribe(OnLevelStateChanged);
        }

        private void OnLevelStateChanged(LevelStateChangedEventData data)
        {
            if (data.CurrentState == nameof(LevelCompletedState))
            {
                HandleLevelCompleted();
            }
        }

        private void HandleLevelCompleted()
        {
            _Navigator.AdvanceLevel();
            _LevelSetupService.LoadLevel(_Tracker.CurrentStageIndex, _Tracker.CurrentLevelIndex);
        }
    }
}
