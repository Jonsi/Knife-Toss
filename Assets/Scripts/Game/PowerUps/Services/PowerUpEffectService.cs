using System;
using System.Collections.Generic;
using Common.Events;
using Game.Entities.Target.Interfaces;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
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
        private readonly IEventChannel<PowerUpOutcomeEventData> _OutcomeChannel;

        private readonly List<ActiveEffect> _ActiveEffects = new List<ActiveEffect>();

        /// <summary>
        /// Initializes a new instance of the power-up effect service.
        /// </summary>
        public PowerUpEffectService(
            LevelData levelData,
            LevelStartedEventChannel levelStartedChannel,
            IEventChannel<PowerUpOutcomeEventData> outcomeChannel)
        {
            _LevelData = levelData;
            _LevelStartedChannel = levelStartedChannel;
            _OutcomeChannel = outcomeChannel;
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

            for (int i = 0; i < _ActiveEffects.Count; i++)
            {
                if (ReferenceEquals(_ActiveEffects[i].Effect, effect))
                {
                    return;
                }
            }

            var activeEffect = new ActiveEffect
            {
                Effect = effect,
                Duration = duration,
                RemainingTime = duration
            };

            ApplyEffectToAllTargets(effect);
            _ActiveEffects.Add(activeEffect);
        }

        /// <summary>
        /// Gets the normalized remaining time for an active effect.
        /// </summary>
        public bool TryGetEffectProgress(IPowerUpEffect effect, out float normalizedRemaining)
        {
            for (int i = 0; i < _ActiveEffects.Count; i++)
            {
                var activeEffect = _ActiveEffects[i];
                if (!ReferenceEquals(activeEffect.Effect, effect))
                    continue;

                normalizedRemaining = activeEffect.Duration > 0f
                    ? Mathf.Clamp01(activeEffect.RemainingTime / activeEffect.Duration)
                    : 0f;
                return true;
            }

            normalizedRemaining = 0f;
            return false;
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
                    RaiseEndedOutcome(activeEffect.Effect, PowerUpOutcome.Expired);
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
                RaiseEndedOutcome(_ActiveEffects[i].Effect, PowerUpOutcome.Cleared);
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

        private void RaiseEndedOutcome(IPowerUpEffect effect, PowerUpOutcome outcome)
        {
            if (effect is PowerUpConfigBase config)
            {
                _OutcomeChannel?.Raise(new PowerUpOutcomeEventData(config, outcome, -1));
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
            public float Duration;
            public float RemainingTime;
        }
    }
}
