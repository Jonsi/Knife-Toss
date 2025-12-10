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
        private readonly LevelSetupService _LevelSetupService;

        public GameFlowController(LevelSetupService levelSetupService)
        {
            _LevelSetupService = levelSetupService;
        }

        public void Start()
        {
            // Load the first level of the first stage.
            _LevelSetupService.LoadLevel(0, 0);
        }
    }
}
