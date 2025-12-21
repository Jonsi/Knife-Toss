using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Events;
using Game.Entities.Collectible.Views;
using Game.Entities.Collectible.Data;
using Game.Entities.Knife.Views;
using Game.Flow.Events;
using Game.KnifeThrower.Configs;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using Game.PowerUps.Events;
using Game.PowerUps.Interfaces;
using Game.Persistence.Services;
using Game.Pooling;
using UnityEngine;
using VContainer.Unity;

namespace Game.PowerUps.Services
{
    /// <summary>
    /// Manages power-up activation, consumption, and execution.
    /// </summary>
    public class PowerUpService : IPowerUpService, IStartable, IDisposable
    {
        private readonly PlayerDataService _PlayerDataService;
        private readonly PowerUpActivatedEventChannel _ActivatedChannel;
        private readonly PowerUpUsedEventChannel _UsedChannel;
        private readonly LevelStartedEventChannel _LevelStartedChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly CollectibleRegistry _CollectibleRegistry;
        private readonly PowerUpRuntimeState _RuntimeState = new PowerUpRuntimeState();

        /// <summary>
        /// Initializes a new instance of the power-up service.
        /// </summary>
        public PowerUpService(
            PlayerDataService playerDataService,
            PowerUpActivatedEventChannel activatedChannel,
            PowerUpUsedEventChannel usedChannel,
            LevelStartedEventChannel levelStartedChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            CollectibleRegistry collectibleRegistry)
        {
            _PlayerDataService = playerDataService;
            _ActivatedChannel = activatedChannel;
            _UsedChannel = usedChannel;
            _LevelStartedChannel = levelStartedChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _CollectibleRegistry = collectibleRegistry;
        }

        /// <summary>
        /// Gets whether a power-up is currently active.
        /// </summary>
        public bool HasActivePowerUp => _RuntimeState.HasActivePowerUp;
        /// <summary>
        /// Gets the active power-up configuration.
        /// </summary>
        public PowerUpConfigBase ActiveConfig => _RuntimeState.ActiveConfig;

        /// <summary>
        /// Subscribes to level lifecycle events.
        /// </summary>
        public void Start()
        {
            _LevelStartedChannel.Subscribe(HandleLevelStarted);
            ResetState();
        }

        /// <summary>
        /// Cleans up subscriptions.
        /// </summary>
        public void Dispose()
        {
            _LevelStartedChannel.Unsubscribe(HandleLevelStarted);
        }

        /// <summary>
        /// Attempts to activate a power-up if inventory allows.
        /// </summary>
        public bool TryActivate(PowerUpConfigBase config)
        {
            if (config == null)
                return false;

            var inventory = _PlayerDataService.EnsurePowerUpInventory();
            if (!HasInventory(inventory, config))
                return false;

            _RuntimeState.HasActivePowerUp = true;
            _RuntimeState.ActiveConfig = config;
            _ActivatedChannel?.Raise(new PowerUpActivatedEventData(config, GetRemainingCount(inventory, config)));
            return true;
        }

        /// <summary>
        /// Clears active power-up state.
        /// </summary>
        public void ClearActive()
        {
            _RuntimeState.Clear();
        }

        /// <summary>
        /// Prepares the loaded knife based on the active power-up.
        /// </summary>
        public void PrepareLoadedKnives(IReadOnlyList<KnifeView> knives)
        {
            if (knives == null || knives.Count == 0)
                return;

            if (!HasActivePowerUp)
                return;

            if (_RuntimeState.ActiveConfig is SuperKnifeConfig)
            {
                for (int i = 0; i < knives.Count; i++)
                {
                    var knife = knives[i];
                    if (knife == null)
                        continue;

                    knife.IsSuperKnife = true;
                }
            }
        }

        /// <summary>
        /// Attempts to handle a throw request using the active power-up.
        /// </summary>
        public bool TryHandleThrow(PowerUpThrowContext context)
        {
            if (!HasActivePowerUp || context == null)
                return false;

            bool handled = false;
            switch (_RuntimeState.ActiveConfig)
            {
                case SuperKnifeConfig superKnifeConfig:
                    handled = ExecuteSuperKnife(context, superKnifeConfig);
                    break;
                case MultipleKnivesConfig multipleKnivesConfig:
                    handled = ExecuteMultipleKnives(context, multipleKnivesConfig);
                    break;
                case CollectiblesBladeConfig collectiblesBladeConfig:
                    handled = ExecuteCollectiblesBlade(collectiblesBladeConfig);
                    break;
            }

            if (handled)
            {
                ConsumeActive();
            }

            return handled;
        }

        /// <summary>
        /// Clears runtime state when a level begins.
        /// </summary>
        public void OnLevelStarted()
        {
            ResetState();
        }

        private void HandleLevelStarted(LevelStartedEventData data)
        {
            OnLevelStarted();
        }

        private void ResetState()
        {
            _RuntimeState.Clear();
        }

        private bool ExecuteSuperKnife(PowerUpThrowContext context, SuperKnifeConfig config)
        {
            context.ThrowerView.ThrowLoadedKnife(config.ThrowSpeed);
            return true;
        }

        private bool ExecuteMultipleKnives(PowerUpThrowContext context, MultipleKnivesConfig config)
        {
            int thrown = context.ThrowerView.ThrowLoadedKnives(config.ThrowSpeed);
            return thrown > 0;
        }

        private bool ExecuteCollectiblesBlade(CollectiblesBladeConfig config)
        {
            _ = CollectAllCollectibles(config);
            return true;
        }

        private async Task CollectAllCollectibles(CollectiblesBladeConfig config)
        {
            if (_CollectibleRegistry == null)
                return;

            var items = new List<CollectibleView>(_CollectibleRegistry.GetAll());
            for (int i = 0; i < items.Count; i++)
            {
                var collectible = items[i];
                if (collectible == null)
                    continue;

                if (!collectible.MarkAsHit())
                    continue;

                collectible.PlayHitFeedback();
                _CollectibleHitChannel?.Raise(new CollectibleHitEventData(collectible.transform.position, collectible.Value));
                UnityEngine.Object.Destroy(collectible.gameObject);

                if (config.TimeBetweenBlades > 0f)
                {
                    await Task.Delay((int)(config.TimeBetweenBlades * 1000f));
                }
            }
        }

        private void ConsumeActive()
        {
            var inventory = _PlayerDataService.EnsurePowerUpInventory();
            DecrementInventory(inventory, _RuntimeState.ActiveConfig);
            _PlayerDataService.SavePowerUpInventory();

            _UsedChannel?.Raise(new PowerUpUsedEventData(_RuntimeState.ActiveConfig, GetRemainingCount(inventory, _RuntimeState.ActiveConfig)));
            _RuntimeState.Clear();
        }

        private static bool HasInventory(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            return GetRemainingCount(inventory, config) > 0;
        }

        private static int GetRemainingCount(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            return config switch
            {
                SuperKnifeConfig => inventory.SuperKnifeCount,
                MultipleKnivesConfig => inventory.MultipleKnivesCount,
                CollectiblesBladeConfig => inventory.CollectiblesBladeCount,
                _ => 0
            };
        }

        private static void DecrementInventory(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            switch (config)
            {
                case SuperKnifeConfig:
                    inventory.SuperKnifeCount = Mathf.Max(0, inventory.SuperKnifeCount - 1);
                    break;
                case MultipleKnivesConfig:
                    inventory.MultipleKnivesCount = Mathf.Max(0, inventory.MultipleKnivesCount - 1);
                    break;
                case CollectiblesBladeConfig:
                    inventory.CollectiblesBladeCount = Mathf.Max(0, inventory.CollectiblesBladeCount - 1);
                    break;
            }
        }
    }
}
