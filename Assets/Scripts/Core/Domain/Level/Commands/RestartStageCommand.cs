using KnifeToss.Core.Abstractions.Commands;
using KnifeToss.Core.Abstractions.Flow;
using KnifeToss.Core.Abstractions.Leveling;

namespace KnifeToss.Core.Domain.Level.Commands
{
    public class RestartStageCommand : ICommand
    {
        private readonly IStageNavigator _navigator;
        private readonly IStageProgressTracker _tracker;
        private readonly ILevelSetupService _levelSetup;

        public RestartStageCommand(IStageNavigator navigator, IStageProgressTracker tracker, ILevelSetupService levelSetup)
        {
            _navigator = navigator;
            _tracker = tracker;
            _levelSetup = levelSetup;
        }

        public void Execute()
        {
            _navigator.RestartStage();
            _levelSetup.LoadLevel(_tracker.CurrentStageIndex, _tracker.CurrentLevelIndex);
        }
    }
}
