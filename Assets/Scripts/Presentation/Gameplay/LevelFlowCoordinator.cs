using System;
using App.Services;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Cysharp.Threading.Tasks;
using Presentation.UI;
using Presentation.UI.ViewModels;
using UnityEngine;
using VContainer.Unity;

namespace Presentation.Gameplay
{



    public sealed class LevelFlowCoordinator : IStartable, IDisposable
    {
        private readonly LevelSessionService sessionService;
        private readonly LevelSession session;
        private readonly LevelHudPresenter hudPresenter;
        private readonly LevelCommandEventChannel commandChannel;
        private readonly IPlayerDataService playerDataService;

        /// <summary>
        /// Performs the LevelFlowCoordinator operation.
        /// </summary>
        public LevelFlowCoordinator(
            LevelSessionService sessionService,
            LevelSession session,
            LevelHudPresenter hudPresenter,
            LevelCommandEventChannel commandChannel,
            IPlayerDataService playerDataService)
        {
            this.sessionService = sessionService;
            this.session = session;
            this.hudPresenter = hudPresenter;
            this.commandChannel = commandChannel;
            this.playerDataService = playerDataService;
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        public void Start()
        {
            if (commandChannel != null)
            {
                commandChannel.NextRequested += HandleNextRequested;
                commandChannel.RetryRequested += HandleRetryRequested;
                commandChannel.LevelRequested += HandleLevelRequested;
                commandChannel.ResetToStageStartRequested += HandleResetToStageStartRequested;
            }

            if (session != null)
            {
                session.SessionChanged += HandleSessionChanged;
                session.Completed += HandleLevelCompleted;
                session.Failed += HandleLevelFailed;
            }

            InitializeAsync().Forget();
        }

        /// <summary>
        /// Performs the Dispose operation.
        /// </summary>
        public void Dispose()
        {
            if (commandChannel != null)
            {
                commandChannel.NextRequested -= HandleNextRequested;
                commandChannel.RetryRequested -= HandleRetryRequested;
                commandChannel.LevelRequested -= HandleLevelRequested;
                commandChannel.ResetToStageStartRequested -= HandleResetToStageStartRequested;
            }

            if (session != null)
            {
                session.SessionChanged -= HandleSessionChanged;
                session.Completed -= HandleLevelCompleted;
                session.Failed -= HandleLevelFailed;
            }
        }

        /// <summary>
        /// Performs the InitializeAsync operation.
        /// </summary>
        private async UniTaskVoid InitializeAsync()
        {
            Time.timeScale = 1f;
            hudPresenter?.ResetState();
            await sessionService.StartInitialLevelAsync();
            RenderHud();
        }

        /// <summary>
        /// Performs the HandleNextRequested operation.
        /// </summary>
        private void HandleNextRequested()
        {
            BeginTransition(sessionService.NextLevelAsync());
        }

        /// <summary>
        /// Performs the HandleRetryRequested operation.
        /// </summary>
        private void HandleRetryRequested()
        {
            BeginTransition(sessionService.RetryLevelAsync());
        }

        /// <summary>
        /// Performs the HandleLevelRequested operation.
        /// </summary>
        /// <param name="levelIndex">1-based index of the level to load.</param>
        private void HandleLevelRequested(int levelIndex)
        {
            int clamped = Math.Max(1, levelIndex);
            BeginTransition(sessionService.StartLevelAsync(session.StageIndex, clamped));
        }

        /// <summary>
        /// Performs the HandleResetToStageStartRequested operation.
        /// </summary>
        private void HandleResetToStageStartRequested()
        {
            BeginTransition(sessionService.ResetToStageStartAsync());
        }

        /// <summary>
        /// Performs the HandleSessionChanged operation.
        /// </summary>
        private void HandleSessionChanged(LevelSession _)
        {
            RenderHud();
        }

        /// <summary>
        /// Performs the HandleLevelCompleted operation.
        /// </summary>
        private void HandleLevelCompleted(LevelSession completedSession)
        {
            BeginTransition(sessionService.NextLevelAsync());
        }

        /// <summary>
        /// Performs the HandleLevelFailed operation.
        /// </summary>
        private void HandleLevelFailed(LevelSession _)
        {
            Time.timeScale = 0f;
            hudPresenter?.ShowFail(true);
        }

        /// <summary>
        /// Performs the RenderHud operation.
        /// </summary>
        private void RenderHud()
        {
            if (hudPresenter == null || session == null)
            {
                return;
            }

            hudPresenter.Render(LevelHudViewModel.FromSession(session, playerDataService));
        }

        /// <summary>
        /// Performs the BeginTransition operation.
        /// </summary>
        private void BeginTransition(UniTask transitionTask)
        {
            Time.timeScale = 1f;
            hudPresenter?.ResetState();
            CompleteTransitionAsync(transitionTask).Forget();
        }

        /// <summary>
        /// Performs the CompleteTransitionAsync operation.
        /// </summary>
        private async UniTaskVoid CompleteTransitionAsync(UniTask transitionTask)
        {
            await transitionTask;
            RenderHud();
        }
    }
}
