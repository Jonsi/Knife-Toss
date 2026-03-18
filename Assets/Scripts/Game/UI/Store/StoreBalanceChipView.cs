using Game.UI.Common;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Store-specific adapter for the shared currency balance display.
    /// </summary>
    public class StoreBalanceChipView : MonoBehaviour
    {
        [SerializeField] private CurrencyBalanceDisplay _BalanceDisplay;

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
