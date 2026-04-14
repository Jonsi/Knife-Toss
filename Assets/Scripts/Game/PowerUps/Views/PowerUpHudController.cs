using System.Collections.Generic;
using Common.Events;
using Game.Flow.Events;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using Game.PowerUps.Events;
using Game.PowerUps.Interfaces;
using Game.Persistence.Services;
using Game.Store.Data;
using Game.Store.Rewards;
using Game.UI.Popups;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.PowerUps.Views
{
    /// <summary>
    /// Builds and manages HUD buttons for available power-ups.
    /// </summary>
    public class PowerUpHudController : MonoBehaviour
    {
        [SerializeField] private PowerUpCatalog _Catalog;
        [SerializeField] private PowerUpButtonView _ButtonPrefab;
        [SerializeField] private Transform _Container;
        [SerializeField] private PowerUpActivatedEventChannel _ActivatedChannel;
        [SerializeField] private PowerUpUsedEventChannel _UsedChannel;
        [SerializeField] private PowerUpCancelledEventChannel _CancelledChannel;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private VoidEventChannel _FailFlowStartedChannel;
        [SerializeField] private AddPowerUpPopup _AddPowerUpPopupPrefab;

        private readonly List<PowerUpButtonView> _Buttons = new List<PowerUpButtonView>();
        private IPowerUpService _PowerUpService;
        private PlayerDataService _PlayerDataService;
        private StoreCatalog _StoreCatalog;
        private IObjectResolver _Resolver;
        private AddPowerUpPopup _PopupInstance;

        [Inject]
        private void Construct(
            IPowerUpService powerUpService,
            PlayerDataService playerDataService,
            StoreCatalog storeCatalog,
            IObjectResolver resolver)
        {
            _PowerUpService = powerUpService;
            _PlayerDataService = playerDataService;
            _StoreCatalog = storeCatalog;
            _Resolver = resolver;
            RefreshCounts();
        }

        private void OnEnable()
        {
            BuildButtons();
            Subscribe();
            RefreshInventoryAndCounts();
            ClearSelection();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            if (_PopupInstance != null)
            {
                _PopupInstance.InventoryChanged -= OnPopupInventoryChanged;
            }
        }

        private void Subscribe()
        {
            _ActivatedChannel?.Subscribe(OnActivated);
            _UsedChannel?.Subscribe(OnUsed);
            _CancelledChannel?.Subscribe(OnCancelled);
            _LevelStartedChannel?.Subscribe(OnLevelStarted);
        }

        private void Unsubscribe()
        {
            _ActivatedChannel?.Unsubscribe(OnActivated);
            _UsedChannel?.Unsubscribe(OnUsed);
            _CancelledChannel?.Unsubscribe(OnCancelled);
            _LevelStartedChannel?.Unsubscribe(OnLevelStarted);
        }

        private void BuildButtons()
        {
            if (_Catalog == null || _ButtonPrefab == null || _Container == null)
                return;

            if (_Buttons.Count > 0)
                return;

            for (int i = 0; i < _Catalog.Entries.Count; i++)
            {
                var config = _Catalog.Entries[i];
                if (config == null)
                    continue;

                var view = Instantiate(_ButtonPrefab, _Container);
                view.Initialize(
                    config,
                    OnButtonClicked,
                    _AddPowerUpPopupPrefab != null ? OnAddButtonClicked : null,
                    _LevelStartedChannel,
                    _FailFlowStartedChannel);
                _Buttons.Add(view);
            }
        }

        private void OnButtonClicked(PowerUpConfigBase config)
        {
            if (_PowerUpService == null)
                return;

            if (_PowerUpService.ActiveConfig == config)
            {
                _PowerUpService.TryCancelActive();
                return;
            }

            if (_PowerUpService.TryActivate(config))
            {
                Select(config);
            }
        }

        private void OnAddButtonClicked(PowerUpConfigBase config)
        {
            if (_AddPowerUpPopupPrefab == null)
                return;

            var popup = GetOrCreatePopup();
            popup?.ShowFor(config);
        }

        private void RefreshCounts()
        {
            if (_PlayerDataService == null)
                return;

            var inventory = _PlayerDataService.EnsurePowerUpInventory();
            for (int i = 0; i < _Buttons.Count; i++)
            {
                var button = _Buttons[i];
                if (button == null || button.Config == null)
                    continue;

                int count = GetCount(inventory, button.Config);
                button.SetCount(count);
            }
        }

        private static int GetCount(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            if (config == null || string.IsNullOrEmpty(config.InventoryKey))
                return 0;

            return inventory.GetCount(config.InventoryKey);
        }

        private void OnActivated(PowerUpActivatedEventData data)
        {
            Select(data.Config);
            RefreshCounts();
        }

        private void OnUsed(PowerUpUsedEventData data)
        {
            RefreshCounts();
            ClearSelection();
        }

        private void OnCancelled(PowerUpCancelledEventData data)
        {
            ClearSelection();
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            ClearSelection();
            RefreshInventoryAndCounts();
        }

        private void Select(PowerUpConfigBase config)
        {
            if (config == null)
            {
                ClearSelection();
                return;
            }

            for (int i = 0; i < _Buttons.Count; i++)
            {
                var button = _Buttons[i];
                if (button == null)
                    continue;

                bool isSelected = button.Config == config;
                button.UpdateSelected(isSelected);
            }
        }

        private void ClearSelection()
        {
            for (int i = 0; i < _Buttons.Count; i++)
            {
                var button = _Buttons[i];
                if (button == null)
                    continue;

                button.UpdateSelected(false);
            }
        }

        private void RefreshInventoryAndCounts()
        {
            if (_PlayerDataService != null)
            {
                _PlayerDataService.ReloadPowerUpInventory();
            }

            RefreshCounts();
        }

        private bool CanOpenAddPopup(PowerUpConfigBase config)
        {
            return _AddPowerUpPopupPrefab != null;
        }

        private PurchasableEntry FindStoreEntry(PowerUpConfigBase config)
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

        private AddPowerUpPopup GetOrCreatePopup()
        {
            if (_PopupInstance != null)
                return _PopupInstance;

            if (_AddPowerUpPopupPrefab == null || _Resolver == null)
                return null;

            var parent = GetComponentInParent<Canvas>()?.transform;
            _PopupInstance = parent != null
                ? _Resolver.Instantiate(_AddPowerUpPopupPrefab, parent)
                : _Resolver.Instantiate(_AddPowerUpPopupPrefab);
            _PopupInstance.InventoryChanged += OnPopupInventoryChanged;
            _PopupInstance.gameObject.SetActive(false);
            return _PopupInstance;
        }

        private void OnPopupInventoryChanged()
        {
            RefreshInventoryAndCounts();
        }
    }
}
