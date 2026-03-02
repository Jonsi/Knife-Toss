using System;
using Game.Economy.Data;
using Game.Store.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Base store card view. Concrete card types provide the prefab-specific implementation.
    /// </summary>
    public abstract class StoreCardView : MonoBehaviour
    {
        [field: SerializeField] public PricePillBinding PricePill { get; private set; }
        [field: SerializeField] public Image IconImage { get; private set; }
        [field: SerializeField] public Image ShadowImage { get; private set; }
        [field: SerializeField] public Button BuyButton { get; private set; }

        protected PurchasableEntry Entry { get; private set; }
        private Action<PurchasableEntry> _OnBuyClicked;

        public void Setup(PurchasableEntry entry, Action<PurchasableEntry> onBuyClicked)
        {
            Entry = entry;
            _OnBuyClicked = onBuyClicked;

            if (PricePill != null)
            {
                PricePill.SetPrice(entry?.Price?.Amount ?? 0, entry?.Price?.Currency ?? CurrencyType.None);
            }

            BindEntry(entry);

            if (BuyButton != null)
            {
                BuyButton.onClick.RemoveListener(OnBuyPressed);
                BuyButton.onClick.AddListener(OnBuyPressed);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (BuyButton != null)
            {
                BuyButton.interactable = interactable;
            }
        }

        protected virtual void BindEntry(PurchasableEntry entry)
        {
        }

        protected void SetDisplaySprite(Sprite sprite)
        {
            if (IconImage != null)
            {
                IconImage.sprite = sprite;
            }

            if (ShadowImage != null)
            {
                ShadowImage.sprite = sprite;
            }
        }

        protected virtual void OnDisable()
        {
            if (BuyButton != null)
            {
                BuyButton.onClick.RemoveListener(OnBuyPressed);
            }
        }

        private void OnBuyPressed()
        {
            if (Entry != null)
            {
                _OnBuyClicked?.Invoke(Entry);
            }
        }

        [Serializable]
        public class PricePillBinding
        {
            [field: SerializeField] public TMP_Text AmountText { get; private set; }

            public void SetPrice(int amount, CurrencyType currencyType)
            {
                if (AmountText != null)
                {
                    AmountText.text = amount.ToString();
                }
            }
        }
    }
}
