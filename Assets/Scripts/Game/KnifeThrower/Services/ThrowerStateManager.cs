using System.Collections.Generic;
using Common.Config;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.PowerUps.Configs;
using Game.PowerUps.Interfaces;
using Game.Pooling;
using UnityEngine;

namespace Game.KnifeThrower.Services
{
    /// <summary>
    /// Implementation of IThrowerStateManager that handles transitions between normal and powerup throwers.
    /// </summary>
    public class ThrowerStateManager : IThrowerStateManager
    {
        private readonly IKnifeThrowerView _NormalThrower;
        private readonly IPowerUpThrowerView _PowerUpThrower;
        private readonly KnifePoolProvider _KnifePoolProvider;
        private readonly IConfigurationProvider<KnifeThrowConfiguration> _ConfigProvider;

        public bool IsProjectilePowerUpActive { get; private set; }

        public ThrowerStateManager(
            IKnifeThrowerView normalThrower,
            IPowerUpThrowerView powerUpThrower,
            KnifePoolProvider knifePoolProvider,
            IConfigurationProvider<KnifeThrowConfiguration> configProvider)
        {
            _NormalThrower = normalThrower;
            _PowerUpThrower = powerUpThrower;
            _KnifePoolProvider = knifePoolProvider;
            _ConfigProvider = configProvider;
        }

        public void ActivateProjectilePowerUp(PowerUpConfigBase config)
        {
            IsProjectilePowerUpActive = true;
            _NormalThrower.SetVisible(false);
            _PowerUpThrower.SetVisible(true);
            
            ReloadPowerUpThrower(config);
        }

        public void DeactivateProjectilePowerUp()
        {
            IsProjectilePowerUpActive = false;
            _PowerUpThrower.ClearLoadedKnives();
            _PowerUpThrower.SetVisible(false);
            _NormalThrower.SetVisible(true);
            ReloadNormalThrower();
        }

        public void CancelProjectilePowerUp()
        {
            DeactivateProjectilePowerUp();
        }

        private void ReloadPowerUpThrower(PowerUpConfigBase config)
        {
            var throwConfig = _ConfigProvider.GetConfiguration();
            _PowerUpThrower.ClearLoadedKnives();

            int count = GetDesiredKnifeCount(config);
            float spacing = GetKnifeSpacing(config);

            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = throwConfig.SpawnPositionOffset,
                ThrowSpeed = throwConfig.ThrowSpeed
            };

            if (count <= 1)
            {
                var knife = _KnifePoolProvider.Get(config);
                config.PrepareKnives(new[] { knife });
                _PowerUpThrower.LoadKnife(knife, spawnData);
            }
            else
            {
                var knives = new List<KnifeView>(count);
                for (int i = 0; i < count; i++)
                {
                    knives.Add(_KnifePoolProvider.Get(config));
                }
                config.PrepareKnives(knives);
                _PowerUpThrower.LoadKnives(knives, spawnData, spacing);
            }
        }

        private void ReloadNormalThrower()
        {
            var throwConfig = _ConfigProvider.GetConfiguration();
            var spawnData = new KnifeSpawnRequestData
            {
                SpawnOffset = throwConfig.SpawnPositionOffset,
                ThrowSpeed = throwConfig.ThrowSpeed
            };

            var knife = _KnifePoolProvider.Get(null);
            _NormalThrower.LoadKnife(knife, spawnData);
        }

        private int GetDesiredKnifeCount(PowerUpConfigBase config)
        {
            if (config is MultipleKnivesConfig multipleKnivesConfig)
            {
                return Mathf.Max(1, multipleKnivesConfig.KnifeCount);
            }
            return 1;
        }

        private float GetKnifeSpacing(PowerUpConfigBase config)
        {
            if (config is MultipleKnivesConfig multipleKnivesConfig)
            {
                return Mathf.Max(0f, multipleKnivesConfig.KnifeSpacing);
            }
            return 0f;
        }
    }
}
