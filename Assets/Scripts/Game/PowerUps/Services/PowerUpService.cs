using System;
using Common.Events;
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
        private readonly IEventChannel<PowerUpOutcomeEventData> _OutcomeChannel;
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
            IEventChannel<PowerUpOutcomeEventData> outcomeChannel,
            LevelStartedEventChannel levelStartedChannel,
            PowerUpEffectService effectService,
            Game.KnifeThrower.Interfaces.IThrowerStateManager throwerStateManager)
        {
            _PlayerDataService = playerDataService;
            _ActivatedChannel = activatedChannel;
            _UsedChannel = usedChannel;
            _CancelledChannel = cancelledChannel;
            _OutcomeChannel = outcomeChannel;
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
            {
                RaiseOutcome(config, PowerUpOutcome.InvalidConfiguration, 0, "Power-up configuration or inventory key is missing.");
                return false;
            }

            var inventory = _PlayerDataService.EnsurePowerUpInventory();
            if (!HasInventory(inventory, config))
            {
                RaiseOutcome(config, PowerUpOutcome.Unavailable, 0, "No inventory is available for this power-up.");
                return false;
            }

            // World effects apply immediately without affecting thrower
            if (config is IWorldEffectPowerUp worldEffect)
            {
                if (config is IPowerUpEffect effect &&
                    _EffectService.TryGetEffectProgress(effect, out _))
                {
                    RaiseOutcome(
                        config,
                        PowerUpOutcome.Blocked,
                        GetRemainingCount(inventory, config),
                        "This world effect is already active.");
                    return false;
                }

                worldEffect.ApplyWorldEffect(_EffectService);
                int countBeforeConsumption = GetRemainingCount(inventory, config);
                _ActivatedChannel?.Raise(new PowerUpActivatedEventData(config, countBeforeConsumption));
                RaiseOutcome(config, PowerUpOutcome.Activated, countBeforeConsumption);
                ConsumeInventory(inventory, config);
                int remainingCount = GetRemainingCount(inventory, config);
                _UsedChannel?.Raise(new PowerUpUsedEventData(config, remainingCount));
                RaiseOutcome(config, PowerUpOutcome.Consumed, remainingCount);
                return true;
            }

            // Projectile power-ups switch thrower state
            if (config is IProjectilePowerUp)
            {
                if (HasActivePowerUp)
                {
                    RaiseOutcome(
                        config,
                        PowerUpOutcome.Blocked,
                        GetRemainingCount(inventory, config),
                        "Another projectile power-up is already active.");
                    return false;
                }

                _ThrowerStateManager.ActivateProjectilePowerUp(config);
                _RuntimeState.HasActivePowerUp = true;
                _RuntimeState.ActiveConfig = config;
                int remainingCount = GetRemainingCount(inventory, config);
                _ActivatedChannel?.Raise(new PowerUpActivatedEventData(config, remainingCount));
                RaiseOutcome(config, PowerUpOutcome.Activated, remainingCount);
                return true;
            }

            RaiseOutcome(
                config,
                PowerUpOutcome.Unsupported,
                GetRemainingCount(inventory, config),
                "The power-up does not implement a supported execution type.");
            return false;
        }

        /// <summary>
        /// Gets the normalized remaining time for an active world effect.
        /// </summary>
        public bool TryGetWorldEffectProgress(PowerUpConfigBase config, out float normalizedRemaining)
        {
            if (config is IPowerUpEffect effect)
            {
                return _EffectService.TryGetEffectProgress(effect, out normalizedRemaining);
            }

            normalizedRemaining = 0f;
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
                RaiseOutcome(config, PowerUpOutcome.Cancelled, GetRemainingCountFor(config));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears active power-up state.
        /// </summary>
        public void ClearActive()
        {
            ResetState();
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
            else
            {
                var config = _RuntimeState.ActiveConfig;
                RaiseOutcome(
                    config,
                    PowerUpOutcome.ExecutionFailed,
                    GetRemainingCountFor(config),
                    "The active power-up could not handle the throw request.");
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
            var config = _RuntimeState.ActiveConfig;
            if (_ThrowerStateManager != null && _ThrowerStateManager.IsProjectilePowerUpActive)
            {
                _ThrowerStateManager.DeactivateProjectilePowerUp();
            }

            _RuntimeState.Clear();
            if (config != null)
            {
                RaiseOutcome(config, PowerUpOutcome.Cleared, GetRemainingCountFor(config));
            }
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
            int remainingCount = GetRemainingCount(inventory, activeConfig);
            _UsedChannel?.Raise(new PowerUpUsedEventData(activeConfig, remainingCount));
            RaiseOutcome(activeConfig, PowerUpOutcome.Consumed, remainingCount);
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

        private int GetRemainingCountFor(PowerUpConfigBase config)
        {
            return GetRemainingCount(_PlayerDataService.EnsurePowerUpInventory(), config);
        }

        private void RaiseOutcome(
            PowerUpConfigBase config,
            PowerUpOutcome outcome,
            int remainingCount,
            string failureReason = null)
        {
            _OutcomeChannel?.Raise(new PowerUpOutcomeEventData(config, outcome, remainingCount, failureReason));
        }
    }
}
