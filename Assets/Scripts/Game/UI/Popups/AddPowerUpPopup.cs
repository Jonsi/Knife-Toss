using System;
using System.Collections.Generic;
using Common.Ads.Interfaces;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Ads.Data;
using Game.Ads.Extensions;
using Game.Feedback.Events;
using Game.Persistence.Services;
using Game.PowerUps.Configs;
using Game.Store.Data;
using Game.Store.Interfaces;
using Game.Store.Rewards;
using Game.UI.Common;
using Game.UI.HUD;
using Game.UI.Store;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Popups
{
    /// <summary>
    /// Modal for replenishing power-ups from the gameplay HUD.
    /// </summary>
    public class AddPowerUpPopup : BasePopup
    {
        [SerializeField] private Button _CloseButton;
        [SerializeField] private StoreAdCtaView _AdCtaView;
        [SerializeField] private RewardedPlacement _AdPlacement;
        [SerializeField] private PowerUpStoreCardView[] _PowerUpCards;

        private StoreCatalog _StoreCatalog;
        private IStorePurchaseRouter _PurchaseRouter;
        private PlayerDataService _PlayerDataService;
        private IAdService _AdService;
        private IRewardGrantContext _GrantContext;
        private CoinsUiDisplay _CoinsUiDisplay;
        private FeedbackEventChannel _FeedbackEventChannel;
        private PowerUpConfigBase _RequestedConfig;
        private readonly List<PurchasableEntry> _VisibleEntries = new List<PurchasableEntry>();

        public event Action InventoryChanged;

        [Inject]
        private void Construct(
            StoreCatalog storeCatalog,
            IStorePurchaseRouter purchaseRouter,
            PlayerDataService playerDataService,
            IAdService adService,
            IRewardGrantContext grantContext,
            CoinsUiDisplay coinsUiDisplay,
            FeedbackEventChannel feedbackEventChannel)
        {
            _StoreCatalog = storeCatalog;
            _PurchaseRouter = purchaseRouter;
            _PlayerDataService = playerDataService;
            _AdService = adService;
            _GrantContext = grantContext;
            _CoinsUiDisplay = coinsUiDisplay;
            _FeedbackEventChannel = feedbackEventChannel;
        }

        private void Awake()
        {
            _CloseButton ??= FindButton("CloseButton");
            _AdCtaView ??= GetComponentInChildren<StoreAdCtaView>(true);
            if (_PowerUpCards == null || _PowerUpCards.Length == 0)
            {
                _PowerUpCards = GetComponentsInChildren<PowerUpStoreCardView>(true);
            }
        }

        private void OnEnable()
        {
            ActionButtonPresenter.GetOrAdd(_CloseButton, _FeedbackEventChannel);
            if (_CloseButton != null)
            {
                _CloseButton.onClick.RemoveListener(OnCloseClicked);
                _CloseButton.onClick.AddListener(OnCloseClicked);
            }

            RefreshContent();
        }

        private void OnDisable()
        {
            if (_CloseButton != null)
            {
                _CloseButton.onClick.RemoveListener(OnCloseClicked);
            }
        }

        public void ShowFor(PowerUpConfigBase config)
        {
            _RequestedConfig = config;
            RefreshContent();
            Show();
        }

        private void RefreshContent()
        {
            BindCards();
            BindAdCta();
            RefreshCardState();
        }

        private void BindCards()
        {
            _VisibleEntries.Clear();

            if (_PowerUpCards == null || _PowerUpCards.Length == 0)
                return;

            var orderedEntries = BuildOrderedEntries();
            for (int i = 0; i < _PowerUpCards.Length; i++)
            {
                var card = _PowerUpCards[i];
                if (card == null)
                {
                    continue;
                }

                if (i >= orderedEntries.Count)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }

                var entry = orderedEntries[i];
                _VisibleEntries.Add(entry);
                card.gameObject.SetActive(true);
                card.Setup(entry, OnBuyClicked, _FeedbackEventChannel);
                card.SetCurrentAmount(GetCurrentAmount(entry));
            }
        }

        private List<PurchasableEntry> BuildOrderedEntries()
        {
            var orderedEntries = new List<PurchasableEntry>();
            var entries = _StoreCatalog?.PowerUps;
            if (entries == null)
                return orderedEntries;

            var requestedEntry = FindEntry(_RequestedConfig);
            if (requestedEntry != null)
            {
                orderedEntries.Add(requestedEntry);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || orderedEntries.Contains(entry))
                {
                    continue;
                }

                orderedEntries.Add(entry);
            }

            return orderedEntries;
        }

        private void BindAdCta()
        {
            if (_AdCtaView == null)
                return;

            _AdCtaView.Setup(
                _AdPlacement?.Reward?.GetDisplayText() ?? string.Empty,
                OnWatchAdClicked,
                _FeedbackEventChannel);
            _AdCtaView.SetInteractable(_AdPlacement != null && _AdService != null && _AdService.CanShowAd(_AdPlacement));
        }

        private void RefreshCardState()
        {
            var canInteract = _PurchaseRouter == null || !_PurchaseRouter.IsPurchaseInProgress;

            if (_PowerUpCards != null)
            {
                for (int i = 0; i < _PowerUpCards.Length; i++)
                {
                    var card = _PowerUpCards[i];
                    if (card == null || !card.gameObject.activeSelf)
                    {
                        continue;
                    }

                    var entry = i < _VisibleEntries.Count ? _VisibleEntries[i] : null;
                    card.SetInteractable(canInteract && entry != null && _PurchaseRouter != null);
                    card.SetCurrentAmount(GetCurrentAmount(entry));
                }
            }

            if (_CloseButton != null)
            {
                _CloseButton.interactable = canInteract;
            }
        }

        private void OnBuyClicked(PurchasableEntry entry)
        {
            TryPurchaseAsync(entry).Forget();
        }

        private async UniTaskVoid TryPurchaseAsync(PurchasableEntry entry)
        {
            if (_PurchaseRouter == null || entry == null)
                return;

            RefreshCardState();

            var result = await _PurchaseRouter.TryPurchaseAsync(entry);
            FindCard(entry)?.ResolveSubmission(result);
            if (result.Success)
            {
                InventoryChanged?.Invoke();
            }

            RefreshCardState();
        }

        private async void OnWatchAdClicked()
        {
            if (_AdService == null || _AdPlacement == null || _AdCtaView == null)
                return;

            _AdCtaView.SetInteractable(false);

            var result = await _AdService.ShowRewardedAsync(_AdPlacement);
            if (result.Success)
            {
                await _AdPlacement.TryGrantRewardAsync(_GrantContext);
                PlayRewardAccumulation();
                InventoryChanged?.Invoke();
            }

            _AdCtaView.ResolveSubmission(result.Success);

            RefreshCardState();
            _AdCtaView.SetInteractable(_AdService.CanShowAd(_AdPlacement));
        }

        private void OnCloseClicked()
        {
            Hide();
        }

        private PurchasableEntry FindEntry(PowerUpConfigBase config)
        {
            if (config == null || _StoreCatalog?.PowerUps == null)
                return null;

            for (int i = 0; i < _StoreCatalog.PowerUps.Count; i++)
            {
                var entry = _StoreCatalog.PowerUps[i];
                if (entry?.Reward is PowerUpReward powerUpReward && powerUpReward.Config == config)
                {
                    return entry;
                }
            }

            return null;
        }

        private int GetCurrentAmount(PurchasableEntry entry)
        {
            if (entry?.Reward is not PowerUpReward powerUpReward ||
                powerUpReward.Config == null ||
                string.IsNullOrEmpty(powerUpReward.Config.InventoryKey))
            {
                return 0;
            }

            var inventory = _PlayerDataService?.EnsurePowerUpInventory();
            return inventory?.GetCount(powerUpReward.Config.InventoryKey) ?? 0;
        }

        private StoreCardView FindCard(PurchasableEntry entry)
        {
            if (_PowerUpCards == null)
                return null;

            for (int i = 0; i < _PowerUpCards.Length && i < _VisibleEntries.Count; i++)
            {
                if (_VisibleEntries[i] == entry)
                {
                    return _PowerUpCards[i];
                }
            }

            return null;
        }

        private void PlayRewardAccumulation()
        {
            if (_CoinsUiDisplay == null || _AdPlacement?.Reward is not CurrencyReward currencyReward || currencyReward.Amount <= 0)
                return;

            var source = _AdCtaView.ActionButton != null ? _AdCtaView.ActionButton.transform as RectTransform : null;
            _CoinsUiDisplay.PlayAccumulationFromUiElement(source, currencyReward.Amount);
        }

        private Button FindButton(string objectName)
        {
            var buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].gameObject.name == objectName)
                {
                    return buttons[i];
                }
            }

            return null;
        }
    }
}
