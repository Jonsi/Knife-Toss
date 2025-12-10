using KnifeToss.Game.Level.States;
using KnifeToss.Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace KnifeToss.Game.UI.Popups
{
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _restartStageButton;

        private StageStateMachine _StateMachine;

        [Inject]
        public void Construct(StageStateMachine stateMachine)
        {
            _StateMachine = stateMachine;
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
            if (_StateMachine != null)
            {
                _StateMachine.Failed.RetryLevel();
            }
        }

        private void OnRestartStageClicked()
        {
            if (_StateMachine != null)
            {
                _StateMachine.Failed.RestartStage();
            }
        }
    }
}
