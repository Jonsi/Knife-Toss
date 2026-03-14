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
        private bool _HasPauseRequest;

        [Inject]
        private void Construct(IGamePauseController gamePauseController)
        {
            _GamePauseController = gamePauseController;
        }

        public virtual void Show()
        {
            if (gameObject.activeSelf)
                return;

            TryAcquirePause();
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            if (!gameObject.activeSelf)
            {
                TryReleasePause();
                return;
            }

            TryReleasePause();
            gameObject.SetActive(false);
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
    }
}
