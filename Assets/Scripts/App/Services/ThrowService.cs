using System;
using Core.Abstractions;
using Core.Data;
using Infrastructure.Events;
using Infrastructure.Leveling;
using UnityEngine;

namespace App.Services
{
    /// <summary>
    /// Service that coordinates knife throwing actions based on player input.
    /// Handles the flow from input events to knife spawning via event channels.
    /// </summary>
    public class ThrowService : IThrowService
    {
        private readonly IPlayerThrowInputListener _playerThrowInputListener;
        private readonly KnifeThrowConfigAsset _knifeThrowConfig;
        private readonly IEventChannel<KnifeSpawnRequestData> _spawnRequestEventChannel;

        /// <summary>
        /// Initializes a new instance of the ThrowService.
        /// </summary>
        /// <param name="playerThrowInputListener">The listener that provides player throw input events.</param>
        /// <param name="knifeThrowConfig">The configuration asset containing knife throw parameters.</param>
        /// <param name="spawnRequestEventChannel">The event channel for raising knife spawn requests.</param>
        public ThrowService(
            IPlayerThrowInputListener playerThrowInputListener, 
            KnifeThrowConfigAsset knifeThrowConfig,
            IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel)
        {
            _playerThrowInputListener = playerThrowInputListener;
            _knifeThrowConfig = knifeThrowConfig;
            _spawnRequestEventChannel = spawnRequestEventChannel;
            
            _playerThrowInputListener.OnThrowRequested += HandleThrowRequested;
            _playerThrowInputListener.Initialize();
        }

        private void HandleThrowRequested()
        {
            Vector2 spawnPosition = _knifeThrowConfig.SpawnPositionOffset;
            float throwSpeed = _knifeThrowConfig.ThrowSpeed;
            
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffsetX = spawnPosition.x,
                SpawnOffsetY = spawnPosition.y,
                ThrowSpeed = throwSpeed
            };
            
            _spawnRequestEventChannel.Raise(spawnData);
        }

        /// <summary>
        /// Disposes of resources and unsubscribes from input events.
        /// </summary>
        public void Dispose()
        {
            _playerThrowInputListener.OnThrowRequested -= HandleThrowRequested;
        }
    }
}
