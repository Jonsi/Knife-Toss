using System;
using System.Collections.Generic;
using Common.Config;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.PowerUps.Configs;
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
    public class ThrowService : IThrowService, IStartable, IDisposable, ITickable
    {
        private readonly IPlayerThrowInputListener _PlayerThrowInputListener;
        private readonly IConfigurationProvider<KnifeThrowConfiguration> _KnifeThrowConfigurationProvider;
        private readonly IKnifeThrowerView _KnifeThrowerView;
        private readonly IPowerUpThrowerView _PowerUpThrowerView;
        private readonly KnifePoolProvider _KnifePoolProvider;
        private readonly IPowerUpService _PowerUpService;
        private readonly IThrowerStateManager _ThrowerStateManager;
        private readonly IEventChannel<PowerUpActivatedEventData> _PowerUpActivatedChannel;
        private readonly IEventChannel<PowerUpUsedEventData> _PowerUpUsedChannel;
        private readonly IEventChannel _KnifeThrownChannel;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;

        private bool _IsRunning;
        private bool _IsPointerDown;
        private int _KnivesInFlight;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener,
            IConfigurationProvider<KnifeThrowConfiguration> knifeThrowConfigurationProvider,
            IKnifeThrowerView knifeThrowerView,
            IPowerUpThrowerView powerUpThrowerView,
            KnifePoolProvider knifePoolProvider,
            IPowerUpService powerUpService,
            IThrowerStateManager throwerStateManager,
            IEventChannel<PowerUpActivatedEventData> powerUpActivatedChannel,
            IEventChannel<PowerUpUsedEventData> powerUpUsedChannel,
            IEventChannel knifeThrownChannel,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel)
        {
            _PlayerThrowInputListener = playerThrowInputListener;
            _KnifeThrowConfigurationProvider = knifeThrowConfigurationProvider;
            _KnifeThrowerView = knifeThrowerView;
            _PowerUpThrowerView = powerUpThrowerView;
            _KnifePoolProvider = knifePoolProvider;
            _PowerUpService = powerUpService;
            _ThrowerStateManager = throwerStateManager;
            _PowerUpActivatedChannel = powerUpActivatedChannel;
            _PowerUpUsedChannel = powerUpUsedChannel;
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifeImpactChannel = knifeImpactChannel;
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
            _PlayerThrowInputListener.OnThrowReleased += HandleThrowReleased;
            _PlayerThrowInputListener.Initialize();

            _PowerUpActivatedChannel?.Subscribe(OnPowerUpActivated);
            _PowerUpUsedChannel?.Subscribe(OnPowerUpUsed);
            _KnifeThrownChannel?.Subscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Subscribe(OnKnifeImpact);

            _IsRunning = true;
        }

        public void Tick()
        {
            if (!_IsRunning || !_IsPointerDown)
                return;

            if (_PowerUpService?.ActiveConfig is KnifeAimConfig)
            {
                UpdateKnifeAiming();
            }
        }

        private void OnKnifeThrown()
        {
            _KnivesInFlight++;
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (data.ImpactType == KnifeImpactType.Stuck || data.ImpactType == KnifeImpactType.OutOfView)
            {
                _KnivesInFlight = Mathf.Max(0, _KnivesInFlight - 1);
            }
        }

        private void UpdateKnifeAiming()
        {
            var thrower = _ThrowerStateManager.IsProjectilePowerUpActive ? (IProjectileThrower)_PowerUpThrowerView : _KnifeThrowerView;
            var loadedKnives = thrower.LoadedKnives;
            
            if (loadedKnives == null || loadedKnives.Count == 0)
                return;

            var pointer = UnityEngine.InputSystem.Pointer.current;
            if (pointer == null)
                return;

            Vector2 screenPosition = pointer.position.ReadValue();
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            // Use the thrower's spawn point's Z distance from the camera for accurate world point calculation
            float distance = Mathf.Abs(mainCamera.transform.position.z - thrower.SpawnPoint.position.z);
            Vector3 screenPosWithZ = new Vector3(screenPosition.x, screenPosition.y, distance);
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosWithZ);

            foreach (var knife in loadedKnives)
            {
                if (knife == null) continue;
                
                // Vector pointing from knife to cursor
                Vector2 direction = ((Vector2)worldPosition - (Vector2)knife.transform.position).normalized;
                if (direction.sqrMagnitude > 0.001f)
                {
                    // Assuming knife sprite points UP (+Y) by default.
                    // Atan2 returns angle from +X axis. We subtract 90 to align +Y with the direction.
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                    knife.transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }

        private void HandleThrowRequested()
        {
            if (IsPointerOverUI())
                return;

            _IsPointerDown = true;
            
            // If KnifeAim is NOT active, we might want to throw immediately or prepare.
            // Requirement says: "after activating the knife aim power up... knife should only be thrown on release".
            // For other powerups/normal state, it seems they currently throw on "performed" (which was "press").
            
            if (!(_PowerUpService?.ActiveConfig is KnifeAimConfig))
            {
                PerformThrow();
            }
        }

        private void HandleThrowReleased()
        {
            if (!_IsPointerDown)
                return;

            if (_PowerUpService?.ActiveConfig is KnifeAimConfig)
            {
                PerformThrow();
            }

            _IsPointerDown = false;
        }

        private void PerformThrow()
        {
            var activePowerUp = _PowerUpService?.ActiveConfig;
            
            // Block throwing if a knife is already in flight and no power-up is active.
            if (!_ThrowerStateManager.IsProjectilePowerUpActive && _KnivesInFlight > 0)
                return;

            var config = _KnifeThrowConfigurationProvider.GetConfiguration();
            
            if (_ThrowerStateManager.IsProjectilePowerUpActive && activePowerUp != null)
            {
                var context = new PowerUpThrowContext(
                    _PowerUpThrowerView,
                    _KnifePoolProvider,
                    config,
                    _PowerUpThrowerView.SpawnPoint);

                if (_PowerUpService.TryHandleThrow(context))
                {
                    // Power-up throw handled, magazine remains untouched.
                    return;
                }
            }

            // Normal magazine-based throw
            _KnifeThrowerView.ThrowLoadedKnife();
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
            var knife = _KnifePoolProvider.Get(null); // Explicitly normal knife
            _KnifeThrowerView.LoadKnife(knife, spawnData);
        }

        private void OnPowerUpActivated(PowerUpActivatedEventData data)
        {
            // Redundant: PowerUpService now handles this through ThrowerStateManager
        }

        private void OnPowerUpUsed(PowerUpUsedEventData data)
        {
            // Redundant: PowerUpService now handles this through ThrowerStateManager
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
            _PlayerThrowInputListener.OnThrowReleased -= HandleThrowReleased;
            _PlayerThrowInputListener.Dispose();

            _PowerUpActivatedChannel?.Unsubscribe(OnPowerUpActivated);
            _PowerUpUsedChannel?.Unsubscribe(OnPowerUpUsed);
            _KnifeThrownChannel?.Unsubscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Unsubscribe(OnKnifeImpact);
            _IsRunning = false;
        }
    }
}
