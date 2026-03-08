using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Interfaces;
using Game.Store.Rewards;
using Game.Store.Services;
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

        private StoreCatalog _StoreCatalog;
        private IPurchaseService _PurchaseService;
        private KnifeSkinService _KnifeSkinService;
        private SceneLoaderService _SceneLoaderService;
        private PurchaseStartedEventChannel _PurchaseStartedChannel;
        private PurchaseCompletedEventChannel _PurchaseCompletedChannel;
        private PurchaseFailedEventChannel _PurchaseFailedChannel;
        private readonly List<SpawnedCardBinding> _KnifeSkinBindings = new List<SpawnedCardBinding>();
        private readonly List<SpawnedCardBinding> _PowerUpBindings = new List<SpawnedCardBinding>();

        [Inject]
        private void Construct(
            StoreCatalog storeCatalog,
            IPurchaseService purchaseService,
            KnifeSkinService knifeSkinService,
            SceneLoaderService sceneLoaderService,
            PurchaseStartedEventChannel purchaseStartedChannel,
            PurchaseCompletedEventChannel purchaseCompletedChannel,
            PurchaseFailedEventChannel purchaseFailedChannel)
        {
            _StoreCatalog = storeCatalog;
            _PurchaseService = purchaseService;
            _KnifeSkinService = knifeSkinService;
            _SceneLoaderService = sceneLoaderService;
            _PurchaseStartedChannel = purchaseStartedChannel;
            _PurchaseCompletedChannel = purchaseCompletedChannel;
            _PurchaseFailedChannel = purchaseFailedChannel;
        }

        private void Start()
        {
            BindCloseButton();
            BindCatalog();
            RefreshCardState();
        }

        private void OnEnable()
        {
            _PurchaseStartedChannel?.Subscribe(OnPurchaseStarted);
            _PurchaseCompletedChannel?.Subscribe(OnPurchaseCompleted);
            _PurchaseFailedChannel?.Subscribe(OnPurchaseFailed);
        }

        private void OnDisable()
        {
            _PurchaseStartedChannel?.Unsubscribe(OnPurchaseStarted);
            _PurchaseCompletedChannel?.Unsubscribe(OnPurchaseCompleted);
            _PurchaseFailedChannel?.Unsubscribe(OnPurchaseFailed);

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
            BindEntries(entries, KnifeSkinsContentRoot, KnifeSkinCardPrefab, _KnifeSkinBindings);
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
        }

        private void RefreshCardState()
        {
            var canInteract = _PurchaseService == null || !_PurchaseService.IsPurchaseInProgress;
            RefreshKnifeSkinStates(canInteract);
            SetBindingsInteractable(_PowerUpBindings, canInteract);
            if (CloseButton != null)
            {
                CloseButton.interactable = canInteract;
            }
        }

        private void OnBuyClicked(PurchasableEntry entry)
        {
            if (TryEquip(entry))
            {
                RefreshCardState();
                return;
            }

            TryPurchaseAsync(entry).Forget();
        }

        private async UniTaskVoid TryPurchaseAsync(PurchasableEntry entry)
        {
            if (_PurchaseService == null || entry == null)
            {
                return;
            }

            var result = await _PurchaseService.TryPurchaseAsync(entry);
            if (result.Success)
            {
                SyncKnifeSkinState();
            }
            RefreshCardState();
        }

        private void OnPurchaseStarted(PurchaseStartedEventData _)
        {
            RefreshCardState();
        }

        private void OnPurchaseCompleted(PurchaseCompletedEventData _)
        {
            SyncKnifeSkinState();
            RefreshCardState();
        }

        private void OnPurchaseFailed(PurchaseFailedEventData _)
        {
            RefreshCardState();
        }

        private void OnCloseClicked()
        {
            _SceneLoaderService?.LoadMainMenu().Forget();
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private void SyncKnifeSkinState()
        {
            _KnifeSkinService?.ReloadState();
        }

        private void RefreshKnifeSkinStates(bool canInteract)
        {
            string equippedId = _KnifeSkinService?.GetEquippedId() ?? string.Empty;

            for (var i = 0; i < _KnifeSkinBindings.Count; i++)
            {
                var binding = _KnifeSkinBindings[i];
                binding.IsOwned = IsOwned(binding.Entry);
                binding.IsEquipped = binding.IsOwned &&
                    TryGetSkinId(binding.Entry, out var skinId) &&
                    skinId == equippedId;

                if (binding.Card is KnifeSkinStoreCardView knifeSkinCard)
                {
                    knifeSkinCard.SetState(binding.IsOwned, binding.IsEquipped, canInteract);
                }
                else if (binding.Card != null)
                {
                    binding.Card.SetInteractable(canInteract && !binding.IsOwned);
                }
            }
        }

        private void SetBindingsInteractable(List<SpawnedCardBinding> bindings, bool canInteract)
        {
            for (var i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding.Card == null)
                {
                    continue;
                }

                binding.Card.SetInteractable(canInteract && !binding.IsOwned);
            }
        }

        private bool TryEquip(PurchasableEntry entry)
        {
            if (_KnifeSkinService == null)
            {
                return false;
            }

            if (!TryGetSkinId(entry, out var skinId))
            {
                return false;
            }

            if (!_KnifeSkinService.IsUnlocked(skinId))
            {
                return false;
            }

            if (_KnifeSkinService.IsEquipped(skinId))
            {
                return true;
            }

            return _KnifeSkinService.TryEquip(skinId);
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
            public bool IsOwned;
            public bool IsEquipped;
        }
    }
}
