using System.Reflection;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Feedback.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Feedback.Tests
{
    public class FeedbackPresentationQueueTests
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void FirstRequestPresentsImmediatelyAndHigherPriorityInterrupts()
        {
            var queue = new FeedbackPresentationQueue();
            FeedbackEventData success = Event(FeedbackKind.Success, "action.success");
            FeedbackEventData failure = Event(FeedbackKind.Failure, "level.failed");

            Assert.That(queue.Enqueue(success, 0d), Is.EqualTo(FeedbackEnqueueResult.Presented));
            Assert.That(queue.Enqueue(failure, 0.1d), Is.EqualTo(FeedbackEnqueueResult.Interrupted));
            Assert.That(queue.Active.DuplicateKey, Is.EqualTo(failure.DuplicateKey));
            Assert.That(queue.PendingCount, Is.Zero);
        }

        [Test]
        public void EqualPriorityQueuesInFifoOrder()
        {
            var queue = new FeedbackPresentationQueue();
            FeedbackEventData active = Event(FeedbackKind.Failure, "level.failed");
            FeedbackEventData purchase = Event(FeedbackKind.Purchase, "store.purchase", "one");
            FeedbackEventData reward = Event(FeedbackKind.Reward, "reward.granted", "two");

            queue.Enqueue(active, 0d);
            queue.Enqueue(purchase, 0.1d);
            queue.Enqueue(reward, 0.2d);

            Assert.That(queue.Pending[0].DuplicateKey, Is.EqualTo(purchase.DuplicateKey));
            Assert.That(queue.Pending[1].DuplicateKey, Is.EqualTo(reward.DuplicateKey));

            Assert.That(queue.CompleteActive(1d, out FeedbackEventData next), Is.True);
            Assert.That(next.DuplicateKey, Is.EqualTo(purchase.DuplicateKey));
        }

        [Test]
        public void PendingQueueOrdersByPriorityWithoutReorderingEquals()
        {
            var queue = new FeedbackPresentationQueue();
            FeedbackEventData active = Event(FeedbackKind.Failure, "level.failed");
            FeedbackEventData navigation = Event(FeedbackKind.Navigation, "navigation.open");
            FeedbackEventData success = Event(FeedbackKind.Success, "action.success");
            FeedbackEventData blocked = Event(FeedbackKind.Blocked, "action.blocked");

            queue.Enqueue(active, 0d);
            queue.Enqueue(navigation, 0.1d);
            queue.Enqueue(success, 0.2d);
            queue.Enqueue(blocked, 0.3d);

            Assert.That(queue.Pending[0].Priority, Is.EqualTo(FeedbackPriority.High));
            Assert.That(queue.Pending[1].Priority, Is.EqualTo(FeedbackPriority.Normal));
            Assert.That(queue.Pending[2].Priority, Is.EqualTo(FeedbackPriority.Low));
        }

        [Test]
        public void ActiveAndPendingDuplicatesAreSuppressed()
        {
            var queue = new FeedbackPresentationQueue();
            FeedbackEventData active = Event(FeedbackKind.Failure, "level.failed");
            FeedbackEventData reward = Event(FeedbackKind.Reward, "reward.granted", "daily");

            queue.Enqueue(active, 0d);
            Assert.That(
                queue.Enqueue(active, 0.1d),
                Is.EqualTo(FeedbackEnqueueResult.SuppressedDuplicate));

            queue.Enqueue(reward, 0.2d);
            Assert.That(
                queue.Enqueue(reward, 0.3d),
                Is.EqualTo(FeedbackEnqueueResult.SuppressedDuplicate));
            Assert.That(queue.PendingCount, Is.EqualTo(1));
        }

        [Test]
        public void RecentlyPresentedDuplicateIsSuppressedUntilExactCooldownBoundary()
        {
            var queue = new FeedbackPresentationQueue();
            FeedbackEventData success = Event(FeedbackKind.Success, "action.success");

            queue.Enqueue(success, 0d);
            queue.CompleteActive(0.1d, out _);

            Assert.That(
                queue.Enqueue(success, success.CooldownSeconds - 0.001d),
                Is.EqualTo(FeedbackEnqueueResult.SuppressedCooldown));
            Assert.That(
                queue.Enqueue(success, success.CooldownSeconds),
                Is.EqualTo(FeedbackEnqueueResult.Presented));
        }

        [Test]
        public void FullQueueReplacesNewestLowestPriorityOnlyForHigherPriority()
        {
            var queue = new FeedbackPresentationQueue();
            queue.Enqueue(Event(FeedbackKind.Failure, "level.failed"), 0d);
            for (int i = 0; i < FeedbackEventRules.PendingQueueCapacity; i++)
            {
                queue.Enqueue(Event(FeedbackKind.Navigation, "navigation.open", i.ToString()), i + 1d);
            }

            FeedbackEventData incoming = Event(FeedbackKind.Success, "action.success");
            Assert.That(queue.Enqueue(incoming, 20d), Is.EqualTo(FeedbackEnqueueResult.Queued));
            Assert.That(queue.PendingCount, Is.EqualTo(FeedbackEventRules.PendingQueueCapacity));
            Assert.That(queue.Pending[0].DuplicateKey, Is.EqualTo(incoming.DuplicateKey));
            Assert.That(queue.Pending[^1].ContextId, Is.EqualTo("6"));
        }

        [Test]
        public void FullQueueDropsEqualOrLowerPriorityRequest()
        {
            var queue = new FeedbackPresentationQueue();
            queue.Enqueue(Event(FeedbackKind.Failure, "level.failed"), 0d);
            for (int i = 0; i < FeedbackEventRules.PendingQueueCapacity; i++)
            {
                queue.Enqueue(Event(FeedbackKind.Success, "action.success", i.ToString()), i + 1d);
            }

            Assert.That(
                queue.Enqueue(Event(FeedbackKind.Consume, "power.consume", "extra"), 20d),
                Is.EqualTo(FeedbackEnqueueResult.DroppedQueueFull));
            Assert.That(queue.PendingCount, Is.EqualTo(FeedbackEventRules.PendingQueueCapacity));
        }

        [Test]
        public void ClearRemovesActivePendingAndCooldownState()
        {
            var queue = new FeedbackPresentationQueue();
            FeedbackEventData failure = Event(FeedbackKind.Failure, "level.failed");
            queue.Enqueue(failure, 0d);
            queue.Enqueue(Event(FeedbackKind.Reward, "reward.granted"), 0.1d);

            queue.Clear();

            Assert.That(queue.HasActive, Is.False);
            Assert.That(queue.PendingCount, Is.Zero);
            Assert.That(queue.Enqueue(failure, 0.2d), Is.EqualTo(FeedbackEnqueueResult.Presented));
        }

        [Test]
        public void PresenterSubscribesToTypedChannelAndAdvancesQueuedCopy()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var root = new GameObject(
                "Feedback Toast",
                typeof(RectTransform),
                typeof(CanvasGroup));
            try
            {
                root.SetActive(false);

                var textObject = new GameObject(
                    "Message",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI));
                textObject.transform.SetParent(root.transform, false);
                var message = textObject.GetComponent<TextMeshProUGUI>();
                var presenter = root.AddComponent<FeedbackToastPresenter>();
                SetPrivate(presenter, "_Channel", channel);
                SetPrivate(presenter, "_CanvasGroup", root.GetComponent<CanvasGroup>());
                SetPrivate(presenter, "_ToastRoot", root.GetComponent<RectTransform>());
                SetPrivate(presenter, "_MessageText", message);
                root.SetActive(true);
                InvokePrivate(presenter, "OnEnable");

                channel.Raise(Event(FeedbackKind.Failure, "level.failed"));
                channel.Raise(Event(FeedbackKind.Reward, "reward.granted"));

                Assert.That(presenter.HasActiveFeedback, Is.True);
                Assert.That(presenter.PendingFeedbackCount, Is.EqualTo(1));
                Assert.That(message.text, Is.EqualTo("Try again"));

                presenter.AdvancePresentation(0.16f, 0.16d);
                presenter.AdvancePresentation(1.8f, 1.96d);
                presenter.AdvancePresentation(0.18f, 2.14d);

                Assert.That(presenter.ActiveFeedback.Kind, Is.EqualTo(FeedbackKind.Reward));
                Assert.That(message.text, Is.EqualTo("Reward received"));

                InvokePrivate(presenter, "OnActiveSceneChanged", default(Scene), default(Scene));

                Assert.That(presenter.HasActiveFeedback, Is.False);
                Assert.That(presenter.PendingFeedbackCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(channel);
            }
        }

        private static FeedbackEventData Event(
            FeedbackKind kind,
            string cueId,
            string contextId = null)
        {
            return new FeedbackEventData(kind, cueId, contextId);
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

        private static void InvokePrivate<TFirst, TSecond>(
            object target,
            string methodName,
            TFirst first,
            TSecond second)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.That(method, Is.Not.Null, $"Private method {methodName} was not found.");
            method.Invoke(target, new object[] { first, second });
        }
    }
}
