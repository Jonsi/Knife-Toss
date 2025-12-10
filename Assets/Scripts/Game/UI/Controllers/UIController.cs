using Common.Events;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.UI.Popups;
using UnityEngine;

namespace Game.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [Header("Popups")]
        [SerializeField] private StageFailPopup _StageFailPopup;

        [Header("Channels")]
        [SerializeField] private LevelFailedEventChannel _LevelFailedChannel;
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;

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
    }
}
