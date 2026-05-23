using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    /// Controller for the Store scene. Binds catalog data to UI and drives purchases.
    /// </summary>
    public class StoreMenuController : MonoBehaviour
    {
        [Header("Store UI")]
        [field: SerializeField] public KnifeSkinStoreCardView KnifeSkinCardPrefab { get; private set; }
        [field: SerializeField] public Transform KnifeSkinsContentRoot { get; private set; }
        [field: SerializeField] public PowerUpStoreCardView PowerUpCardPrefab { get; private set; }
        [field: SerializeField] public Transform PowerUpsContentRoot { get; private set; }
        [field: SerializeField] public Button CloseButton { get; private set; }
        [SerializeField] private StoreBalanceChipView _StoreBalanceChipView;
        [SerializeField] private UiElementAnimator[] _StoreAnimators;

        private StoreCatalog _StoreCatalog;
        private IStorePurchaseRouter _PurchaseRouter;
        private KnifeSkinService _KnifeSkinService;
        private SceneLoaderService _SceneLoaderService;
        private PlayerDataService _PlayerDataService;
        private readonly List<SpawnedCardBinding> _KnifeSkinBindings = new List<SpawnedCardBinding>();
        private readonly List<SpawnedCardBinding> _PowerUpBindings = new List<SpawnedCardBinding>();
        private readonly List<UiElementAnimator> _RuntimeAnimators = new List<UiElementAnimator>();
        private bool _IsClosing;

        [Inject]
        private void Construct(
            StoreCatalog storeCatalog,
            IStorePurchaseRouter purchaseRouter,
            KnifeSkinService knifeSkinService,
            SceneLoaderService sceneLoaderService,
            PlayerDataService playerDataService)
        {
            _StoreCatalog = storeCatalog;
            _PurchaseRouter = purchaseRouter;
            _KnifeSkinService = knifeSkinService;
            _SceneLoaderService = sceneLoaderService;
            _PlayerDataService = playerDataService;
        }

        private void Start()
        {
            BindCloseButton();
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

        private void BindCatalog()
        {
            BindKnifeSkinEntries(_StoreCatalog?.Skins);
            BindPowerUpEntries(_StoreCatalog?.PowerUps);
        }

        private void BindKnifeSkinEntries(IReadOnlyList<PurchasableEntry> entries)
        {
            _KnifeSkinBindings.Clear();

            if (KnifeSkinsContentRoot == null || KnifeSkinCardPrefab == null)
            {
                return;
            }

            ClearChildren(KnifeSkinsContentRoot);

            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || IsOwned(entry))
                {
                    continue;
                }

                var card = Instantiate(KnifeSkinCardPrefab, KnifeSkinsContentRoot);
                card.Setup(entry, OnBuyClicked);
                _KnifeSkinBindings.Add(new SpawnedCardBinding
                {
                    Card = card,
                    Entry = entry
                });
            }

            RefreshScrollableContentLayout(KnifeSkinsContentRoot);
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

            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                var card = Instantiate(prefab, parent);
                card.Setup(entry, OnBuyClicked);
                bindings.Add(new SpawnedCardBinding
                {
                    Card = card,
                    Entry = entry
                });
            }

            RefreshScrollableContentLayout(parent);
        }

        private void RefreshCardState()
        {
            var canInteract = !_IsClosing && (_PurchaseRouter == null || !_PurchaseRouter.IsPurchaseInProgress);
            RefreshKnifeSkinStates(canInteract);
            RefreshPowerUpStates(canInteract);
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
            if (_PurchaseRouter == null || entry == null)
            {
                return;
            }

            RefreshCardState();
            var result = await _PurchaseRouter.TryPurchaseAsync(entry);
            if (result.Success)
            {
                SyncKnifeSkinState();
                await HandleSuccessfulPurchaseAsync(entry);
            }

            RefreshCardState();
        }

        private void OnCloseClicked()
        {
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
            UiElementAnimator[] animators = GetStoreAnimators();
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i]?.PlayIn();
            }
        }

        private async UniTask PlayStoreOutAsync()
        {
            UiElementAnimator[] animators = GetStoreAnimators();
            if (animators.Length == 0)
            {
                return;
            }

            var tasks = new List<UniTask>();
            for (int i = 0; i < animators.Length; i++)
            {
                UiElementAnimator animator = animators[i];
                if (animator != null)
                {
                    tasks.Add(animator.PlayOutAsync());
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

            Transform root = transform;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                UiElementAnimator animator = child.GetComponent<UiElementAnimator>();
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

            if (contentRect.parent is RectTransform viewportRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRect);
            }

            Canvas.ForceUpdateCanvases();
        }

        private void SyncKnifeSkinState()
        {
            _KnifeSkinService?.ReloadState();
        }

        private void RefreshKnifeSkinStates(bool canInteract)
        {
            for (var i = 0; i < _KnifeSkinBindings.Count; i++)
            {
                var binding = _KnifeSkinBindings[i];
                if (binding.Card == null || binding.IsPurchased)
                {
                    continue;
                }

                if (binding.Card is KnifeSkinStoreCardView knifeSkinCard)
                {
                    knifeSkinCard.SetAvailableState(CanPressSoftPurchaseButton(binding.Entry, canInteract));
                }
                else
                {
                    binding.Card.SetInteractable(CanPressSoftPurchaseButton(binding.Entry, canInteract));
                }
            }
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

                binding.Card.SetInteractable(CanPressSoftPurchaseButton(binding.Entry, canInteract));

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

        private bool CanPressSoftPurchaseButton(PurchasableEntry entry, bool canInteract)
        {
            return canInteract && entry != null && _PurchaseRouter != null;
        }

        private void PlayRewardAccumulation(PurchasableEntry entry)
        {
            if (_StoreBalanceChipView == null || entry?.Reward is not CurrencyReward currencyReward || currencyReward.Amount <= 0)
            {
                return;
            }

            var source = FindCardForEntry(entry)?.BuyButton?.transform as RectTransform;
            _StoreBalanceChipView.PlayAccumulationFromUiElement(source, currencyReward.Amount);
        }

        private StoreCardView FindCardForEntry(PurchasableEntry entry)
        {
            for (var i = 0; i < _KnifeSkinBindings.Count; i++)
            {
                if (_KnifeSkinBindings[i].Entry == entry)
                {
                    return _KnifeSkinBindings[i].Card;
                }
            }

            for (var i = 0; i < _PowerUpBindings.Count; i++)
            {
                if (_PowerUpBindings[i].Entry == entry)
                {
                    return _PowerUpBindings[i].Card;
                }
            }

            return null;
        }

        private async UniTask HandleSuccessfulPurchaseAsync(PurchasableEntry entry)
        {
            if (TryGetKnifeSkinBinding(entry, out var binding))
            {
                binding.IsPurchased = true;
                binding.Card?.SetInteractable(false);

                if (binding.Card is KnifeSkinStoreCardView knifeSkinCard)
                {
                    await knifeSkinCard.PlayPurchasedOutAsync();
                }
                else if (binding.Card != null)
                {
                    binding.Card.gameObject.SetActive(false);
                }

                RefreshScrollableContentLayout(KnifeSkinsContentRoot);
                return;
            }

            PlayRewardAccumulation(entry);
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

        private bool TryGetKnifeSkinBinding(PurchasableEntry entry, out SpawnedCardBinding binding)
        {
            for (var i = 0; i < _KnifeSkinBindings.Count; i++)
            {
                binding = _KnifeSkinBindings[i];
                if (binding.Entry == entry)
                {
                    return true;
                }
            }

            binding = null;
            return false;
        }

        private sealed class SpawnedCardBinding
        {
            public StoreCardView Card;
            public PurchasableEntry Entry;
            public bool IsPurchased;
        }
    }
}
