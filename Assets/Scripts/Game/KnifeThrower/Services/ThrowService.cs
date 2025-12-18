using System;
using System.Collections.Generic;
using Common.Config;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using Game.Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Game.KnifeThrower.Services
{
    /// <summary>
    /// Service that coordinates knife throwing actions based on player input.
    /// Manages knife lifecycle through pooling and view coordination.
    /// </summary>
    public class ThrowService : IThrowService, IStartable, IDisposable
    {
        private readonly IPlayerThrowInputListener _PlayerThrowInputListener;
        private readonly IConfigurationProvider<KnifeThrowConfiguration> _KnifeThrowConfigurationProvider;
        private readonly IKnifeThrowerView _KnifeThrowerView;
        private readonly KnifePoolProvider _KnifePoolProvider;
        private readonly IPowerUpService _PowerUpService;
        private readonly IEventChannel<PowerUpActivatedEventData> _PowerUpActivatedChannel;
        private readonly IEventChannel<PowerUpUsedEventData> _PowerUpUsedChannel;
        private bool _IsRunning;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener,
            IConfigurationProvider<KnifeThrowConfiguration> knifeThrowConfigurationProvider,
            IKnifeThrowerView knifeThrowerView,
            KnifePoolProvider knifePoolProvider,
            IPowerUpService powerUpService,
            IEventChannel<PowerUpActivatedEventData> powerUpActivatedChannel,
            IEventChannel<PowerUpUsedEventData> powerUpUsedChannel)
        {
            _PlayerThrowInputListener = playerThrowInputListener;
            _KnifeThrowConfigurationProvider = knifeThrowConfigurationProvider;
            _KnifeThrowerView = knifeThrowerView;
            _KnifePoolProvider = knifePoolProvider;
            _PowerUpService = powerUpService;
            _PowerUpActivatedChannel = powerUpActivatedChannel;
            _PowerUpUsedChannel = powerUpUsedChannel;
        }

        /// <summary>
        /// Called automatically by VContainer when the LifetimeScope starts.
        /// Initializes the input listener and subscribes to throw events.
        /// </summary>
        public void Start()
        {
            if (_IsRunning)
                return;

            _PlayerThrowInputListener.OnThrowRequested += HandleThrowRequested;
            _PlayerThrowInputListener.Initialize();

            _PowerUpActivatedChannel?.Subscribe(OnPowerUpActivated);
            _PowerUpUsedChannel?.Subscribe(OnPowerUpUsed);

            ReloadLoadedKnifeForActivePowerUp();
            _IsRunning = true;
        }

        private void HandleThrowRequested()
        {
            if (IsPointerOverUI())
                return;

            var config = _KnifeThrowConfigurationProvider.GetConfiguration();
            ReloadLoadedKnifeForActivePowerUp();
            _PowerUpService?.PrepareLoadedKnife(_KnifeThrowerView.LoadedKnife);

            var context = new PowerUpThrowContext(
                _KnifeThrowerView,
                _KnifePoolProvider,
                config,
                _KnifeThrowerView.SpawnPoint);

            if (_PowerUpService != null && _PowerUpService.TryHandleThrow(context))
            {
                LoadKnife(config);
                return;
            }

            _KnifeThrowerView.ThrowLoadedKnife(config.ThrowSpeed);
            LoadKnife(config);
        }

        private bool IsPointerOverUI()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            var pointer = Pointer.current;
            if (pointer == null)
                return false;

            var position = pointer.position.ReadValue();
            var pointerData = new PointerEventData(eventSystem) { position = position };
            var raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, raycastResults);
            return raycastResults.Count > 0;
        }

        private void LoadKnife(KnifeThrowConfiguration config)
        {
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = config.SpawnPositionOffset,
                ThrowSpeed = config.ThrowSpeed
            };
            var knife = _KnifePoolProvider.Get(_PowerUpService?.ActiveConfig);
            _PowerUpService?.PrepareLoadedKnife(knife);
            _KnifeThrowerView.LoadKnife(knife, spawnData);
        }

        private void OnPowerUpActivated(PowerUpActivatedEventData data)
        {
            ReloadLoadedKnifeForActivePowerUp();
        }

        private void OnPowerUpUsed(PowerUpUsedEventData data)
        {
            ReloadLoadedKnifeForActivePowerUp();
        }

        private void ReloadLoadedKnifeForActivePowerUp()
        {
            var config = _KnifeThrowConfigurationProvider.GetConfiguration();
            var activePowerUp = _PowerUpService?.ActiveConfig;
            var loadedKnife = _KnifeThrowerView.LoadedKnife;

            if (loadedKnife != null && loadedKnife.SourcePowerUpConfig == activePowerUp)
                return;

            if (loadedKnife != null)
            {
                _KnifePoolProvider.Release(loadedKnife);
            }

            var knife = _KnifePoolProvider.Get(activePowerUp);
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = config.SpawnPositionOffset,
                ThrowSpeed = config.ThrowSpeed
            };
            _KnifeThrowerView.LoadKnife(knife, spawnData);
        }

        /// <summary>
        /// Disposes of resources and unsubscribes from input events.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Stops listening for throw input and cleans up resources.
        /// </summary>
        public void Stop()
        {
            if (!_IsRunning)
                return;

            _PlayerThrowInputListener.OnThrowRequested -= HandleThrowRequested;
            _PlayerThrowInputListener.Dispose();

            _PowerUpActivatedChannel?.Unsubscribe(OnPowerUpActivated);
            _PowerUpUsedChannel?.Unsubscribe(OnPowerUpUsed);
            _IsRunning = false;
        }
    }
}
