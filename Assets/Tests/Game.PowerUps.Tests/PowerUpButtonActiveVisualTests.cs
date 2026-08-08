using System.Reflection;
using Game.PowerUps.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.PowerUps.Tests
{
    public class PowerUpButtonActiveVisualTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject _Root;
        private Material _DefaultMaterial;
        private Material _ActiveMaterial;
        private Image _Icon;
        private PowerUpButtonView _View;

        [SetUp]
        public void SetUp()
        {
            _Root = new GameObject("Power Up Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var button = _Root.GetComponent<Button>();

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(_Root.transform, false);
            _Icon = iconObject.GetComponent<Image>();

            Shader shader = Shader.Find("UI/Default");
            _DefaultMaterial = new Material(shader) { name = "Default Test Material" };
            _ActiveMaterial = new Material(shader) { name = "Active Test Material" };
            _Icon.material = _DefaultMaterial;
            button.targetGraphic = _Icon;

            _View = _Root.AddComponent<PowerUpButtonView>();
            SetPrivate(_View, "_Button", button);
            SetPrivate(_View, "_Icon", _Icon);
            SetPrivate(_View, "_DefaultIconMaterial", _DefaultMaterial);
            SetPrivate(_View, "_ActiveIconMaterial", _ActiveMaterial);
            SetPrivate(_View, "_SelectedColor", Color.white);
            SetPrivate(_View, "_UnselectedColor", Color.white);
            SetPrivate(_View, "_UnavailableColor", Color.gray);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_Root);
            Object.DestroyImmediate(_DefaultMaterial);
            Object.DestroyImmediate(_ActiveMaterial);
        }

        [Test]
        public void SelectedButtonUsesActiveMaterialAndRestoresDefaultWhenDeselected()
        {
            _View.SetCount(3);
            _View.UpdateSelected(true);

            Assert.That(_Icon.material, Is.SameAs(_ActiveMaterial));

            _View.UpdateSelected(false);

            Assert.That(_Icon.material, Is.SameAs(_DefaultMaterial));
        }

        [Test]
        public void ActiveTimedEffectAtZeroStockRemainsVisuallyActive()
        {
            _View.SetCount(0);
            _View.SetEffectProgress(true, 0.5f);

            Assert.That(_Icon.material, Is.SameAs(_ActiveMaterial));
            Assert.That(_Icon.color, Is.EqualTo(Color.white));

            _View.SetEffectProgress(false, 0f);

            Assert.That(_Icon.material, Is.SameAs(_DefaultMaterial));
            Assert.That(_Icon.color, Is.EqualTo(Color.gray));
        }

        [Test]
        public void ActivePulseScalesUniformlyAroundTheIconPivot()
        {
            _Icon.rectTransform.anchoredPosition = new Vector2(12f, -7f);
            Vector2 originalPosition = _Icon.rectTransform.anchoredPosition;

            _View.SetCount(3);
            _View.UpdateSelected(true);
            SetPrivate(_View, "_ActivePulseElapsed", 0.4f);
            InvokePrivate(_View, "Update");

            Assert.That(_Icon.rectTransform.anchoredPosition, Is.EqualTo(originalPosition));
            Assert.That(_Icon.rectTransform.localScale.x, Is.GreaterThan(1f));
            Assert.That(_Icon.rectTransform.localScale.x,
                Is.EqualTo(_Icon.rectTransform.localScale.y).Within(0.0001f));
        }

        private static void SetPrivate<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Private field {fieldName} was not found.");
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.That(method, Is.Not.Null, $"Private method {methodName} was not found.");
            method.Invoke(target, null);
        }
    }
}
