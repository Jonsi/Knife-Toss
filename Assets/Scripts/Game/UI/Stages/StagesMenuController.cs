using Cysharp.Threading.Tasks;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Flow.Game;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Stages
{
    /// <summary>
    /// Stages Menu scene navigation. World rows are built by <see cref="StageListController"/>.
    /// </summary>
    public class StagesMenuController : MonoBehaviour
    {
        [SerializeField] private Button _BackButton;

        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private FeedbackEventChannel _FeedbackEventChannel;

        private ActionButtonPresenter _BackButtonPresenter;

        private void OnEnable()
        {
            if (_BackButton != null)
            {
                _BackButtonPresenter = ActionButtonPresenter.GetOrAdd(_BackButton, _FeedbackEventChannel);
                _BackButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDisable()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        private void OnBackClicked()
        {
            if (_BackButtonPresenter != null && !_BackButtonPresenter.TryBeginSubmission())
            {
                return;
            }

            LoadMainMenuAsync().Forget();
        }

        private async UniTaskVoid LoadMainMenuAsync()
        {
            try
            {
                await _SceneLoaderService.LoadMainMenu();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                _BackButtonPresenter?.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.Blocked,
                    "navigation.failed",
                    gameObject.name));
            }
        }
    }
}
