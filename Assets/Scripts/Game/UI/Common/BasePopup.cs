using Game.UI.Interfaces;
using UnityEngine;
using VContainer;

namespace Game.UI.Common
{
    /// <summary>
    /// Base class for UI popups providing common show/hide functionality.
    /// </summary>
    public abstract class BasePopup : MonoBehaviour
    {
        [SerializeField] private bool _PauseGameplayWhileVisible = true;

        private IGamePauseController _GamePauseController;
        private PopupFadeInAnimation _PopupAnimation;
        private bool _HasPauseRequest;
        private bool _IsHiding;
        private bool _IsInitialized;

        [Inject]
        private void Construct(IGamePauseController gamePauseController)
        {
            _GamePauseController = gamePauseController;
        }

        public virtual void Show()
        {
            EnsureInitialized();

            if (gameObject.activeSelf)
            {
                if (_IsHiding)
                {
                    _IsHiding = false;
                    _PopupAnimation?.PlayShow();
                }

                return;
            }

            _IsHiding = false;
            TryAcquirePause();
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            EnsureInitialized();

            if (!gameObject.activeSelf)
            {
                TryReleasePause();
                _IsHiding = false;
                return;
            }

            if (_IsHiding)
                return;

            if (_PopupAnimation == null)
            {
                TryReleasePause();
                gameObject.SetActive(false);
                return;
            }

            _IsHiding = true;
            _PopupAnimation.PlayHide(CompleteHide);
        }

        protected virtual bool ShouldPauseGameplayWhileVisible => _PauseGameplayWhileVisible;

        private void OnDisable()
        {
            TryReleasePause();
        }

        private void OnDestroy()
        {
            TryReleasePause();
        }

        private void CompleteHide()
        {
            if (!_IsHiding)
                return;

            _IsHiding = false;
            TryReleasePause();
            gameObject.SetActive(false);
        }

        private void TryAcquirePause()
        {
            if (_HasPauseRequest || !ShouldPauseGameplayWhileVisible)
                return;

            _GamePauseController?.AcquirePause(this);
            _HasPauseRequest = _GamePauseController != null;
        }

        private void TryReleasePause()
        {
            if (!_HasPauseRequest)
                return;

            _GamePauseController?.ReleasePause(this);
            _HasPauseRequest = false;
        }

        private void EnsureInitialized()
        {
            if (_IsInitialized)
                return;

            _PopupAnimation = GetComponent<PopupFadeInAnimation>();
            _IsInitialized = true;
        }
    }
}
