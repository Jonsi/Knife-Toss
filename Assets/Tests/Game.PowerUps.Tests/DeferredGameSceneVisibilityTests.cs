using System.Reflection;
using Game.Flow.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.PowerUps.Tests
{
    public class DeferredGameSceneVisibilityTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void HiddenPreloadSuppressesInitialAndLateSpawnedRenderersUntilReveal()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject gateObject = null;
            SpriteRenderer initialRenderer = null;
            SpriteRenderer lateRenderer = null;
            try
            {
                gateObject = new GameObject("Visibility Gate");
                var gate = gateObject.AddComponent<DeferredGameSceneVisibility>();
                SetPrivate(gate, "_RootsToHide", new[] { gateObject });

                initialRenderer = CreateRenderer(scene, "Initial Renderer");

                DeferredGameSceneVisibility.BeginHiddenLoad();
                InvokePrivate(gate, "Awake");
                Assert.That(initialRenderer.enabled, Is.False);

                lateRenderer = CreateRenderer(scene, "Late Renderer");
                InvokePrivate(gate, "LateUpdate");
                Assert.That(lateRenderer.enabled, Is.False);

                initialRenderer.enabled = true;
                InvokePrivate(gate, "LateUpdate");
                Assert.That(initialRenderer.enabled, Is.False);

                DeferredGameSceneVisibility.RevealLoadedScene();
                Assert.That(initialRenderer.enabled, Is.True);
                Assert.That(lateRenderer.enabled, Is.True);
            }
            finally
            {
                DeferredGameSceneVisibility.ResetHiddenLoadState();
                DestroyImmediateIfPresent(lateRenderer != null ? lateRenderer.gameObject : null);
                DestroyImmediateIfPresent(initialRenderer != null ? initialRenderer.gameObject : null);
                DestroyImmediateIfPresent(gateObject);
            }
        }

        private static SpriteRenderer CreateRenderer(Scene scene, string name)
        {
            var gameObject = new GameObject(name);
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            return gameObject.AddComponent<SpriteRenderer>();
        }

        private static void DestroyImmediateIfPresent(Object value)
        {
            if (value != null)
            {
                Object.DestroyImmediate(value);
            }
        }

        private static void SetPrivate<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Private field {fieldName} was not found.");
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.That(method, Is.Not.Null, $"Private method {methodName} was not found.");
            method.Invoke(target, null);
        }
    }
}
