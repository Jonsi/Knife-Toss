using System;
using Cysharp.Threading.Tasks;
using KnifeToss.Application.Services;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using KnifeToss.Presentation.UI;
using VContainer.Unity;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Non-MonoBehaviour coordinator that wires level lifecycle, UI, and command events.
    /// </summary>
    public sealed class LevelFlowCoordinator : IStartable, IDisposable
    {
        private readonly LevelSessionService sessionService;
        private readonly LevelSession session;
        private readonly LevelHudPresenter hudPresenter;
        private readonly LevelCommandEventChannel commandChannel;

        public LevelFlowCoordinator(
            LevelSessionService sessionService,
            LevelSession session,
            LevelHudPresenter hudPresenter,
            LevelCommandEventChannel commandChannel)
        {
            this.sessionService = sessionService;
            this.session = session;
            this.hudPresenter = hudPresenter;
            this.commandChannel = commandChannel;
        }

        public void Start()
        {
            if (commandChannel != null)
            {
                commandChannel.NextRequested += HandleNextRequested;
                commandChannel.RetryRequested += HandleRetryRequested;
            }

            if (session != null)
            {
                session.SessionChanged += HandleSessionChanged;
                session.Completed += HandleLevelCompleted;
                session.Failed += HandleLevelFailed;
            }

            InitializeAsync().Forget();
        }

        public void Dispose()
        {
            if (commandChannel != null)
            {
                commandChannel.NextRequested -= HandleNextRequested;
                commandChannel.RetryRequested -= HandleRetryRequested;
            }

            if (session != null)
            {
                session.SessionChanged -= HandleSessionChanged;
                session.Completed -= HandleLevelCompleted;
                session.Failed -= HandleLevelFailed;
            }
        }

        private async UniTaskVoid InitializeAsync()
        {
            hudPresenter?.ResetState();
            await sessionService.StartInitialLevelAsync();
            RenderHud();
        }

        private void HandleNextRequested()
        {
            BeginTransition(sessionService.NextLevelAsync());
        }

        private void HandleRetryRequested()
        {
            BeginTransition(sessionService.RetryLevelAsync());
        }

        private void HandleSessionChanged(LevelSession _)
        {
            RenderHud();
        }

        private void HandleLevelCompleted(LevelSession _)
        {
            hudPresenter?.ShowLevelComplete(true);
            UnityEngine.Time.timeScale = 0f;
        }

        private void HandleLevelFailed(LevelSession _)
        {
            hudPresenter?.ShowFail(true);
            UnityEngine.Time.timeScale = 0f;
        }

        private void RenderHud()
        {
            hudPresenter?.Render(session);
        }

        private void BeginTransition(UniTask transitionTask)
        {
            hudPresenter?.ResetState();
            UnityEngine.Time.timeScale = 1f;
            CompleteTransitionAsync(transitionTask).Forget();
        }

        private async UniTaskVoid CompleteTransitionAsync(UniTask transitionTask)
        {
            await transitionTask;
            RenderHud();
        }
    }
}
