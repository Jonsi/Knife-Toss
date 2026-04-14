using System;
using Common.Patterns.State;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using VContainer.Unity;

namespace Game
{
    /// <summary>
    /// Top-level flow controller.
    /// Manages the GameStateMachine.
    /// </summary>
    public class GameFlowController : IStartable, IDisposable
    {
        private readonly IGameStateMachine _StateMachine;
        private readonly IAnalyticsService _AnalyticsService;

        /// <summary>
        /// Initializes a new instance of the GameFlowController.
        /// </summary>
        /// <param name="stateMachine">The game state machine.</param>
        public GameFlowController(
            IGameStateMachine stateMachine,
            IAnalyticsService analyticsService)
        {
            _StateMachine = stateMachine;
            _AnalyticsService = analyticsService;
        }

        public void Start()
        {
            StartGameFlowAsync().Forget();
        }

        private async UniTaskVoid StartGameFlowAsync()
        {
            await DeferredGameSceneVisibility.WaitForReveal();
            _AnalyticsService?.TrackEvent(new AnalyticsEvent("game_started"));
            _StateMachine.SetState(_StateMachine.Playing);
        }

        public void Dispose()
        {
            if (_StateMachine is StateMachine stateMachine)
            {
                stateMachine.Shutdown();
            }
        }
    }
}
