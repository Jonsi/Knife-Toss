using Common.Events;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _RestartStageButton;

        [Header("Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;

        private void OnEnable()
        {
            _RetryButton.onClick.AddListener(OnRetryClicked);
            _RestartStageButton.onClick.AddListener(OnRestartStageClicked);
        }

        private void OnDisable()
        {
            _RetryButton.onClick.RemoveListener(OnRetryClicked);
            _RestartStageButton.onClick.RemoveListener(OnRestartStageClicked);
        }

        private void OnRetryClicked()
        {
            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Raise();
        }

        private void OnRestartStageClicked()
        {
            if (_RestartStageChannel != null)
                _RestartStageChannel.Raise();
        }
    }
}
