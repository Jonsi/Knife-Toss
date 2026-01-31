using Common.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Flow.Events;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.UI.HUD
{
    /// <summary>
    /// Displays the player's coin balance (first currency in the catalog).
    /// </summary>
    public class CoinsUiDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _CountText;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private CurrencyBalanceChangedEventChannel _BalanceChangedChannel;

        private ICurrencyService _CurrencyService;
        private string _CoinCurrencyId;

        [Inject]
        private void Construct(ICurrencyService currencyService, CurrencyTypeCatalog catalog)
        {
            _CurrencyService = currencyService;
            var configs = catalog?.GetAllConfigs();
            _CoinCurrencyId = (configs != null && configs.Count > 0 && configs[0] != null)
                ? (configs[0].TypeId ?? string.Empty)
                : string.Empty;
        }

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            if (_BalanceChangedChannel != null)
                _BalanceChangedChannel.Subscribe(OnBalanceChanged);

            SyncFromCurrency();
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_BalanceChangedChannel != null)
                _BalanceChangedChannel.Unsubscribe(OnBalanceChanged);
        }

        private void OnLevelStarted(LevelStartedEventData _)
        {
            SyncFromCurrency();
        }

        private void OnBalanceChanged(CurrencyBalanceChangedEventData data)
        {
            if (!string.IsNullOrEmpty(_CoinCurrencyId) && data.CurrencyId == _CoinCurrencyId)
                UpdateCountText(data.NewBalance);
        }

        private void SyncFromCurrency()
        {
            if (_CurrencyService == null || string.IsNullOrEmpty(_CoinCurrencyId))
                return;

            UpdateCountText(_CurrencyService.GetBalance(_CoinCurrencyId));
        }

        private void UpdateCountText(int value)
        {
            if (_CountText != null)
                _CountText.text = value.ToString();
        }
    }
}
