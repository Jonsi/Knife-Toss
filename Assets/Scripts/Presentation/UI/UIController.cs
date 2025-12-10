using Infrastructure.Events;
using KnifeToss.Core.Data.Level;
using KnifeToss.Core.Domain.Level;
using KnifeToss.Presentation.UI.Popups;
using UnityEngine;

namespace KnifeToss.Presentation.UI
{
    public class UIController : MonoBehaviour
    {
        [Header("Popups")]
        [SerializeField] private StageFailPopup _stageFailPopup;

        [Header("Channels")]
        [SerializeField] private LevelStateChangedEventChannel _stateChangedChannel;

        private void OnEnable()
        {
            if (_stateChangedChannel != null)
                _stateChangedChannel.Subscribe(OnLevelStateChanged);
        }

        private void OnDisable()
        {
            if (_stateChangedChannel != null)
                _stateChangedChannel.Unsubscribe(OnLevelStateChanged);
        }

        private void OnLevelStateChanged(LevelStateChangedEventData data)
        {
            if (data.CurrentState == nameof(LevelFailedState))
            {
                if (_stageFailPopup != null)
                    _stageFailPopup.Show();
            }
            else if (data.CurrentState == nameof(LevelPlayingState) || data.CurrentState == nameof(LevelLoadingState))
            {
                if (_stageFailPopup != null)
                    _stageFailPopup.Hide();
            }
        }
    }
}
