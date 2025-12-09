using KnifeToss.Core.Abstractions.Gameplay.Session;
using VContainer.Unity;

namespace App.Services.Flow
{
    /// <summary>
    /// Top-level flow controller responsible for initializing a level session.
    /// Extend this to manage stages/levels as they are introduced.
    /// </summary>
    public class GameFlowController : IStartable
    {
        private readonly ILevelSessionService _levelSessionService;

        public GameFlowController(ILevelSessionService levelSessionService)
        {
            _levelSessionService = levelSessionService;
        }

        public void Start()
        {
            // Kick off the initial session. Future: orchestrate stage/level selection.
            _levelSessionService.StartSession();
        }
    }
}

