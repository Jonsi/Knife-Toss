using Game.Economy.Data;
using Game.Economy.Interfaces;
using Game.UI.Common;
using UnityEngine;
using VContainer;

namespace Game.UI.Store
{
    /// <summary>
    /// Store-specific adapter for the shared currency balance display.
    /// </summary>
    public class StoreBalanceChipView : MonoBehaviour
    {
        [SerializeField] private CurrencyBalanceDisplay _BalanceDisplay;

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

        public void PlayAccumulationFromUiElement(RectTransform source, int amount)
        {
            _BalanceDisplay?.PlayAccumulationFromUiElement(source, amount);
        }
    }
}
