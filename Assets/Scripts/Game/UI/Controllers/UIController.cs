using Game.Level.Data;
using Game.Level.Events;
using Game.Level.States;
using Game.UI.Popups;
using UnityEngine;

namespace Game.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [Header("Popups")]
        [SerializeField] private StageFailPopup _StageFailPopup;

        [Header("Channels")]
        [SerializeField] private LevelStateChangedEventChannel _StateChangedChannel;

        private void OnEnable()
        {
            if (_StateChangedChannel != null)
                _StateChangedChannel.Subscribe(OnLevelStateChanged);
        }

        private void OnDisable()
        {
            if (_StateChangedChannel != null)
                _StateChangedChannel.Unsubscribe(OnLevelStateChanged);
        }

        private void OnLevelStateChanged(LevelStateChangedEventData data)
        {
            if (data.CurrentState == nameof(LevelFailedState))
            {
                if (_StageFailPopup != null)
                    _StageFailPopup.Show();
            }
            else if (data.CurrentState == nameof(LevelPlayingState) || data.CurrentState == nameof(LevelLoadingState))
            {
                if (_StageFailPopup != null)
                    _StageFailPopup.Hide();
            }
        }
    }
}
