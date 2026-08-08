using Cysharp.Threading.Tasks;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Flow.Game;
using Game.UI.Controllers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Common
{
    /// <summary>
    /// Loads the main menu scene when the assigned button is clicked.
    /// </summary>
    public class LoadMainMenuButton : MonoBehaviour
    {
        [SerializeField] private Button _Button;

        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private FeedbackEventChannel _FeedbackEventChannel;
        private UIController _UIController;
        private bool _IsLoading;
        private ActionButtonPresenter _Presenter;

        private void Awake()
        {
            if (_Button == null)
            {
                _Button = GetComponent<Button>();
            }

            _UIController = GetComponentInParent<UIController>();
        }

        private void Start()
        {
            _Presenter = ActionButtonPresenter.GetOrAdd(_Button, _FeedbackEventChannel);
            if (_Button != null)
            {
                _Button.onClick.AddListener(OnClicked);
            }
        }

        private void OnDestroy()
        {
            if (_Button != null)
            {
                _Button.onClick.RemoveListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            LoadMainMenuAsync().Forget();
        }

        private async UniTaskVoid LoadMainMenuAsync()
        {
            if (_IsLoading)
            {
                return;
            }

            if (_Presenter != null && !_Presenter.TryBeginSubmission())
            {
                return;
            }

            _IsLoading = true;
            try
            {
                if (_UIController != null)
                {
                    await _UIController.PlayStageOutAsync();
                }

                if (_SceneLoaderService != null)
                {
                    await _SceneLoaderService.LoadMainMenu();
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                _IsLoading = false;
                _Presenter?.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.Blocked,
                    "navigation.failed",
                    gameObject.name));
            }
        }
    }
}
