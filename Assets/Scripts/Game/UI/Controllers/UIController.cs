using KnifeToss.Game.Level.Data;
using KnifeToss.Game.Level.Events;
using KnifeToss.Game.Level.States;
using KnifeToss.Game.UI.Popups;
using UnityEngine;

namespace KnifeToss.Game.UI.Controllers
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
