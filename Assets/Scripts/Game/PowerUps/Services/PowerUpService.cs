using System;
using Game.Entities.Knife.Views;
using Game.Flow.Events;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using Game.PowerUps.Events;
using Game.PowerUps.Interfaces;
using Game.Persistence.Services;
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
        private readonly PowerUpCancelledEventChannel _CancelledChannel;
        private readonly LevelStartedEventChannel _LevelStartedChannel;
        private readonly PowerUpEffectService _EffectService;
        private readonly Game.KnifeThrower.Interfaces.IThrowerStateManager _ThrowerStateManager;
        private readonly PowerUpRuntimeState _RuntimeState = new PowerUpRuntimeState();

        /// <summary>
        /// Initializes a new instance of the power-up service.
        /// </summary>
        public PowerUpService(
            PlayerDataService playerDataService,
            PowerUpActivatedEventChannel activatedChannel,
            PowerUpUsedEventChannel usedChannel,
            PowerUpCancelledEventChannel cancelledChannel,
            LevelStartedEventChannel levelStartedChannel,
            PowerUpEffectService effectService,
            Game.KnifeThrower.Interfaces.IThrowerStateManager throwerStateManager)
        {
            _PlayerDataService = playerDataService;
            _ActivatedChannel = activatedChannel;
            _UsedChannel = usedChannel;
            _CancelledChannel = cancelledChannel;
            _LevelStartedChannel = levelStartedChannel;
            _EffectService = effectService;
            _ThrowerStateManager = throwerStateManager;
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
            if (config == null || string.IsNullOrEmpty(config.InventoryKey))
                return false;

            var inventory = _PlayerDataService.EnsurePowerUpInventory();
            if (!HasInventory(inventory, config))
                return false;

            // World effects apply immediately without affecting thrower
            if (config is IWorldEffectPowerUp worldEffect)
            {
                worldEffect.ApplyWorldEffect(_EffectService);
                ConsumeInventory(inventory, config);
                _UsedChannel?.Raise(new PowerUpUsedEventData(config, GetRemainingCount(inventory, config)));
                return true;
            }

            // Projectile powerups switch thrower state
            if (config is IProjectilePowerUp)
            {
                _ThrowerStateManager.ActivateProjectilePowerUp(config);
                _RuntimeState.HasActivePowerUp = true;
                _RuntimeState.ActiveConfig = config;
                _ActivatedChannel?.Raise(new PowerUpActivatedEventData(config, GetRemainingCount(inventory, config)));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to cancel the active power-up.
        /// </summary>
        public bool TryCancelActive()
        {
            if (!HasActivePowerUp)
                return false;

            if (_RuntimeState.ActiveConfig is ICancellablePowerUp cancellable)
            {
                cancellable.OnCancelled();
                _ThrowerStateManager.CancelProjectilePowerUp();
                var config = _RuntimeState.ActiveConfig;
                _RuntimeState.Clear();
                _CancelledChannel?.Raise(new PowerUpCancelledEventData(config));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears active power-up state.
        /// </summary>
        public void ClearActive()
        {
            _RuntimeState.Clear();
        }

        /// <summary>
        /// Attempts to handle a throw request using the active power-up.
        /// </summary>
        public bool TryHandleThrow(PowerUpThrowContext context)
        {
            if (!HasActivePowerUp || context == null)
                return false;

            var executionContext = new PowerUpExecutionContext(context);
            bool handled = _RuntimeState.ActiveConfig.Execute(executionContext);

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

        private void ConsumeActive()
        {
            var inventory = _PlayerDataService.EnsurePowerUpInventory();
            var activeConfig = _RuntimeState.ActiveConfig;

            if (activeConfig is IProjectilePowerUp)
            {
                _ThrowerStateManager.DeactivateProjectilePowerUp();
            }

            ConsumeInventory(inventory, activeConfig);
            _RuntimeState.Clear();
            _UsedChannel?.Raise(new PowerUpUsedEventData(activeConfig, GetRemainingCount(inventory, activeConfig)));
        }

        private void ConsumeInventory(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            DecrementInventory(inventory, config);
            _PlayerDataService.SavePowerUpInventory();
        }

        private static bool HasInventory(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            return GetRemainingCount(inventory, config) > 0;
        }

        private static int GetRemainingCount(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            if (config == null || string.IsNullOrEmpty(config.InventoryKey))
                return 0;

            return inventory.GetCount(config.InventoryKey);
        }

        private static void DecrementInventory(PowerUpInventoryData inventory, PowerUpConfigBase config)
        {
            if (config == null || string.IsNullOrEmpty(config.InventoryKey))
                return;

            inventory.DecrementCount(config.InventoryKey, 1);
        }
    }
}
