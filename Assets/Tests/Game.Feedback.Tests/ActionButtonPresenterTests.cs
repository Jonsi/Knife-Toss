using System.Reflection;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.UI.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Feedback.Tests
{
    public class ActionButtonPresenterTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void StateModelUsesBlockedLoadingDisabledPressedReadyPrecedence()
        {
            var model = new ActionButtonStateModel();

            Assert.That(model.State, Is.EqualTo(ActionButtonPresentationState.Ready));
            model.SetPressed(true);
            Assert.That(model.State, Is.EqualTo(ActionButtonPresentationState.Pressed));

            model.SetAvailable(false);
            Assert.That(model.State, Is.EqualTo(ActionButtonPresentationState.Disabled));

            model.SetAvailable(true);
            Assert.That(model.TryBeginSubmission(), Is.True);
            model.SetAvailable(false);
            Assert.That(model.State, Is.EqualTo(ActionButtonPresentationState.Loading));

            model.SetBlocked(true);
            Assert.That(model.State, Is.EqualTo(ActionButtonPresentationState.Blocked));

            model.ResetTransient();
            Assert.That(model.State, Is.EqualTo(ActionButtonPresentationState.Disabled));
        }

        [Test]
        public void StateModelRejectsDuplicateSubmissionUntilCompletion()
        {
            var model = new ActionButtonStateModel();

            Assert.That(model.TryBeginSubmission(), Is.True);
            Assert.That(model.TryBeginSubmission(), Is.False);

            model.CompleteSubmission();
            Assert.That(model.TryBeginSubmission(), Is.True);
        }

        [Test]
        public void LoadingBlocksInteractionWithoutOverwritingBaseInteractability()
        {
            GameObject root = CreateButton(out Button button, out ActionButtonPresenter presenter);
            try
            {
                CanvasGroup group = root.GetComponent<CanvasGroup>();
                Assert.That(button.interactable, Is.True);
                Assert.That(presenter.TryBeginSubmission(), Is.True);
                Assert.That(presenter.TryBeginSubmission(), Is.False);
                Assert.That(group.interactable, Is.False);
                Assert.That(button.interactable, Is.True);

                presenter.CompleteSubmission();

                Assert.That(group.interactable, Is.True);
                Assert.That(button.interactable, Is.True);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void BlockedPresentationRaisesTypedFeedbackAndExpires()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            GameObject root = CreateButton(out _, out ActionButtonPresenter presenter, channel);
            FeedbackEventData received = default;
            bool wasRaised = false;
            void Handler(FeedbackEventData data)
            {
                received = data;
                wasRaised = true;
            }

            try
            {
                channel.Subscribe(Handler);
                var feedback = new FeedbackEventData(
                    FeedbackKind.InsufficientFunds,
                    "store.insufficient_funds",
                    "power.super_knife");

                presenter.PresentBlocked(feedback);

                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Blocked));
                Assert.That(wasRaised, Is.True);
                Assert.That(received.DuplicateKey, Is.EqualTo(feedback.DuplicateKey));

                presenter.AdvancePresentation(1f);

                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));
            }
            finally
            {
                channel.Unsubscribe(Handler);
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(channel);
            }
        }

        [Test]
        public void DisableCleansTransientStateAndRestoresVisualAndInteractionState()
        {
            GameObject root = CreateButton(out _, out ActionButtonPresenter presenter);
            var pointer = new PointerEventData(null);
            var rect = root.GetComponent<RectTransform>();
            Vector3 originalScale = new Vector3(1.2f, 1.2f, 1f);
            rect.localScale = originalScale;

            try
            {
                presenter.OnPointerDown(pointer);
                presenter.AdvancePresentation(0.25f);
                Assert.That(rect.localScale, Is.Not.EqualTo(originalScale));
                Assert.That(presenter.TryBeginSubmission(), Is.True);

                InvokePrivate(presenter, "OnDisable");

                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));
                Assert.That(root.GetComponent<CanvasGroup>().interactable, Is.True);
                Assert.That(rect.localScale, Is.EqualTo(originalScale));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void DisabledAvailabilityPreventsSubmissionAndUsesDisabledState()
        {
            GameObject root = CreateButton(out Button button, out ActionButtonPresenter presenter);
            try
            {
                button.interactable = false;
                presenter.AdvancePresentation(0f);

                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Disabled));
                Assert.That(presenter.TryBeginSubmission(), Is.False);
                Assert.That(root.GetComponent<CanvasGroup>().alpha, Is.LessThan(1f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateButton(
            out Button button,
            out ActionButtonPresenter presenter,
            FeedbackEventChannel channel = null)
        {
            var root = new GameObject(
                "Action Button",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            button = root.GetComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            presenter = root.AddComponent<ActionButtonPresenter>();
            presenter.Configure(button, channel);
            return root;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.That(method, Is.Not.Null, $"Private method {methodName} was not found.");
            method.Invoke(target, null);
        }
    }
}
