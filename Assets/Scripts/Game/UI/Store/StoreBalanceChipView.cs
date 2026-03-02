using Game.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.UI.Store
{
    /// <summary>
    /// Top-right currency balance chip.
    /// </summary>
    public class StoreBalanceChipView : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text BalanceText { get; private set; }
        [field: SerializeField] public CurrencyType Currency { get; private set; }

        private ICurrencyService _CurrencyService;
        private CurrencyBalanceChangedEventChannel _BalanceChangedChannel;
        private string _ResolvedCurrencyId;
        private bool _IsSubscribed;

        [Inject]
        private void Construct(
            ICurrencyService currencyService,
            CurrencyTypeCatalog currencyTypeCatalog,
            CurrencyBalanceChangedEventChannel balanceChangedChannel)
        {
            _CurrencyService = currencyService;
            _BalanceChangedChannel = balanceChangedChannel;

            if (Currency == CurrencyType.None)
            {
                var currencies = currencyTypeCatalog?.GetAllConfigs();
                if (currencies != null && currencies.Count > 0 && currencies[0] != null)
                {
                    Currency = CurrencyTypeExtensions.FromTypeId(currencies[0].TypeId);
                }
            }

            _ResolvedCurrencyId = Currency.ToTypeId();

            if (isActiveAndEnabled)
            {
                Subscribe();
                Refresh();
            }
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetBalance(int value)
        {
            if (BalanceText != null)
            {
                BalanceText.text = value.ToString();
            }
        }

        private void Refresh()
        {
            if (_CurrencyService == null || string.IsNullOrEmpty(_ResolvedCurrencyId))
            {
                return;
            }

            SetBalance(_CurrencyService.GetBalance(_ResolvedCurrencyId));
        }

        private void OnBalanceChanged(CurrencyBalanceChangedEventData data)
        {
            if (!string.IsNullOrEmpty(_ResolvedCurrencyId) && data.CurrencyId == _ResolvedCurrencyId)
            {
                SetBalance(data.NewBalance);
            }
        }

        private void Subscribe()
        {
            if (_IsSubscribed || _BalanceChangedChannel == null)
            {
                return;
            }

            _BalanceChangedChannel.Subscribe(OnBalanceChanged);
            _IsSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_IsSubscribed || _BalanceChangedChannel == null)
            {
                return;
            }

            _BalanceChangedChannel.Unsubscribe(OnBalanceChanged);
            _IsSubscribed = false;
        }
    }
}
