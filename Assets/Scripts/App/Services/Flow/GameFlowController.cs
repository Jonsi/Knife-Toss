using App.Services.Leveling;
using VContainer.Unity;

namespace App.Services.Flow
{
    /// <summary>
    /// Top-level flow controller responsible for initializing a level session.
    /// Extend this to manage stages/levels as they are introduced.
    /// </summary>
    public class GameFlowController : IStartable
    {
        private readonly LevelSetupService _levelSetupService;

        public GameFlowController(LevelSetupService levelSetupService)
        {
            _levelSetupService = levelSetupService;
        }

        public void Start()
        {
            // Load the first level of the first stage.
            _levelSetupService.LoadLevel(0, 0);
        }
    }
}
