using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Entities.Target.Interfaces;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Level;
using Game.KnifeThrower.Interfaces;
using Game.Persistence.Data;
using Game.Persistence.Interfaces;
using Game.Persistence.Services;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using Game.PowerUps.Events;
using Game.PowerUps.Interfaces;
using Game.PowerUps.Services;
using NUnit.Framework;
using UnityEngine;

namespace Game.PowerUps.Tests
{
    public class PowerUpServiceTests
    {
        private MemoryPlayerDataStore _Store;
        private PlayerDataService _PlayerData;
        private TestThrowerStateManager _ThrowerState;
        private PowerUpEffectService _Effects;
        private PowerUpService _Service;
        private PowerUpActivatedEventChannel _Activated;
        private PowerUpUsedEventChannel _Used;
        private PowerUpCancelledEventChannel _Cancelled;
        private PowerUpOutcomeEventChannel _Outcomes;
        private LevelStartedEventChannel _LevelStarted;
        private readonly List<PowerUpOutcomeEventData> _ObservedOutcomes = new();
        private readonly List<ScriptableObject> _CreatedObjects = new();

        [SetUp]
        public void SetUp()
        {
            _Store = new MemoryPlayerDataStore();
            _PlayerData = new PlayerDataService(_Store, new GameData());
            _PlayerData.Initialize();
            _ThrowerState = new TestThrowerStateManager();

            _Activated = CreateChannel<PowerUpActivatedEventChannel>();
            _Used = CreateChannel<PowerUpUsedEventChannel>();
            _Cancelled = CreateChannel<PowerUpCancelledEventChannel>();
            _Outcomes = CreateChannel<PowerUpOutcomeEventChannel>();
            _LevelStarted = CreateChannel<LevelStartedEventChannel>();
            _Outcomes.Subscribe(_ObservedOutcomes.Add);

            _Effects = new PowerUpEffectService(new LevelData(null), _LevelStarted, _Outcomes);
            _Service = new PowerUpService(
                _PlayerData,
                _Activated,
                _Used,
                _Cancelled,
                _Outcomes,
                _LevelStarted,
                _Effects,
                _ThrowerState);
            _Effects.Start();
            _Service.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _Service.Dispose();
            _Effects.Dispose();
            for (int i = 0; i < _CreatedObjects.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(_CreatedObjects[i]);
            }

            _CreatedObjects.Clear();
            _ObservedOutcomes.Clear();
        }

        [Test]
        public void ProjectilePowerUp_ConsumesOnlyAfterSuccessfulThrow()
        {
            var config = CreateConfig<TestProjectileConfig>("SuperKnife");
            SetInventory(config, 2);

            Assert.That(_Service.TryActivate(config), Is.True);
            Assert.That(_Service.HasActivePowerUp, Is.True);
            Assert.That(_ThrowerState.IsProjectilePowerUpActive, Is.True);
            Assert.That(GetInventory(config), Is.EqualTo(2));

            Assert.That(_Service.TryHandleThrow(new PowerUpThrowContext(null, null, null, null)), Is.True);
            Assert.That(_Service.HasActivePowerUp, Is.False);
            Assert.That(_ThrowerState.IsProjectilePowerUpActive, Is.False);
            Assert.That(GetInventory(config), Is.EqualTo(1));
            Assert.That(_Store.SaveCount, Is.EqualTo(1));
            Assert.That(OutcomeSequence(), Is.EqualTo(new[]
            {
                PowerUpOutcome.Activated,
                PowerUpOutcome.Consumed
            }));
        }

        [Test]
        public void WorldEffect_ConsumesImmediatelyAndDoesNotBecomeSelectedRuntimeState()
        {
            var config = CreateConfig<TestWorldEffectConfig>("TimeDilator");
            SetInventory(config, 2);

            Assert.That(_Service.TryActivate(config), Is.True);

            Assert.That(_Service.HasActivePowerUp, Is.False);
            Assert.That(_ThrowerState.IsProjectilePowerUpActive, Is.False);
            Assert.That(GetInventory(config), Is.EqualTo(1));
            Assert.That(_Store.SaveCount, Is.EqualTo(1));
            Assert.That(OutcomeSequence(), Is.EqualTo(new[]
            {
                PowerUpOutcome.Activated,
                PowerUpOutcome.Consumed
            }));
        }

        [Test]
        public void ActiveWorldEffect_BlocksReactivationWithoutConsumingInventory()
        {
            var config = CreateConfig<TestWorldEffectConfig>("TimeDilator");
            SetInventory(config, 2);

            Assert.That(_Service.TryActivate(config), Is.True);
            Assert.That(_Service.TryActivate(config), Is.False);

            Assert.That(GetActiveEffectCount(), Is.EqualTo(1));
            Assert.That(GetInventory(config), Is.EqualTo(1));
            Assert.That(_Store.SaveCount, Is.EqualTo(1));
            Assert.That(_ObservedOutcomes[^1].Outcome, Is.EqualTo(PowerUpOutcome.Blocked));
            Assert.That(_Service.TryGetWorldEffectProgress(config, out float progress), Is.True);
            Assert.That(progress, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void UnavailablePowerUp_EmitsTypedFailureWithoutChangingState()
        {
            var config = CreateConfig<TestProjectileConfig>("SuperKnife");
            SetInventory(config, 0);

            Assert.That(_Service.TryActivate(config), Is.False);

            Assert.That(_Service.HasActivePowerUp, Is.False);
            Assert.That(_ThrowerState.IsProjectilePowerUpActive, Is.False);
            Assert.That(_ObservedOutcomes, Has.Count.EqualTo(1));
            Assert.That(_ObservedOutcomes[0].Outcome, Is.EqualTo(PowerUpOutcome.Unavailable));
            Assert.That(_ObservedOutcomes[0].FailureReason, Is.Not.Empty);
        }

        [Test]
        public void CancelledProjectile_PreservesInventoryAndRestoresNormalThrower()
        {
            var config = CreateConfig<TestProjectileConfig>("SuperKnife");
            SetInventory(config, 2);
            Assert.That(_Service.TryActivate(config), Is.True);

            Assert.That(_Service.TryCancelActive(), Is.True);

            Assert.That(_Service.HasActivePowerUp, Is.False);
            Assert.That(_ThrowerState.IsProjectilePowerUpActive, Is.False);
            Assert.That(GetInventory(config), Is.EqualTo(2));
            Assert.That(_ObservedOutcomes[^1].Outcome, Is.EqualTo(PowerUpOutcome.Cancelled));
        }

        [Test]
        public void LevelStart_ClearsProjectileStateWithoutConsumingInventory()
        {
            var config = CreateConfig<TestProjectileConfig>("SuperKnife");
            SetInventory(config, 2);
            Assert.That(_Service.TryActivate(config), Is.True);

            _LevelStarted.Raise(new LevelStartedEventData());

            Assert.That(_Service.HasActivePowerUp, Is.False);
            Assert.That(_ThrowerState.IsProjectilePowerUpActive, Is.False);
            Assert.That(_ThrowerState.DeactivationCount, Is.EqualTo(1));
            Assert.That(GetInventory(config), Is.EqualTo(2));
            Assert.That(_ObservedOutcomes[^1].Outcome, Is.EqualTo(PowerUpOutcome.Cleared));
        }

        [Test]
        public void ExpiredWorldEffect_EmitsExpiredOutcome()
        {
            var config = CreateConfig<TestWorldEffectConfig>("TimeDilator");
            SetInventory(config, 1);
            Assert.That(_Service.TryActivate(config), Is.True);

            ForceActiveEffectsToExpire();
            _Effects.Tick();

            Assert.That(_ObservedOutcomes[^1].Outcome, Is.EqualTo(PowerUpOutcome.Expired));
            Assert.That(_ObservedOutcomes[^1].Config, Is.SameAs(config));
        }

        private T CreateChannel<T>() where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            _CreatedObjects.Add(instance);
            return instance;
        }

        private T CreateConfig<T>(string inventoryKey) where T : PowerUpConfigBase
        {
            var config = ScriptableObject.CreateInstance<T>();
            typeof(PowerUpConfigBase)
                .GetField("<InventoryKey>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(config, inventoryKey);
            _CreatedObjects.Add(config);
            return config;
        }

        private void SetInventory(PowerUpConfigBase config, int count)
        {
            _PlayerData.EnsurePowerUpInventory().SetCount(config.InventoryKey, count);
            _Store.SaveCount = 0;
        }

        private int GetInventory(PowerUpConfigBase config) =>
            _PlayerData.EnsurePowerUpInventory().GetCount(config.InventoryKey);

        private PowerUpOutcome[] OutcomeSequence() =>
            _ObservedOutcomes.ConvertAll(value => value.Outcome).ToArray();

        private int GetActiveEffectCount()
        {
            var field = typeof(PowerUpEffectService)
                .GetField("_ActiveEffects", BindingFlags.Instance | BindingFlags.NonPublic);
            return (field?.GetValue(_Effects) as ICollection)?.Count ?? -1;
        }

        private void ForceActiveEffectsToExpire()
        {
            var field = typeof(PowerUpEffectService)
                .GetField("_ActiveEffects", BindingFlags.Instance | BindingFlags.NonPublic);
            var effects = field?.GetValue(_Effects) as IEnumerable;
            Assert.That(effects, Is.Not.Null);
            foreach (var effect in effects)
            {
                effect.GetType()
                    .GetField("RemainingTime", BindingFlags.Instance | BindingFlags.Public)
                    ?.SetValue(effect, 0f);
            }
        }

        public sealed class TestProjectileConfig : PowerUpConfigBase, IProjectilePowerUp, ICancellablePowerUp
        {
            public override bool Execute(IPowerUpExecutionContext context) => true;
            public void OnCancelled() { }
        }

        public sealed class TestWorldEffectConfig : PowerUpConfigBase, IWorldEffectPowerUp, IPowerUpEffect
        {
            public float EffectDuration => 5f;
            public override bool Execute(IPowerUpExecutionContext context) => false;
            public void ApplyWorldEffect(PowerUpEffectService effectService) =>
                effectService.ApplyEffect(this, EffectDuration);
            public void Apply(ITargetView target) => target?.SetSpeedMultiplier(0.3f);
            public void Remove(ITargetView target) => target?.ClearRotationModifiers();
        }

        private sealed class TestThrowerStateManager : IThrowerStateManager
        {
            public bool IsProjectilePowerUpActive { get; private set; }
            public int DeactivationCount { get; private set; }

            public void ActivateProjectilePowerUp(PowerUpConfigBase config) =>
                IsProjectilePowerUpActive = true;

            public void DeactivateProjectilePowerUp()
            {
                IsProjectilePowerUpActive = false;
                DeactivationCount++;
            }

            public void CancelProjectilePowerUp() => DeactivateProjectilePowerUp();
        }

        private sealed class MemoryPlayerDataStore : IPlayerDataStore
        {
            private PlayerData _Data = new();
            public int SaveCount { get; set; }

            public PlayerData Load() => _Data;

            public void Save(PlayerData data)
            {
                _Data = data;
                SaveCount++;
            }
        }
    }
}
