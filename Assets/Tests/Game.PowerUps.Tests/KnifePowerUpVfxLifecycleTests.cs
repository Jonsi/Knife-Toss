using System.Reflection;
using Game.Entities.Knife.Views;
using Game.PowerUps.Configs;
using Game.PowerUps.Interfaces;
using NUnit.Framework;
using UnityEngine;

namespace Game.PowerUps.Tests
{
    public class KnifePowerUpVfxLifecycleTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject _KnifeObject;
        private GameObject _VfxPrefab;
        private GameObject _RuntimeVfx;
        private GameObject _CameraObject;
        private TestPowerUpConfig _Config;

        [TearDown]
        public void TearDown()
        {
            DestroyImmediateIfPresent(_KnifeObject);
            DestroyImmediateIfPresent(_VfxPrefab);
            DestroyImmediateIfPresent(_RuntimeVfx);
            DestroyImmediateIfPresent(_CameraObject);
            DestroyImmediateIfPresent(_Config);
        }

        [Test]
        public void PowerUpBounceDestroysAttachedVfx()
        {
            var knife = CreateKnifeWithVfx(destroyVfxOnHit: true);

            knife.Bounce(playFailFeedback: false);

            Assert.That(GetPrivate<GameObject>(knife, "_KnifePowerUpVfxInstance"), Is.Null);
            Assert.That(_RuntimeVfx == null, Is.True);
        }

        [Test]
        public void ResetKnifeClearsPowerUpStateWithoutRecreatingVfx()
        {
            var knife = CreateKnifeWithVfx(destroyVfxOnHit: true);
            SetPrivate(knife, "_IsSuperKnife", true);

            knife.ResetKnife();

            Assert.That(knife.IsSuperKnife, Is.False);
            Assert.That(knife.SourcePowerUpConfig, Is.Null);
            Assert.That(GetPrivate<GameObject>(knife, "_KnifePowerUpVfxInstance"), Is.Null);
            Assert.That(_RuntimeVfx == null, Is.True);
        }

        [Test]
        public void OutOfViewCheckReacquiresMainCamera()
        {
            _KnifeObject = new GameObject("Test Knife");
            var knife = _KnifeObject.AddComponent<KnifeView>();
            InvokePrivate(knife, "Awake");
            SetPrivate<Camera>(knife, "_MainCamera", null);

            _CameraObject = new GameObject("Test Main Camera");
            _CameraObject.tag = "MainCamera";
            var camera = _CameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            knife.Launch(Vector2.up, 1f);
            knife.transform.position = new Vector3(0f, 100f, 0f);
            InvokePrivate(knife, "CheckOutOfView");

            Assert.That(GetPrivate<Camera>(knife, "_MainCamera"), Is.Not.Null);
            Assert.That(GetPrivate<bool>(knife, "_HasReportedOutOfView"), Is.True);
        }

        private KnifeView CreateKnifeWithVfx(bool destroyVfxOnHit)
        {
            _KnifeObject = new GameObject("Test Knife");
            var knife = _KnifeObject.AddComponent<KnifeView>();
            InvokePrivate(knife, "Awake");

            _Config = ScriptableObject.CreateInstance<TestPowerUpConfig>();
            _VfxPrefab = new GameObject("Test VFX Prefab");
            SetBackingField(_Config, nameof(PowerUpConfigBase.VfxPrefab), _VfxPrefab);
            SetBackingField(_Config, nameof(PowerUpConfigBase.DestroyVfxOnHit), destroyVfxOnHit);

            _RuntimeVfx = new GameObject("Test Runtime VFX");
            SetPrivate(knife, "_SourcePowerUpConfig", _Config);
            SetPrivate(knife, "_KnifePowerUpVfxInstance", _RuntimeVfx);
            return knife;
        }

        private static void SetBackingField<T>(object target, string propertyName, T value)
        {
            var field = target.GetType().BaseType?.GetField($"<{propertyName}>k__BackingField", PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Backing field for {propertyName} was not found.");
            field.SetValue(target, value);
        }

        private static void SetPrivate<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Private field {fieldName} was not found.");
            field.SetValue(target, value);
        }

        private static T GetPrivate<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Private field {fieldName} was not found.");
            return (T)field.GetValue(target);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.That(method, Is.Not.Null, $"Private method {methodName} was not found.");
            method.Invoke(target, null);
        }

        private static void DestroyImmediateIfPresent(Object value)
        {
            if (value != null)
            {
                Object.DestroyImmediate(value);
            }
        }

        private sealed class TestPowerUpConfig : PowerUpConfigBase
        {
            public override bool Execute(IPowerUpExecutionContext context) => true;
        }
    }
}
