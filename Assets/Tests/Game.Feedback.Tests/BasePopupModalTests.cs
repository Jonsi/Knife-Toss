using Game.UI.Common;
using Game.UI.Interfaces;
using Game.UI.Services;
using NUnit.Framework;
using UnityEngine;

namespace Game.Feedback.Tests
{
    public class BasePopupModalTests
    {
        [Test]
        public void OnlyNewestVisiblePopupOwnsInputAndClosingItRestoresPreviousOwner()
        {
            var coordinator = new ModalStackCoordinator();
            var pause = new RecordingPauseController();
            TestPopup first = CreatePopup("First", pause, coordinator);
            TestPopup second = CreatePopup("Second", pause, coordinator);

            try
            {
                first.Show();
                Assert.That(first.GetComponent<CanvasGroup>().blocksRaycasts, Is.True);

                second.Show();
                Assert.That(first.GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
                Assert.That(second.GetComponent<CanvasGroup>().blocksRaycasts, Is.True);
                Assert.That(coordinator.State.TopOwner, Is.SameAs(second));

                second.Hide();
                Assert.That(second.gameObject.activeSelf, Is.False);
                Assert.That(first.GetComponent<CanvasGroup>().blocksRaycasts, Is.True);
                Assert.That(coordinator.State.TopOwner, Is.SameAs(first));
            }
            finally
            {
                Object.DestroyImmediate(first.gameObject);
                Object.DestroyImmediate(second.gameObject);
                coordinator.Dispose();
            }
        }

        [Test]
        public void RepeatedShowIsIdempotentAndDisableCleansModalAndPauseClaims()
        {
            var coordinator = new ModalStackCoordinator();
            var pause = new RecordingPauseController();
            TestPopup popup = CreatePopup("Popup", pause, coordinator);

            try
            {
                popup.Show();
                popup.Show();

                Assert.That(coordinator.State.ActiveCount, Is.EqualTo(1));
                Assert.That(pause.AcquireCount, Is.EqualTo(1));

                popup.SimulateDisable();
                popup.gameObject.SetActive(false);

                Assert.That(coordinator.State.ActiveCount, Is.Zero);
                Assert.That(pause.ReleaseCount, Is.EqualTo(1));
                Assert.That(popup.GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(popup.gameObject);
                coordinator.Dispose();
            }
        }

        private static TestPopup CreatePopup(
            string name,
            IGamePauseController pauseController,
            IModalStackCoordinator coordinator)
        {
            var popupObject = new GameObject(name, typeof(RectTransform));
            popupObject.SetActive(false);
            TestPopup popup = popupObject.AddComponent<TestPopup>();
            popup.Construct(pauseController, coordinator);
            return popup;
        }

        private sealed class TestPopup : BasePopup
        {
            public void SimulateDisable()
            {
                base.OnDisable();
            }
        }

        private sealed class RecordingPauseController : IGamePauseController
        {
            public int AcquireCount { get; private set; }
            public int ReleaseCount { get; private set; }

            public void AcquirePause(object requester)
            {
                AcquireCount++;
            }

            public void ReleasePause(object requester)
            {
                ReleaseCount++;
            }
        }
    }
}
