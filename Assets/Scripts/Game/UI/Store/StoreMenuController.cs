using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Flow.Game;
using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Store
{
    /// <summary>
    /// Controller for the Store scene. Binds catalog data to UI and drives purchases.
    /// </summary>
    public class StoreMenuController : MonoBehaviour
    {
        [Header("Store UI")]
        [field: SerializeField] public StoreBalanceChipView BalanceChipView { get; private set; }
        [field: SerializeField] public StoreCardView[] FeaturedItemSlots { get; private set; }
        [field: SerializeField] public StoreCardView[] PowerUpItemSlots { get; private set; }
        [field: SerializeField] public StoreAdCtaView BottomAdCtaView { get; private set; }
        [field: SerializeField] public Button CloseButton { get; private set; }

        [Header("Feedback")]
        [field: SerializeField] public StorePurchaseFeedbackPresenter FeedbackPresenter { get; private set; }

        [Header("Ad Hooks")]
        [field: SerializeField] public int WatchAdRewardAmount { get; private set; } = 50;
        [field: SerializeField] public UnityEvent OnBottomAdClicked { get; private set; }
        [field: SerializeField] public UnityEvent OnPowerUpAdClicked { get; private set; }

        [Header("Currency")]
        [field: SerializeField] public string PrimaryCurrencyId { get; private set; }

        private StoreCatalog _StoreCatalog;
        private IPurchaseService _PurchaseService;
        private ICurrencyService _CurrencyService;
        private CurrencyTypeCatalog _CurrencyTypeCatalog;
        private SceneLoaderService _SceneLoaderService;
        private PurchaseStartedEventChannel _PurchaseStartedChannel;
        private PurchaseCompletedEventChannel _PurchaseCompletedChannel;
        private PurchaseFailedEventChannel _PurchaseFailedChannel;
        private CurrencyBalanceChangedEventChannel _CurrencyBalanceChangedChannel;

        [Inject]
        private void Construct(
            StoreCatalog storeCatalog,
            IPurchaseService purchaseService,
            ICurrencyService currencyService,
            CurrencyTypeCatalog currencyTypeCatalog,
            SceneLoaderService sceneLoaderService,
            PurchaseStartedEventChannel purchaseStartedChannel,
            PurchaseCompletedEventChannel purchaseCompletedChannel,
            PurchaseFailedEventChannel purchaseFailedChannel,
            CurrencyBalanceChangedEventChannel currencyBalanceChangedChannel)
        {
            _StoreCatalog = storeCatalog;
            _PurchaseService = purchaseService;
            _CurrencyService = currencyService;
            _CurrencyTypeCatalog = currencyTypeCatalog;
            _SceneLoaderService = sceneLoaderService;
            _PurchaseStartedChannel = purchaseStartedChannel;
            _PurchaseCompletedChannel = purchaseCompletedChannel;
            _PurchaseFailedChannel = purchaseFailedChannel;
            _CurrencyBalanceChangedChannel = currencyBalanceChangedChannel;
        }

        private void Start()
        {
            BindCloseButton();
            BindBottomAdCta();
            BindCatalog();
            ResolvePrimaryCurrencyId();
            RefreshBalance();
            RefreshInteractableState();
        }

        private void OnEnable()
        {
            _PurchaseStartedChannel?.Subscribe(OnPurchaseStarted);
            _PurchaseCompletedChannel?.Subscribe(OnPurchaseCompleted);
            _PurchaseFailedChannel?.Subscribe(OnPurchaseFailed);
            _CurrencyBalanceChangedChannel?.Subscribe(OnCurrencyChanged);
        }

        private void OnDisable()
        {
            _PurchaseStartedChannel?.Unsubscribe(OnPurchaseStarted);
            _PurchaseCompletedChannel?.Unsubscribe(OnPurchaseCompleted);
            _PurchaseFailedChannel?.Unsubscribe(OnPurchaseFailed);
            _CurrencyBalanceChangedChannel?.Unsubscribe(OnCurrencyChanged);

            if (CloseButton != null)
            {
                CloseButton.onClick.RemoveListener(OnCloseClicked);
            }
        }

        private void BindCloseButton()
        {
            if (CloseButton == null)
            {
                return;
            }

            CloseButton.onClick.RemoveListener(OnCloseClicked);
            CloseButton.onClick.AddListener(OnCloseClicked);
        }

        private void BindBottomAdCta()
        {
            BottomAdCtaView?.Setup(WatchAdRewardAmount, OnBottomAdPressed);
        }

        private void BindCatalog()
        {
            BindFeaturedItems(_StoreCatalog?.CurrencyPacks);
            BindPowerUpItems(_StoreCatalog?.PowerUps);
        }

        private void BindFeaturedItems(IReadOnlyList<PurchasableEntry> entries)
        {
            var slots = FeaturedItemSlots ?? Array.Empty<StoreCardView>();
            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                var entry = entries != null && i < entries.Count ? entries[i] : null;
                slot.Setup(entry, WatchAdRewardAmount, false, OnBuyClicked, OnPowerUpAdPressed);
            }
        }

        private void BindPowerUpItems(IReadOnlyList<PurchasableEntry> entries)
        {
            var slots = PowerUpItemSlots ?? Array.Empty<StoreCardView>();
            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                var entry = entries != null && i < entries.Count ? entries[i] : null;
                slot.Setup(entry, WatchAdRewardAmount, true, OnBuyClicked, OnPowerUpAdPressed);
            }
        }

        private void ResolvePrimaryCurrencyId()
        {
            if (!string.IsNullOrEmpty(PrimaryCurrencyId))
            {
                return;
            }

            var currencies = _CurrencyTypeCatalog?.GetAllConfigs();
            if (currencies != null && currencies.Count > 0 && currencies[0] != null)
            {
                PrimaryCurrencyId = currencies[0].TypeId;
            }
        }

        private void RefreshBalance()
        {
            if (_CurrencyService == null || string.IsNullOrEmpty(PrimaryCurrencyId))
            {
                return;
            }

            var balance = _CurrencyService.GetBalance(PrimaryCurrencyId);
            BalanceChipView?.SetBalance(balance);
        }

        private void RefreshInteractableState()
        {
            var canInteract = _PurchaseService == null || !_PurchaseService.IsPurchaseInProgress;

            if (FeaturedItemSlots != null)
            {
                foreach (var slot in FeaturedItemSlots)
                {
                    slot?.SetInteractable(canInteract);
                }
            }

            if (PowerUpItemSlots != null)
            {
                foreach (var slot in PowerUpItemSlots)
                {
                    slot?.SetInteractable(canInteract);
                }
            }

            BottomAdCtaView?.SetInteractable(canInteract);
            if (CloseButton != null)
            {
                CloseButton.interactable = canInteract;
            }
        }

        private void OnBuyClicked(PurchasableEntry entry)
        {
            TryPurchaseAsync(entry).Forget();
        }

        private async UniTaskVoid TryPurchaseAsync(PurchasableEntry entry)
        {
            if (_PurchaseService == null || entry == null)
            {
                return;
            }

            await _PurchaseService.TryPurchaseAsync(entry);
            RefreshInteractableState();
        }

        private void OnPurchaseStarted(PurchaseStartedEventData _)
        {
            FeedbackPresenter?.ShowLoading(true);
            RefreshInteractableState();
        }

        private void OnPurchaseCompleted(PurchaseCompletedEventData _)
        {
            FeedbackPresenter?.ShowLoading(false);
            FeedbackPresenter?.ShowSuccess("Purchase successful");
            RefreshBalance();
            RefreshInteractableState();
        }

        private void OnPurchaseFailed(PurchaseFailedEventData data)
        {
            FeedbackPresenter?.ShowLoading(false);
            FeedbackPresenter?.ShowError(data.Reason);
            RefreshBalance();
            RefreshInteractableState();
        }

        private void OnCurrencyChanged(CurrencyBalanceChangedEventData data)
        {
            if (!string.IsNullOrEmpty(PrimaryCurrencyId) && data.CurrencyId == PrimaryCurrencyId)
            {
                BalanceChipView?.SetBalance(data.NewBalance);
            }
        }

        private void OnCloseClicked()
        {
            _SceneLoaderService?.LoadMainMenu().Forget();
        }

        private void OnBottomAdPressed()
        {
            OnBottomAdClicked?.Invoke();
        }

        private void OnPowerUpAdPressed(PurchasableEntry _)
        {
            OnPowerUpAdClicked?.Invoke();
        }
    }
}
