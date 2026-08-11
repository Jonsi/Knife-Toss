using Game.UI.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Game.UI.Tests
{
    public class SafeAreaRootTests
    {
        [TestCase(1080, 1920, 0, 0, 1080, 1920)]
        [TestCase(1080, 2340, 0, 80, 1080, 2180)]
        [TestCase(1080, 2400, 0, 96, 1080, 2224)]
        [TestCase(1600, 2560, 24, 32, 1552, 2464)]
        public void DeviceProfilesProduceNormalizedAnchorsInsideTheScreen(
            int screenWidth,
            int screenHeight,
            int safeX,
            int safeY,
            int safeWidth,
            int safeHeight)
        {
            bool success = SafeAreaUtility.TryGetNormalizedAnchors(
                new Rect(safeX, safeY, safeWidth, safeHeight),
                new Vector2Int(screenWidth, screenHeight),
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(success, Is.True);
            Assert.That(anchorMin.x, Is.EqualTo((float)safeX / screenWidth).Within(0.0001f));
            Assert.That(anchorMin.y, Is.EqualTo((float)safeY / screenHeight).Within(0.0001f));
            Assert.That(anchorMax.x, Is.EqualTo((float)(safeX + safeWidth) / screenWidth).Within(0.0001f));
            Assert.That(anchorMax.y, Is.EqualTo((float)(safeY + safeHeight) / screenHeight).Within(0.0001f));
            Assert.That(anchorMin.x, Is.InRange(0f, 1f));
            Assert.That(anchorMin.y, Is.InRange(0f, 1f));
            Assert.That(anchorMax.x, Is.InRange(0f, 1f));
            Assert.That(anchorMax.y, Is.InRange(0f, 1f));
        }

        [Test]
        public void GeometryOutsideTheScreenIsClampedBeforeNormalization()
        {
            bool success = SafeAreaUtility.TryGetNormalizedAnchors(
                new Rect(-20f, 40f, 1140f, 1960f),
                new Vector2Int(1080, 1920),
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(success, Is.True);
            Assert.That(anchorMin, Is.EqualTo(new Vector2(0f, 40f / 1920f)).Using(Vector2ComparerWithEqualsOperator.Instance));
            Assert.That(anchorMax, Is.EqualTo(Vector2.one).Using(Vector2ComparerWithEqualsOperator.Instance));
        }

        [TestCase(0, 1920)]
        [TestCase(1080, 0)]
        [TestCase(-1, 1920)]
        public void InvalidScreenGeometryIsRejectedWithoutDividing(int width, int height)
        {
            bool success = SafeAreaUtility.TryGetNormalizedAnchors(
                new Rect(0f, 0f, 100f, 100f),
                new Vector2Int(width, height),
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(success, Is.False);
            Assert.That(anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(anchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void RootAppliesSafeAnchorsAndRepeatedApplicationDoesNotDrift()
        {
            var root = new GameObject("Safe Area Root", typeof(RectTransform));
            SafeAreaRoot safeAreaRoot = root.AddComponent<SafeAreaRoot>();
            RectTransform rectTransform = root.GetComponent<RectTransform>();

            try
            {
                rectTransform.offsetMin = new Vector2(15f, 20f);
                rectTransform.offsetMax = new Vector2(-15f, -20f);
                var safeArea = new Rect(0f, 100f, 1080f, 2200f);
                var screenSize = new Vector2Int(1080, 2400);

                Assert.That(safeAreaRoot.Apply(safeArea, screenSize), Is.True);
                Vector2 firstMin = rectTransform.anchorMin;
                Vector2 firstMax = rectTransform.anchorMax;
                Assert.That(firstMin, Is.EqualTo(new Vector2(0f, 100f / 2400f)).Using(Vector2ComparerWithEqualsOperator.Instance));
                Assert.That(firstMax, Is.EqualTo(new Vector2(1f, 2300f / 2400f)).Using(Vector2ComparerWithEqualsOperator.Instance));
                Assert.That(rectTransform.offsetMin, Is.EqualTo(Vector2.zero));
                Assert.That(rectTransform.offsetMax, Is.EqualTo(Vector2.zero));

                Assert.That(safeAreaRoot.Apply(safeArea, screenSize), Is.True);
                Assert.That(rectTransform.anchorMin, Is.EqualTo(firstMin));
                Assert.That(rectTransform.anchorMax, Is.EqualTo(firstMax));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SelectedEdgesPreserveUnmanagedAnchors()
        {
            var root = new GameObject("Safe Area Root", typeof(RectTransform));
            SafeAreaRoot safeAreaRoot = root.AddComponent<SafeAreaRoot>();
            RectTransform rectTransform = root.GetComponent<RectTransform>();

            try
            {
                rectTransform.anchorMin = new Vector2(0.2f, 0.3f);
                rectTransform.anchorMax = new Vector2(0.8f, 0.7f);
                safeAreaRoot.SetEdges(SafeAreaEdges.Top | SafeAreaEdges.Bottom);

                Assert.That(safeAreaRoot.Apply(
                    new Rect(40f, 100f, 1000f, 2200f),
                    new Vector2Int(1080, 2400)), Is.True);

                Assert.That(rectTransform.anchorMin.x, Is.EqualTo(0.2f));
                Assert.That(rectTransform.anchorMax.x, Is.EqualTo(0.8f));
                Assert.That(rectTransform.anchorMin.y, Is.EqualTo(100f / 2400f).Within(0.0001f));
                Assert.That(rectTransform.anchorMax.y, Is.EqualTo(2300f / 2400f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
