using Game.UI.Interfaces;
using Game.UI.Services;
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
        private IModalStackCoordinator _ModalStackCoordinator;
        private IModalRegistration _ModalRegistration;
        private CanvasGroup _ModalCanvasGroup;
        private PopupFadeInAnimation _PopupAnimation;
        private bool _HasPauseRequest;
        private bool _IsModalStateSubscribed;
        private bool _IsHiding;
        private bool _IsInitialized;

        [Inject]
        public void Construct(
            IGamePauseController gamePauseController,
            IModalStackCoordinator modalStackCoordinator)
        {
            _GamePauseController = gamePauseController;
            _ModalStackCoordinator = modalStackCoordinator;
        }

        public virtual void Show()
        {
            EnsureInitialized();

            if (gameObject.activeSelf)
            {
                TryAcquireModalAuthority();

                if (_IsHiding)
                {
                    _IsHiding = false;
                    _PopupAnimation?.PlayShow();
                }

                return;
            }

            _IsHiding = false;
            TryAcquirePause();
            TryAcquireModalAuthority();
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            EnsureInitialized();

            if (!gameObject.activeSelf)
            {
                TryReleasePause();
                TryReleaseModalAuthority();
                _IsHiding = false;
                return;
            }

            if (_IsHiding)
                return;

            if (_PopupAnimation == null)
            {
                TryReleasePause();
                TryReleaseModalAuthority();
                gameObject.SetActive(false);
                return;
            }

            _IsHiding = true;
            _PopupAnimation.PlayHide(CompleteHide);
        }

        protected virtual bool ShouldPauseGameplayWhileVisible => _PauseGameplayWhileVisible;

        protected virtual void OnDisable()
        {
            TryReleasePause();
            TryReleaseModalAuthority();
        }

        protected virtual void OnDestroy()
        {
            TryReleasePause();
            TryReleaseModalAuthority();
        }

        private void CompleteHide()
        {
            if (!_IsHiding)
                return;

            _IsHiding = false;
            TryReleasePause();
            TryReleaseModalAuthority();
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

        private void TryAcquireModalAuthority()
        {
            if (_ModalRegistration != null || _ModalStackCoordinator == null)
            {
                return;
            }

            EnsureModalCanvasGroup();
            SubscribeToModalState();
            _ModalRegistration = _ModalStackCoordinator.Acquire(this);
            ApplyModalInputAuthority(_ModalStackCoordinator.State);
        }

        private void TryReleaseModalAuthority()
        {
            IModalRegistration registration = _ModalRegistration;
            _ModalRegistration = null;
            registration?.Dispose();
            UnsubscribeFromModalState();

            if (_ModalCanvasGroup != null)
            {
                _ModalCanvasGroup.interactable = false;
                _ModalCanvasGroup.blocksRaycasts = false;
            }
        }

        private void SubscribeToModalState()
        {
            if (_IsModalStateSubscribed || _ModalStackCoordinator == null)
            {
                return;
            }

            _ModalStackCoordinator.StateChanged += ApplyModalInputAuthority;
            _IsModalStateSubscribed = true;
        }

        private void UnsubscribeFromModalState()
        {
            if (!_IsModalStateSubscribed || _ModalStackCoordinator == null)
            {
                _IsModalStateSubscribed = false;
                return;
            }

            _ModalStackCoordinator.StateChanged -= ApplyModalInputAuthority;
            _IsModalStateSubscribed = false;
        }

        private void ApplyModalInputAuthority(ModalStackState state)
        {
            EnsureModalCanvasGroup();
            if (_ModalCanvasGroup == null)
            {
                return;
            }

            bool isTopModal = object.ReferenceEquals(state.TopOwner, this);
            _ModalCanvasGroup.interactable = isTopModal;
            _ModalCanvasGroup.blocksRaycasts = isTopModal;
        }

        private void EnsureModalCanvasGroup()
        {
            if (_ModalCanvasGroup != null)
            {
                return;
            }

            _ModalCanvasGroup = GetComponent<CanvasGroup>();
            if (_ModalCanvasGroup == null)
            {
                _ModalCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void EnsureInitialized()
        {
            if (_IsInitialized)
                return;

            _PopupAnimation = GetComponent<PopupFadeInAnimation>();
            EnsureModalCanvasGroup();
            _IsInitialized = true;
        }
    }
}
