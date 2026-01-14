using System;
using System.Collections.Generic;
using Game.Entities.Target.Interfaces;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.PowerUps.Interfaces;
using UnityEngine;
using VContainer.Unity;

namespace Game.PowerUps.Services
{
    /// <summary>
    /// Manages timed power-up effects applied to targets.
    /// </summary>
    public class PowerUpEffectService : IStartable, ITickable, IDisposable
    {
        private readonly LevelData _LevelData;
        private readonly LevelStartedEventChannel _LevelStartedChannel;

        private readonly List<ActiveEffect> _ActiveEffects = new List<ActiveEffect>();

        /// <summary>
        /// Initializes a new instance of the power-up effect service.
        /// </summary>
        public PowerUpEffectService(LevelData levelData, LevelStartedEventChannel levelStartedChannel)
        {
            _LevelData = levelData;
            _LevelStartedChannel = levelStartedChannel;
        }

        /// <summary>
        /// Subscribes to level lifecycle events.
        /// </summary>
        public void Start()
        {
            _LevelStartedChannel?.Subscribe(HandleLevelStarted);
        }

        /// <summary>
        /// Applies a timed effect to all active targets.
        /// </summary>
        /// <param name="effect">Effect to apply.</param>
        /// <param name="duration">Duration in seconds.</param>
        public void ApplyEffect(IPowerUpEffect effect, float duration)
        {
            if (effect == null || duration <= 0f)
                return;

            var activeEffect = new ActiveEffect
            {
                Effect = effect,
                RemainingTime = duration
            };

            ApplyEffectToAllTargets(effect);
            _ActiveEffects.Add(activeEffect);
        }

        /// <summary>
        /// Updates active effects and removes expired ones.
        /// </summary>
        public void Tick()
        {
            if (_ActiveEffects.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = _ActiveEffects.Count - 1; i >= 0; i--)
            {
                var activeEffect = _ActiveEffects[i];
                activeEffect.RemainingTime -= deltaTime;

                if (activeEffect.RemainingTime <= 0f)
                {
                    RemoveEffectFromAllTargets(activeEffect.Effect);
                    _ActiveEffects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Clears all active effects.
        /// </summary>
        public void ClearAllEffects()
        {
            for (int i = 0; i < _ActiveEffects.Count; i++)
            {
                RemoveEffectFromAllTargets(_ActiveEffects[i].Effect);
            }
            _ActiveEffects.Clear();
        }

        private void HandleLevelStarted(LevelStartedEventData data)
        {
            ClearAllEffects();
        }

        private void ApplyEffectToAllTargets(IPowerUpEffect effect)
        {
            if (_LevelData?.ActiveTargets == null)
                return;

            for (int i = 0; i < _LevelData.ActiveTargets.Count; i++)
            {
                var target = _LevelData.ActiveTargets[i];
                if (target != null)
                {
                    effect.Apply(target);
                }
            }
        }

        private void RemoveEffectFromAllTargets(IPowerUpEffect effect)
        {
            if (_LevelData?.ActiveTargets == null)
                return;

            for (int i = 0; i < _LevelData.ActiveTargets.Count; i++)
            {
                var target = _LevelData.ActiveTargets[i];
                if (target != null)
                {
                    effect.Remove(target);
                }
            }
        }

        public void Dispose()
        {
            _LevelStartedChannel?.Unsubscribe(HandleLevelStarted);
            ClearAllEffects();
        }

        private class ActiveEffect
        {
            public IPowerUpEffect Effect;
            public float RemainingTime;
        }
    }
}
