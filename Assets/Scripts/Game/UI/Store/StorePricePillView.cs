using Game.Economy.Data;
using TMPro;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Shared, prefab-owned Store price presentation. Currency and amount remain Unity UI content.
    /// </summary>
    public sealed class StorePricePillView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _AmountText;

        public TMP_Text AmountText => _AmountText;

        public void SetPrice(int amount, CurrencyType currencyType)
        {
            if (_AmountText != null)
            {
                _AmountText.text = amount.ToString();
            }
        }

        public void SetCustomPrice(string text)
        {
            if (_AmountText != null)
            {
                _AmountText.text = text ?? string.Empty;
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
