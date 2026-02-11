using System;
using Game.Store.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Reusable store card view for both featured and power-up entries.
    /// </summary>
    public class StoreCardView : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text NameText { get; private set; }
        [field: SerializeField] public TMP_Text DescriptionText { get; private set; }
        [field: SerializeField] public PricePillView PricePill { get; private set; }
        [field: SerializeField] public Button BuyButton { get; private set; }
        [field: SerializeField] public Button WatchAdButton { get; private set; }
        [field: SerializeField] public TMP_Text WatchAdRewardText { get; private set; }
        [field: SerializeField] public GameObject LockedOverlay { get; private set; }

        private PurchasableEntry _Entry;
        private Action<PurchasableEntry> _OnBuyClicked;
        private Action<PurchasableEntry> _OnWatchAdClicked;

        public void Setup(
            PurchasableEntry entry,
            int adRewardAmount,
            bool showWatchAdButton,
            Action<PurchasableEntry> onBuyClicked,
            Action<PurchasableEntry> onWatchAdClicked)
        {
            _Entry = entry;
            _OnBuyClicked = onBuyClicked;
            _OnWatchAdClicked = onWatchAdClicked;

            if (NameText != null)
            {
                NameText.text = entry?.DisplayName ?? string.Empty;
            }

            if (DescriptionText != null)
            {
                DescriptionText.text = entry?.Description ?? string.Empty;
            }

            if (PricePill != null)
            {
                PricePill.SetPrice(entry?.Price?.Amount ?? 0, entry?.Price?.CurrencyId);
            }

            if (WatchAdRewardText != null)
            {
                WatchAdRewardText.text = $"+{adRewardAmount}";
            }

            if (BuyButton != null)
            {
                BuyButton.onClick.RemoveListener(OnBuyPressed);
                BuyButton.onClick.AddListener(OnBuyPressed);
            }

            if (WatchAdButton != null)
            {
                WatchAdButton.gameObject.SetActive(showWatchAdButton);
                WatchAdButton.onClick.RemoveListener(OnWatchAdPressed);
                WatchAdButton.onClick.AddListener(OnWatchAdPressed);
            }

            gameObject.SetActive(entry != null);
        }

        public void SetInteractable(bool interactable)
        {
            if (BuyButton != null)
            {
                BuyButton.interactable = interactable;
            }

            if (WatchAdButton != null && WatchAdButton.gameObject.activeSelf)
            {
                WatchAdButton.interactable = interactable;
            }

            if (LockedOverlay != null)
            {
                LockedOverlay.SetActive(!interactable);
            }
        }

        private void OnDisable()
        {
            if (BuyButton != null)
            {
                BuyButton.onClick.RemoveListener(OnBuyPressed);
            }

            if (WatchAdButton != null)
            {
                WatchAdButton.onClick.RemoveListener(OnWatchAdPressed);
            }
        }

        private void OnBuyPressed()
        {
            if (_Entry != null)
            {
                _OnBuyClicked?.Invoke(_Entry);
            }
        }

        private void OnWatchAdPressed()
        {
            if (_Entry != null)
            {
                _OnWatchAdClicked?.Invoke(_Entry);
            }
        }
    }
}
