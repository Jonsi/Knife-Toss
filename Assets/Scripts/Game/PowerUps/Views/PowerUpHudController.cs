using System.Collections.Generic;
using Common.Events;
using Game.Flow.Events;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using Game.PowerUps.Events;
using Game.PowerUps.Interfaces;
using Game.Persistence.Services;
using UnityEngine;
using VContainer;

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
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;

        private readonly List<PowerUpButtonView> _Buttons = new List<PowerUpButtonView>();
        private IPowerUpService _PowerUpService;
        private PlayerDataService _PlayerDataService;

        [Inject]
        private void Construct(IPowerUpService powerUpService, PlayerDataService playerDataService)
        {
            _PowerUpService = powerUpService;
            _PlayerDataService = playerDataService;
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

        private void Subscribe()
        {
            _ActivatedChannel?.Subscribe(OnActivated);
            _UsedChannel?.Subscribe(OnUsed);
            _LevelStartedChannel?.Subscribe(OnLevelStarted);
        }

        private void Unsubscribe()
        {
            _ActivatedChannel?.Unsubscribe(OnActivated);
            _UsedChannel?.Unsubscribe(OnUsed);
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
                view.Initialize(config, OnButtonClicked);
                _Buttons.Add(view);
            }
        }

        private void OnButtonClicked(PowerUpConfigBase config)
        {
            if (_PowerUpService == null)
                return;

            if (_PowerUpService.TryActivate(config))
            {
                Select(config);
            }
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
    }
}