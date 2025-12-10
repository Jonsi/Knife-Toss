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
            if (data.currentState == nameof(LevelFailedState))
            {
                if (_StageFailPopup != null)
                    _StageFailPopup.Show();
            }
            else if (data.currentState == nameof(LevelPlayingState) || data.currentState == nameof(LevelLoadingState))
            {
                if (_StageFailPopup != null)
                    _StageFailPopup.Hide();
            }
        }
    }
}
