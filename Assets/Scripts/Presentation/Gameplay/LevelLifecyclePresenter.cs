using KnifeToss.Application.Services;
using KnifeToss.Core.Domain;
using KnifeToss.Installers;
using KnifeToss.Presentation.UI;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Listens to level session events and coordinates UI plus life-cycle transitions.
    /// </summary>
    public class LevelLifecyclePresenter : MonoBehaviour
    {
        [SerializeField] private GameplayInstaller installer;
        [SerializeField] private LevelHudPresenter hudPresenter;
        [SerializeField] private GameStatePresenter statePresenter;

        private LevelSessionService sessionService;
        private LevelSession session;

        private void Awake()
        {
            if (installer == null)
            {
                installer = FindFirstObjectByType<GameplayInstaller>();
            }
        }

        private void OnEnable()
        {
            Attach();
        }

        private void OnDisable()
        {
            Detach();
        }

        private void Start()
        {
            statePresenter?.ResetState();
            sessionService?.StartInitialLevel();
            hudPresenter?.Render(session);
        }

        public void RequestNextLevel()
        {
            ResetPresentationState();
            sessionService?.NextLevel();
        }

        public void RequestRetryLevel()
        {
            ResetPresentationState();
            sessionService?.RetryLevel();
        }

        private void Attach()
        {
            if (installer == null)
                return;

            sessionService = installer.SessionService;
            session = installer.Session;

            if (session != null)
            {
                session.SessionChanged += HandleSessionChanged;
                session.Completed += HandleLevelCompleted;
                session.Failed += HandleLevelFailed;
            }
        }

        private void Detach()
        {
            if (session != null)
            {
                session.SessionChanged -= HandleSessionChanged;
                session.Completed -= HandleLevelCompleted;
                session.Failed -= HandleLevelFailed;
            }
        }

        private void HandleSessionChanged(LevelSession levelSession)
        {
            hudPresenter?.Render(levelSession);
        }

        private void HandleLevelCompleted(LevelSession levelSession)
        {
            statePresenter?.ShowLevelComplete(true);
            Time.timeScale = 0f;
        }

        private void HandleLevelFailed(LevelSession levelSession)
        {
            statePresenter?.ShowFail(true);
            Time.timeScale = 0f;
        }

        private void ResetPresentationState()
        {
            statePresenter?.ResetState();
            Time.timeScale = 1f;
        }
    }
}
