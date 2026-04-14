using Game.Economy.Data;
using Game.Economy.Interfaces;
using Game.Flow.Events;
using Game.UI.Common;
using UnityEngine;
using VContainer;

namespace Game.UI.HUD
{
    /// <summary>
    /// Stage-specific adapter for the shared currency balance display.
    /// </summary>
    public class CoinsUiDisplay : MonoBehaviour
    {
        [SerializeField] private CurrencyBalanceDisplay _BalanceDisplay;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;

        [Inject]
        private void Construct(
            ICurrencyService currencyService,
            CurrencyTypeCatalog currencyTypeCatalog,
            CurrencyAccumulationConfig accumulationConfig)
        {
            _BalanceDisplay ??= GetComponent<CurrencyBalanceDisplay>();
            _BalanceDisplay?.Initialize(currencyService, currencyTypeCatalog, accumulationConfig);
        }

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

        public void PlayAccumulationFromUiElement(RectTransform source, int amount)
        {
            _BalanceDisplay?.PlayAccumulationFromUiElement(source, amount);
        }
    }
}
