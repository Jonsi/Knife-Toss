using TMPro;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Displays a generic currency price (amount + optional currency id label).
    /// Ready for multi-currency stores.
    /// </summary>
    public class PricePillView : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text AmountText { get; private set; }
        [field: SerializeField] public TMP_Text CurrencyIdText { get; private set; }

        public void SetPrice(int amount, string currencyId)
        {
            if (AmountText != null)
            {
                AmountText.text = amount.ToString();
            }

            if (CurrencyIdText != null)
            {
                CurrencyIdText.text = string.IsNullOrEmpty(currencyId) ? string.Empty : currencyId.ToUpperInvariant();
            }
        }
    }
}
