using Infrastructure.Events;
using KnifeToss.Core.Abstractions.Flow;
using KnifeToss.Core.Abstractions.Leveling;
using KnifeToss.Core.Data.Commands;
using KnifeToss.Core.Domain.Level.Commands;
using KnifeToss.Presentation.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace KnifeToss.Presentation.UI.Popups
{
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _restartStageButton;

        [Header("Channels")]
        [SerializeField] private CommandEventChannel _commandChannel;

        private IStageNavigator _navigator;
        private IStageProgressTracker _tracker;
        private ILevelSetupService _levelSetup;

        [Inject]
        public void Construct(IStageNavigator navigator, IStageProgressTracker tracker, ILevelSetupService levelSetup)
        {
            _navigator = navigator;
            _tracker = tracker;
            _levelSetup = levelSetup;
        }

        private void OnEnable()
        {
            _retryButton.onClick.AddListener(OnRetryClicked);
            _restartStageButton.onClick.AddListener(OnRestartStageClicked);
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(OnRetryClicked);
            _restartStageButton.onClick.RemoveListener(OnRestartStageClicked);
        }

        private void OnRetryClicked()
        {
            if (_commandChannel != null && _navigator != null && _tracker != null && _levelSetup != null)
            {
                var command = new RetryLevelCommand(_navigator, _tracker, _levelSetup);
                _commandChannel.Raise(new CommandEventData(command));
            }
        }

        private void OnRestartStageClicked()
        {
            if (_commandChannel != null && _navigator != null && _tracker != null && _levelSetup != null)
            {
                var command = new RestartStageCommand(_navigator, _tracker, _levelSetup);
                _commandChannel.Raise(new CommandEventData(command));
            }
        }
    }
}
