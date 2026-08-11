using System.Linq;
using Game.UI.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Tests
{
    public class TouchTargetValidatorTests
    {
        private GameObject _CanvasObject;
        private RectTransform _CanvasRoot;

        [SetUp]
        public void SetUp()
        {
            _CanvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _CanvasRoot = _CanvasObject.GetComponent<RectTransform>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_CanvasObject);
        }

        [Test]
        public void TargetAtExactlyFortyEightDpMeetsMinimum()
        {
            RectTransform target = CreateButton("Exact Target", new Vector2(144f, 144f));

            TouchTargetValidationResult result = TouchTargetValidator.Validate(target);

            Assert.That(result.Status, Is.EqualTo(TouchTargetValidationStatus.MeetsMinimum));
            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.SizeDp.x, Is.EqualTo(48f).Within(0.001f));
            Assert.That(result.SizeDp.y, Is.EqualTo(48f).Within(0.001f));
        }

        [Test]
        public void EitherUndersizedAxisFailsTheTarget()
        {
            RectTransform target = CreateButton("Narrow Target", new Vector2(120f, 180f));

            TouchTargetValidationResult result = TouchTargetValidator.Validate(target);

            Assert.That(result.Status, Is.EqualTo(TouchTargetValidationStatus.BelowMinimum));
            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.MeetsWidth, Is.False);
            Assert.That(result.MeetsHeight, Is.True);
        }

        [Test]
        public void HierarchyScaleIsIncludedInEffectiveHitArea()
        {
            RectTransform target = CreateButton("Scaled Target", new Vector2(72f, 72f));
            target.localScale = new Vector3(2f, 2f, 1f);

            TouchTargetValidationResult result = TouchTargetValidator.Validate(target);

            Assert.That(result.Status, Is.EqualTo(TouchTargetValidationStatus.MeetsMinimum));
            Assert.That(result.SizeDp.x, Is.EqualTo(48f).Within(0.001f));
            Assert.That(result.SizeDp.y, Is.EqualTo(48f).Within(0.001f));
        }

        [Test]
        public void ExceptionRequiresANonEmptyReviewableReason()
        {
            RectTransform target = CreateButton("Small Exception", new Vector2(90f, 90f));
            TouchTargetException exception = target.gameObject.AddComponent<TouchTargetException>();

            exception.SetReason("   ");
            TouchTargetValidationResult undocumented = TouchTargetValidator.Validate(target);
            Assert.That(undocumented.Status, Is.EqualTo(TouchTargetValidationStatus.BelowMinimum));

            exception.SetReason("Close icon is paired with a full-size system Back action.");
            TouchTargetValidationResult documented = TouchTargetValidator.Validate(target);
            Assert.That(documented.Status, Is.EqualTo(TouchTargetValidationStatus.DocumentedException));
            Assert.That(documented.IsAccepted, Is.True);
            Assert.That(documented.ExceptionReason, Does.Contain("system Back"));
        }

        [Test]
        public void PaddingExpandsRaycastAreaWithoutResizingVisibleTarget()
        {
            RectTransform target = CreateButton("Padded Target", new Vector2(90f, 100f));
            Vector2 originalSize = target.sizeDelta;
            TouchTargetPadding padding = target.gameObject.AddComponent<TouchTargetPadding>();

            RectTransform hitArea = padding.EnsureHitArea();
            TouchTargetValidationResult result = TouchTargetValidator.Validate(target);

            Assert.That(target.sizeDelta, Is.EqualTo(originalSize));
            Assert.That(hitArea, Is.Not.Null);
            Assert.That(hitArea.sizeDelta, Is.EqualTo(new Vector2(144f, 144f)));
            Assert.That(hitArea.GetComponent<Image>().raycastTarget, Is.True);
            Assert.That(hitArea.GetComponent<Image>().color.a, Is.Zero);
            Assert.That(result.Status, Is.EqualTo(TouchTargetValidationStatus.MeetsMinimum));
            Assert.That(result.SizeDp.x, Is.EqualTo(48f));
            Assert.That(result.SizeDp.y, Is.EqualTo(48f));

            Assert.That(padding.EnsureHitArea(), Is.SameAs(hitArea));
            Assert.That(target.childCount, Is.EqualTo(1));
        }

        [Test]
        public void ScanIncludesVisibleDisabledActionsButExcludesInactiveOnesByDefault()
        {
            RectTransform disabledTarget = CreateButton("Visible Disabled", new Vector2(144f, 144f));
            disabledTarget.GetComponent<Button>().interactable = false;
            RectTransform inactiveTarget = CreateButton("Inactive", new Vector2(144f, 144f));
            inactiveTarget.gameObject.SetActive(false);
            TouchTargetValidator validator = _CanvasObject.AddComponent<TouchTargetValidator>();
            validator.Configure(_CanvasRoot);

            var results = validator.ValidateTargets();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.Single().Target, Is.SameAs(disabledTarget));
            Assert.That(results.Single().Status, Is.EqualTo(TouchTargetValidationStatus.MeetsMinimum));
        }

        [TestCase(0f, 3f)]
        [TestCase(48f, 0f)]
        [TestCase(float.NaN, 3f)]
        public void InvalidValidationConfigurationIsReported(float minimumDp, float pixelsPerDp)
        {
            RectTransform target = CreateButton("Target", new Vector2(144f, 144f));

            TouchTargetValidationResult result = TouchTargetValidator.Validate(target, minimumDp, pixelsPerDp);

            Assert.That(result.Status, Is.EqualTo(TouchTargetValidationStatus.InvalidConfiguration));
            Assert.That(result.IsAccepted, Is.False);
        }

        private RectTransform CreateButton(string name, Vector2 size)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Button));
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.SetParent(_CanvasRoot, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            return rectTransform;
        }
    }
}
