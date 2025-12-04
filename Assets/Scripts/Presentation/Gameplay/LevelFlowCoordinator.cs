using System;
using Cysharp.Threading.Tasks;
using KnifeToss.Application.Services;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using KnifeToss.Presentation.UI;
using UnityEngine;
using VContainer.Unity;

namespace KnifeToss.Presentation.Gameplay
{



    public sealed class LevelFlowCoordinator : IStartable, IDisposable
    {
        private readonly LevelSessionService sessionService;
        private readonly LevelSession session;
        private readonly LevelHudPresenter hudPresenter;
        private readonly LevelCommandEventChannel commandChannel;
        private readonly IStarRatingService _starRatingService;
        private readonly ILevelProgressStore _levelProgressStore;

        /// <summary>
        /// Performs the LevelFlowCoordinator operation.
        /// </summary>
        public LevelFlowCoordinator(
            LevelSessionService sessionService,
            LevelSession session,
            LevelHudPresenter hudPresenter,
            LevelCommandEventChannel commandChannel,
            IStarRatingService starRatingService,
            ILevelProgressStore levelProgressStore)
        {
            this.sessionService = sessionService;
            this.session = session;
            this.hudPresenter = hudPresenter;
            this.commandChannel = commandChannel;
            _starRatingService = starRatingService;
            _levelProgressStore = levelProgressStore;
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
            var stars = _starRatingService.CalculateStars(completedSession);
            
            var progress = _levelProgressStore.LoadProgress();
            if (!progress.starsPerLevel.ContainsKey(completedSession.LevelIndex) || progress.starsPerLevel[completedSession.LevelIndex] < stars)
            {
                progress.starsPerLevel[completedSession.LevelIndex] = stars;
                _levelProgressStore.SaveProgress(progress);
            }
            
            hudPresenter.levelCompletePopup.SetStars(stars);
            hudPresenter.levelCompletePopup.SetScoreText(stars);
            hudPresenter.ShowLevelComplete(true);
        }

        /// <summary>
        /// Performs the HandleLevelFailed operation.
        /// </summary>
        private void HandleLevelFailed(LevelSession _)
        {
            // Reuse the level-complete popup for failure flow as well.
            if (hudPresenter?.levelCompletePopup != null)
            {
                hudPresenter.levelCompletePopup.SetStars(0);
                hudPresenter.levelCompletePopup.SetScoreText(0);
                hudPresenter.ShowLevelComplete(true);
            }

            if (hudPresenter?.retryButton != null)
            {
                hudPresenter.retryButton.gameObject.SetActive(true);
                hudPresenter.retryButton.interactable = true;
            }
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

            hudPresenter.Render(LevelHudViewModel.FromSession(session));
        }

        /// <summary>
        /// Performs the BeginTransition operation.
        /// </summary>
        private void BeginTransition(UniTask transitionTask)
        {
            hudPresenter?.ResetState();
            if (hudPresenter?.retryButton != null)
            {
                hudPresenter.retryButton.interactable = false;
            }
            CompleteTransitionAsync(transitionTask).Forget();
        }

        /// <summary>
        /// Performs the CompleteTransitionAsync operation.
        /// </summary>
        private async UniTaskVoid CompleteTransitionAsync(UniTask transitionTask)
        {
            await transitionTask;
            if (hudPresenter?.retryButton != null)
            {
                hudPresenter.retryButton.interactable = true;
            }
            RenderHud();
        }
    }
}
