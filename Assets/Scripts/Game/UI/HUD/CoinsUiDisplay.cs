using Game.Flow.Events;
using Game.UI.Common;
using UnityEngine;

namespace Game.UI.HUD
{
    /// <summary>
    /// Stage-specific adapter for the shared currency balance display.
    /// </summary>
    public class CoinsUiDisplay : MonoBehaviour
    {
        [SerializeField] private CurrencyBalanceDisplay _BalanceDisplay;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;

        private void Awake()
        {
            _BalanceDisplay ??= GetComponent<CurrencyBalanceDisplay>();
        }

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
            {
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            }
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
            {
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            }
        }

        private void OnLevelStarted(LevelStartedEventData _)
        {
            _BalanceDisplay?.RefreshImmediate();
        }

        public void PlayAccumulationFromWorld(Vector2 worldPosition, int amount)
        {
            _BalanceDisplay?.PlayAccumulationFromWorld(worldPosition, amount);
        }
    }
}
