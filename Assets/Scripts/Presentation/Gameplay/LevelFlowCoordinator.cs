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
            hudPresenter.ShowLevelComplete(true);
        }

        private void HandleLevelFailed(LevelSession _)
        {
            Time.timeScale = 0f;
            if (hudPresenter?.retryButton != null)
            {
                hudPresenter.retryButton.gameObject.SetActive(true);
                hudPresenter.retryButton.interactable = true;
            }
            hudPresenter?.PlayRetryAnimation();
        }

        private void RenderHud()
        {
            if (hudPresenter == null || session == null)
            {
                return;
            }

            hudPresenter.Render(LevelHudViewModel.FromSession(session));
        }

        private void BeginTransition(UniTask transitionTask)
        {
            Time.timeScale = 1f;
            hudPresenter?.ResetState();
            if (hudPresenter?.retryButton != null)
            {
                hudPresenter.retryButton.interactable = false;
            }
            CompleteTransitionAsync(transitionTask).Forget();
        }

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
