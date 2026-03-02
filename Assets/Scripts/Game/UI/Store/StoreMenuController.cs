using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Persistence.Services;
using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Interfaces;
using Game.Store.Rewards;
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
        private PlayerDataService _PlayerDataService;
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
            PlayerDataService playerDataService,
            SceneLoaderService sceneLoaderService,
            PurchaseStartedEventChannel purchaseStartedChannel,
            PurchaseCompletedEventChannel purchaseCompletedChannel,
            PurchaseFailedEventChannel purchaseFailedChannel)
        {
            _StoreCatalog = storeCatalog;
            _PurchaseService = purchaseService;
            _PlayerDataService = playerDataService;
            _SceneLoaderService = sceneLoaderService;
            _PurchaseStartedChannel = purchaseStartedChannel;
            _PurchaseCompletedChannel = purchaseCompletedChannel;
            _PurchaseFailedChannel = purchaseFailedChannel;
        }

        private void Start()
        {
            BindCloseButton();
            BindCatalog();
            RefreshInteractableState();
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

            RefreshOwnedState();
        }

        private void RefreshInteractableState()
        {
            var canInteract = _PurchaseService == null || !_PurchaseService.IsPurchaseInProgress;
            SetBindingsInteractable(_KnifeSkinBindings, canInteract);
            SetBindingsInteractable(_PowerUpBindings, canInteract);
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
            RefreshInteractableState();
        }

        private void OnPurchaseCompleted(PurchaseCompletedEventData _)
        {
            RefreshOwnedState();
            RefreshInteractableState();
        }

        private void OnPurchaseFailed(PurchaseFailedEventData _)
        {
            RefreshInteractableState();
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

        private void RefreshOwnedState()
        {
            for (var i = 0; i < _KnifeSkinBindings.Count; i++)
            {
                var binding = _KnifeSkinBindings[i];
                binding.IsOwned = IsOwned(binding.Entry);
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

        private bool IsOwned(PurchasableEntry entry)
        {
            if (entry?.Reward is KnifeSkinReward knifeSkinReward &&
                knifeSkinReward.Config != null &&
                !string.IsNullOrEmpty(knifeSkinReward.Config.Id))
            {
                return _PlayerDataService != null &&
                    _PlayerDataService.EnsureKnifeSkinCollection().IsUnlocked(knifeSkinReward.Config.Id);
            }

            return false;
        }

        private sealed class SpawnedCardBinding
        {
            public StoreCardView Card;
            public PurchasableEntry Entry;
            public bool IsOwned;
        }
    }
}
