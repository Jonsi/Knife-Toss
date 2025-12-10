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
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _RestartStageButton;

        private StageStateMachine _StateMachine;

        [Inject]
        public void Construct(StageStateMachine stateMachine)
        {
            _StateMachine = stateMachine;
        }

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
            if (_StateMachine != null)
            {
                _StateMachine.failed.RetryLevel();
            }
        }

        private void OnRestartStageClicked()
        {
            if (_StateMachine != null)
            {
                _StateMachine.failed.RestartStage();
            }
        }
    }
}
