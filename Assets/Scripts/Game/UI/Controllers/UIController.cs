using Common.Events;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.UI.Common;
using Game.UI.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image _BackgroundImage;

        [Header("Popups")]
        [SerializeField] private StageFailPopup _StageFailPopup;

        [Header("Startup Animation")]
        [SerializeField] private UiElementAnimator[] _StartupAnimators;

        [Header("Channels")]
        [SerializeField] private LevelFailedEventChannel _LevelFailedChannel;
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;

        private SettingsPopupController _SettingsPopupController;

        [VContainer.Inject]
        public void Construct(SettingsPopupController settingsPopupController)
        {
            _SettingsPopupController = settingsPopupController;
        }

        public void ToggleSettings()
        {
            if (_SettingsPopupController != null)
            {
                _SettingsPopupController.ToggleSettings();
            }
        }

        public void SetBackground(Sprite sprite)
        {
            if (_BackgroundImage != null && sprite != null)
            {
                _BackgroundImage.sprite = sprite;
            }
        }

        private void Start()
        {
            PlayStartupAnimationsAsync().Forget();
        }

        private void OnEnable()
        {
            if (_LevelFailedChannel != null)
                _LevelFailedChannel.Subscribe(OnLevelFailed);
            
            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Subscribe(OnRetryOrRestart);

            if (_RestartStageChannel != null)
                _RestartStageChannel.Subscribe(OnRetryOrRestart);
        }

        private void OnDisable()
        {
            if (_LevelFailedChannel != null)
                _LevelFailedChannel.Unsubscribe(OnLevelFailed);

            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Unsubscribe(OnRetryOrRestart);

            if (_RestartStageChannel != null)
                _RestartStageChannel.Unsubscribe(OnRetryOrRestart);
        }

        private void OnLevelFailed(LevelData data)
        {
            if (_StageFailPopup != null)
                _StageFailPopup.Show();
        }

        private void OnRetryOrRestart()
        {
            if (_StageFailPopup != null)
                _StageFailPopup.Hide();
        }

        private async UniTaskVoid PlayStartupAnimationsAsync()
        {
            if (_StartupAnimators == null || _StartupAnimators.Length == 0)
            {
                return;
            }

            await DeferredGameSceneVisibility.WaitForReveal();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            foreach (UiElementAnimator animator in _StartupAnimators)
            {
                animator?.PlayIn();
            }
        }
    }
}
