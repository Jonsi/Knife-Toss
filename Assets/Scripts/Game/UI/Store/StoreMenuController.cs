using System.Collections.Generic;
using Common.IAP.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Feedback.Events;
using Game.Flow.Game;
using Game.Persistence.Services;
using Game.Store.Data;
using Game.Store.Interfaces;
using Game.Store.Rewards;
using Game.Store.Services;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Store
{
    /// <summary>
    /// Controller for the Store scene. Binds catalog data to authored Store presentation and
    /// preserves the existing purchase, entitlement, inventory, and equip authorities.
    /// </summary>
    public class StoreMenuController : MonoBehaviour
    {
        [Header("Store UI")]
        [field: SerializeField] public KnifeSkinStoreCardView KnifeSkinCardPrefab { get; private set; }
        [field: SerializeField] public Transform KnifeSkinsContentRoot { get; private set; }
        [field: SerializeField] public PowerUpStoreCardView PowerUpCardPrefab { get; private set; }
        [field: SerializeField] public Transform PowerUpsContentRoot { get; private set; }
        [field: SerializeField] public Button CloseButton { get; private set; }
        [SerializeField] private StoreKnifeCarouselView _KnifeCarouselView;
        [SerializeField] private StoreBalanceChipView _StoreBalanceChipView;
        [SerializeField] private StoreAdCtaView _StoreAdCtaView;
        [SerializeField] private StoreRemoveAdsView _RemoveAdsView;
        [SerializeField] private UiElementAnimator[] _StoreAnimators;

        private StoreCatalog _StoreCatalog;
        private IStorePurchaseRouter _PurchaseRouter;
        private KnifeSkinService _KnifeSkinService;
        private SceneLoaderService _SceneLoaderService;
        private PlayerDataService _PlayerDataService;
        private FeedbackEventChannel _FeedbackEventChannel;
        private IIapService _IapService;
        private ActionButtonPresenter _CloseButtonPresenter;
        private readonly List<SpawnedCardBinding> _PowerUpBindings = new();
        private readonly List<UiElementAnimator> _RuntimeAnimators = new();
        private PurchasableEntry _RemoveAdsEntry;
        private bool _IsClosing;
        private bool _IsSubmitting;

        [Inject]
        private void Construct(
            StoreCatalog storeCatalog,
            IStorePurchaseRouter purchaseRouter,
            KnifeSkinService knifeSkinService,
            SceneLoaderService sceneLoaderService,
            PlayerDataService playerDataService,
            FeedbackEventChannel feedbackEventChannel,
            IIapService iapService)
        {
            _StoreCatalog = storeCatalog;
            _PurchaseRouter = purchaseRouter;
            _KnifeSkinService = knifeSkinService;
            _SceneLoaderService = sceneLoaderService;
            _PlayerDataService = playerDataService;
            _FeedbackEventChannel = feedbackEventChannel;
            _IapService = iapService;
        }

        private void Start()
        {
            BindCloseButton();
            BindAdSubmissionState();
            BindCatalog();
            RefreshCardState();
            PlayStoreIn();
        }

        private void OnDisable()
        {
            if (CloseButton != null)
            {
                CloseButton.onClick.RemoveListener(OnCloseClicked);
            }

            if (_StoreAdCtaView != null)
            {
                _StoreAdCtaView.SubmissionStateChanged -= OnAdSubmissionStateChanged;
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
            _CloseButtonPresenter = ActionButtonPresenter.GetOrAdd(CloseButton, _FeedbackEventChannel);
        }

        private void BindAdSubmissionState()
        {
            if (_StoreAdCtaView == null)
            {
                return;
            }

            _StoreAdCtaView.SubmissionStateChanged -= OnAdSubmissionStateChanged;
            _StoreAdCtaView.SubmissionStateChanged += OnAdSubmissionStateChanged;
        }

        private void BindCatalog()
        {
            BindKnifeSkinEntries(_StoreCatalog?.Skins);
            BindPowerUpEntries(_StoreCatalog?.PowerUps);
            BindRemoveAdsEntry(_StoreCatalog?.IapEntries);
        }

        private void BindKnifeSkinEntries(IReadOnlyList<PurchasableEntry> entries)
        {
            if (_KnifeCarouselView == null || KnifeSkinCardPrefab == null)
            {
                return;
            }

            var knifeEntries = new List<PurchasableEntry>();

            if (entries != null)
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry?.Reward is not KnifeSkinReward)
                    {
                        continue;
                    }

                    knifeEntries.Add(entry);
                }
            }

            _KnifeCarouselView.Configure(
                knifeEntries,
                KnifeSkinCardPrefab,
                OnOfferPressed,
                ApplyKnifeSkinCardState,
                _FeedbackEventChannel);
        }

        private void BindPowerUpEntries(IReadOnlyList<PurchasableEntry> entries)
        {
            BindEntries(entries, PowerUpsContentRoot, PowerUpCardPrefab, _PowerUpBindings);
        }

        private void BindEntries<TCard>(
            IReadOnlyList<PurchasableEntry> entries,
            Transform parent,
            TCard prefab,
            List<SpawnedCardBinding> bindings)
            where TCard : StoreCardView
        {
            bindings.Clear();

            if (parent == null || prefab == null)
            {
                return;
            }

            ClearChildren(parent);

            if (entries != null)
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    var card = Instantiate(prefab, parent);
                    card.name = $"PowerUpCard_{i + 1}";
                    card.Setup(entry, OnOfferPressed, _FeedbackEventChannel);
                    bindings.Add(new SpawnedCardBinding
                    {
                        Card = card,
                        Entry = entry
                    });
                }
            }

            RefreshScrollableContentLayout(parent);
        }

        private void BindRemoveAdsEntry(IReadOnlyList<PurchasableEntry> entries)
        {
            _RemoveAdsEntry = null;
            if (entries != null)
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i]?.Reward is RemoveAdsReward)
                    {
                        _RemoveAdsEntry = entries[i];
                        break;
                    }
                }
            }

            _RemoveAdsView?.Setup(_RemoveAdsEntry, OnOfferPressed, _FeedbackEventChannel);
        }

        private void RefreshCardState()
        {
            var canInteract = !_IsClosing &&
                              !_IsSubmitting &&
                              (_StoreAdCtaView == null || !_StoreAdCtaView.IsSubmissionInProgress) &&
                              (_PurchaseRouter == null || !_PurchaseRouter.IsPurchaseInProgress);

            RefreshKnifeSkinStates(canInteract);
            RefreshPowerUpStates(canInteract);
            RefreshRemoveAdsState(canInteract);

            if (CloseButton != null)
            {
                CloseButton.interactable = canInteract;
            }
        }

        private void OnOfferPressed(PurchasableEntry entry)
        {
            TrySubmitOfferAsync(entry).Forget();
        }

        private async UniTaskVoid TrySubmitOfferAsync(PurchasableEntry entry)
        {
            if (_PurchaseRouter == null || entry == null || _IsSubmitting || _IsClosing)
            {
                return;
            }

            if (TryGetSkinId(entry, out var skinId) && IsOwned(entry))
            {
                var equipped = _KnifeSkinService != null && _KnifeSkinService.TryEquip(skinId);
                FindCardForEntry(entry)?.ResolveSubmission(
                    equipped ? PurchaseResult.Succeeded : PurchaseResult.Error("Unable to equip item."));
                RefreshCardState();
                return;
            }

            _IsSubmitting = true;
            RefreshCardState();

            try
            {
                var result = await _PurchaseRouter.TryPurchaseAsync(entry);
                ResolveSubmission(entry, result);
                if (result.Success)
                {
                    SyncKnifeSkinState();
                    await HandleSuccessfulPurchaseAsync(entry);
                }
            }
            finally
            {
                _IsSubmitting = false;
                RefreshCardState();
            }
        }

        private void ResolveSubmission(PurchasableEntry entry, PurchaseResult result)
        {
            if (ReferenceEquals(entry, _RemoveAdsEntry))
            {
                _RemoveAdsView?.ResolveSubmission(result);
                return;
            }

            FindCardForEntry(entry)?.ResolveSubmission(result);
        }

        private void OnAdSubmissionStateChanged(bool _)
        {
            RefreshCardState();
        }

        private void OnCloseClicked()
        {
            if (_CloseButtonPresenter != null && !_CloseButtonPresenter.TryBeginSubmission())
            {
                return;
            }

            CloseStoreAsync().Forget();
        }

        private async UniTaskVoid CloseStoreAsync()
        {
            if (_IsClosing)
            {
                return;
            }

            _IsClosing = true;
            RefreshCardState();

            await PlayStoreOutAsync();
            if (_SceneLoaderService != null)
            {
                await _SceneLoaderService.UnloadStore();
            }
        }

        private void PlayStoreIn()
        {
            var animators = GetStoreAnimators();
            for (var i = 0; i < animators.Length; i++)
            {
                animators[i]?.PlayIn();
            }
        }

        private async UniTask PlayStoreOutAsync()
        {
            var animators = GetStoreAnimators();
            if (animators.Length == 0)
            {
                return;
            }

            var tasks = new List<UniTask>();
            for (var i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null)
                {
                    tasks.Add(animators[i].PlayOutAsync());
                }
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        private UiElementAnimator[] GetStoreAnimators()
        {
            if (_StoreAnimators != null && _StoreAnimators.Length > 0)
            {
                return _StoreAnimators;
            }

            if (_RuntimeAnimators.Count > 0)
            {
                return _RuntimeAnimators.ToArray();
            }

            var root = transform;
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                var animator = child.GetComponent<UiElementAnimator>();
                if (animator == null)
                {
                    animator = child.gameObject.AddComponent<UiElementAnimator>();
                }

                _RuntimeAnimators.Add(animator);
            }

            return _RuntimeAnimators.ToArray();
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private static void RefreshScrollableContentLayout(Transform parent)
        {
            if (parent is not RectTransform contentRect)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            var preferredWidth = LayoutUtility.GetPreferredWidth(contentRect);
            if (preferredWidth > 0f)
            {
                contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            Canvas.ForceUpdateCanvases();
        }

        private void SyncKnifeSkinState()
        {
            _KnifeSkinService?.ReloadState();
        }

        private void RefreshKnifeSkinStates(bool canInteract)
        {
            _KnifeCarouselView?.SetInteractable(canInteract);
        }

        private void ApplyKnifeSkinCardState(KnifeSkinStoreCardView card, PurchasableEntry entry)
        {
            if (card == null || entry == null)
            {
                return;
            }

            var canInteract = !_IsClosing &&
                              !_IsSubmitting &&
                              (_StoreAdCtaView == null || !_StoreAdCtaView.IsSubmissionInProgress) &&
                              (_PurchaseRouter == null || !_PurchaseRouter.IsPurchaseInProgress);
            var equippedId = _KnifeSkinService?.GetEquippedId() ?? string.Empty;
            var owned = IsOwned(entry);
            var equipped = TryGetSkinId(entry, out var skinId) && skinId == equippedId;
            card.SetStoreState(owned, equipped, CanSubmitEntry(entry, canInteract));
        }

        private void RefreshPowerUpStates(bool canInteract)
        {
            var inventory = _PlayerDataService?.EnsurePowerUpInventory();

            for (var i = 0; i < _PowerUpBindings.Count; i++)
            {
                var binding = _PowerUpBindings[i];
                if (binding.Card == null)
                {
                    continue;
                }

                binding.Card.SetInteractable(CanSubmitEntry(binding.Entry, canInteract));

                if (binding.Card is not PowerUpStoreCardView powerUpCard ||
                    binding.Entry?.Reward is not PowerUpReward powerUpReward ||
                    powerUpReward.Config == null ||
                    string.IsNullOrEmpty(powerUpReward.Config.InventoryKey))
                {
                    continue;
                }

                powerUpCard.SetCurrentAmount(inventory?.GetCount(powerUpReward.Config.InventoryKey) ?? 0);
            }
        }

        private void RefreshRemoveAdsState(bool canInteract)
        {
            if (_RemoveAdsView == null)
            {
                return;
            }

            var product = _RemoveAdsEntry?.GetIapProduct();
            var available = _RemoveAdsEntry != null && product != null;
            var owned = available && _IapService != null && _IapService.IsOwned(product);
            _RemoveAdsView.SetState(
                available,
                owned,
                available && _IapService != null && CanSubmitEntry(_RemoveAdsEntry, canInteract));
        }

        private bool CanSubmitEntry(PurchasableEntry entry, bool canInteract)
        {
            return canInteract && entry != null && _PurchaseRouter != null;
        }

        private void PlayRewardAccumulation(PurchasableEntry entry)
        {
            if (_StoreBalanceChipView == null ||
                entry?.Reward is not CurrencyReward currencyReward ||
                currencyReward.Amount <= 0)
            {
                return;
            }

            var source = FindCardForEntry(entry)?.BuyButton?.transform as RectTransform;
            _StoreBalanceChipView.PlayAccumulationFromUiElement(source, currencyReward.Amount);
        }

        private StoreCardView FindCardForEntry(PurchasableEntry entry)
        {
            var knifeCard = _KnifeCarouselView?.FindVisibleCard(entry);
            if (knifeCard != null)
            {
                return knifeCard;
            }

            for (var i = 0; i < _PowerUpBindings.Count; i++)
            {
                if (ReferenceEquals(_PowerUpBindings[i].Entry, entry))
                {
                    return _PowerUpBindings[i].Card;
                }
            }

            return null;
        }

        private async UniTask HandleSuccessfulPurchaseAsync(PurchasableEntry entry)
        {
            PlayRewardAccumulation(entry);
            await UniTask.Yield();
        }

        private bool IsOwned(PurchasableEntry entry)
        {
            return _KnifeSkinService != null &&
                   TryGetSkinId(entry, out var skinId) &&
                   _KnifeSkinService.IsUnlocked(skinId);
        }

        private static bool TryGetSkinId(PurchasableEntry entry, out string skinId)
        {
            skinId = (entry?.Reward as KnifeSkinReward)?.Config?.Id;
            return !string.IsNullOrEmpty(skinId);
        }

        private sealed class SpawnedCardBinding
        {
            public StoreCardView Card;
            public PurchasableEntry Entry;
        }
    }
}
