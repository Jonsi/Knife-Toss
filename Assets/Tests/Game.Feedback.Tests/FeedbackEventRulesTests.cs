using System;
using System.Collections.Generic;
using Game.Feedback.Data;
using Game.Feedback.Events;
using NUnit.Framework;
using UnityEngine;

namespace Game.Feedback.Tests
{
    public class FeedbackEventRulesTests
    {
        [Test]
        public void RequiredKindsAreCompleteAndHavePolicy()
        {
            var expected = new HashSet<FeedbackKind>
            {
                FeedbackKind.Success,
                FeedbackKind.Blocked,
                FeedbackKind.InsufficientFunds,
                FeedbackKind.Purchase,
                FeedbackKind.Reward,
                FeedbackKind.Consume,
                FeedbackKind.Navigation,
                FeedbackKind.Failure,
                FeedbackKind.Completion
            };

            CollectionAssert.AreEquivalent(expected, Enum.GetValues(typeof(FeedbackKind)));
            foreach (FeedbackKind kind in expected)
            {
                Assert.That(FeedbackEventRules.GetCooldownSeconds(kind), Is.GreaterThan(0f));
                Assert.DoesNotThrow(() => FeedbackEventRules.Validate(kind));
            }
        }

        [TestCase(FeedbackKind.Navigation, FeedbackPriority.Low)]
        [TestCase(FeedbackKind.Success, FeedbackPriority.Normal)]
        [TestCase(FeedbackKind.Consume, FeedbackPriority.Normal)]
        [TestCase(FeedbackKind.Purchase, FeedbackPriority.Elevated)]
        [TestCase(FeedbackKind.Reward, FeedbackPriority.Elevated)]
        [TestCase(FeedbackKind.Blocked, FeedbackPriority.High)]
        [TestCase(FeedbackKind.InsufficientFunds, FeedbackPriority.High)]
        [TestCase(FeedbackKind.Failure, FeedbackPriority.Critical)]
        [TestCase(FeedbackKind.Completion, FeedbackPriority.Critical)]
        public void KindMapsToExplicitPriority(FeedbackKind kind, FeedbackPriority expected)
        {
            Assert.That(FeedbackEventRules.GetPriority(kind), Is.EqualTo(expected));
        }

        [Test]
        public void EventDerivesPriorityCooldownAndStableDuplicateKey()
        {
            var feedback = new FeedbackEventData(
                FeedbackKind.InsufficientFunds,
                "store.insufficient_funds",
                "power_up.chrono");

            Assert.That(feedback.Priority, Is.EqualTo(FeedbackPriority.High));
            Assert.That(feedback.CooldownSeconds, Is.EqualTo(0.75f));
            Assert.That(
                feedback.DuplicateKey,
                Is.EqualTo("InsufficientFunds:store.insufficient_funds:power_up.chrono"));
        }

        [Test]
        public void SharedChannelDispatchesTypedEventWithoutPresentationDependencies()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var expected = new FeedbackEventData(
                FeedbackKind.Completion,
                "level.completed",
                "stage.1.level.3");
            FeedbackEventData received = default;

            void Handler(FeedbackEventData data) => received = data;

            channel.Subscribe(Handler);
            channel.Raise(expected);
            channel.Unsubscribe(Handler);

            Assert.That(received.Kind, Is.EqualTo(expected.Kind));
            Assert.That(received.CueId, Is.EqualTo(expected.CueId));
            Assert.That(received.ContextId, Is.EqualTo(expected.ContextId));

            UnityEngine.Object.DestroyImmediate(channel);
        }

        [Test]
        public void HigherPriorityInterruptsAndInterruptedRequestDoesNotAffectTheDecision()
        {
            var active = Event(FeedbackKind.Success, "action.success");
            var incoming = Event(FeedbackKind.Failure, "level.failure");

            Assert.That(FeedbackEventRules.ShouldInterrupt(active, incoming), Is.True);
            Assert.That(FeedbackEventRules.ShouldInterrupt(incoming, active), Is.False);
        }

        [Test]
        public void EqualPriorityNeverInterruptsAndPreservesFifoQueueOrder()
        {
            var purchase = Event(FeedbackKind.Purchase, "store.purchase");
            var reward = Event(FeedbackKind.Reward, "reward.granted");

            Assert.That(FeedbackEventRules.ShouldInterrupt(purchase, reward), Is.False);
            Assert.That(FeedbackEventRules.ShouldInsertBefore(reward, purchase), Is.False);
        }

        [Test]
        public void QueueOrdersByDescendingPriority()
        {
            var navigation = Event(FeedbackKind.Navigation, "navigation.open");
            var blocked = Event(FeedbackKind.Blocked, "power_up.blocked");

            Assert.That(FeedbackEventRules.ShouldInsertBefore(blocked, navigation), Is.True);
            Assert.That(FeedbackEventRules.ShouldInsertBefore(navigation, blocked), Is.False);
        }

        [Test]
        public void FullQueueReplacementRequiresStrictlyHigherPriority()
        {
            var incoming = Event(FeedbackKind.InsufficientFunds, "store.insufficient_funds");
            var lowest = Event(FeedbackKind.Navigation, "navigation.open");
            var equal = Event(FeedbackKind.Blocked, "power_up.blocked");

            Assert.That(FeedbackEventRules.PendingQueueCapacity, Is.EqualTo(8));
            Assert.That(FeedbackEventRules.ShouldReplaceWhenQueueIsFull(incoming, lowest), Is.True);
            Assert.That(FeedbackEventRules.ShouldReplaceWhenQueueIsFull(incoming, equal), Is.False);
        }

        [Test]
        public void DuplicateSuppressionUsesKindCueAndContext()
        {
            var first = new FeedbackEventData(FeedbackKind.Reward, "reward.granted", "mission.daily");
            var same = new FeedbackEventData(FeedbackKind.Reward, "reward.granted", "mission.daily");
            var otherContext = new FeedbackEventData(FeedbackKind.Reward, "reward.granted", "mission.weekly");
            var otherKind = new FeedbackEventData(FeedbackKind.Purchase, "reward.granted", "mission.daily");

            Assert.That(FeedbackEventRules.IsDuplicate(first, same), Is.True);
            Assert.That(FeedbackEventRules.IsDuplicate(first, otherContext), Is.False);
            Assert.That(FeedbackEventRules.IsDuplicate(first, otherKind), Is.False);
        }

        [Test]
        public void CooldownSuppressesBeforeButNotAtBoundary()
        {
            var feedback = Event(FeedbackKind.Consume, "power_up.consumed");

            Assert.That(
                FeedbackEventRules.IsInCooldown(
                    feedback,
                    feedback.DuplicateKey,
                    0d,
                    feedback.CooldownSeconds - 0.001d),
                Is.True);
            Assert.That(
                FeedbackEventRules.IsInCooldown(
                    feedback,
                    feedback.DuplicateKey,
                    0d,
                    feedback.CooldownSeconds),
                Is.False);
            Assert.That(
                FeedbackEventRules.IsInCooldown(feedback, "Consume:other", 10d, 10.1d),
                Is.False);
        }

        [Test]
        public void InvalidCueOrKindIsRejected()
        {
            Assert.Throws<ArgumentException>(() =>
                new FeedbackEventData(FeedbackKind.Success, " "));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new FeedbackEventData((FeedbackKind)999, "unknown"));
        }

        private static FeedbackEventData Event(FeedbackKind kind, string cueId)
        {
            return new FeedbackEventData(kind, cueId);
        }
    }
}
