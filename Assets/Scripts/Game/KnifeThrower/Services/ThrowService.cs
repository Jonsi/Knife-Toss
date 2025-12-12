using System;
using System.Collections.Generic;
using Common.Config;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
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
        private bool _IsRunning;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener,
            IConfigurationProvider<KnifeThrowConfiguration> knifeThrowConfigurationProvider,
            IKnifeThrowerView knifeThrowerView,
            KnifePoolProvider knifePoolProvider)
        {
            _PlayerThrowInputListener = playerThrowInputListener;
            _KnifeThrowConfigurationProvider = knifeThrowConfigurationProvider;
            _KnifeThrowerView = knifeThrowerView;
            _KnifePoolProvider = knifePoolProvider;
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
            _IsRunning = true;
        }

        private void HandleThrowRequested()
        {
            if (IsPointerOverUI())
                return;

            var config = _KnifeThrowConfigurationProvider.GetConfiguration();
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
            var knife = _KnifePoolProvider.Get();
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
            _IsRunning = false;
        }
    }
}
