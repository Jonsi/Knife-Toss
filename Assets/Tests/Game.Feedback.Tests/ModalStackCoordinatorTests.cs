using System;
using Game.UI.Services;
using NUnit.Framework;
using UnityEngine;

namespace Game.Feedback.Tests
{
    public class ModalStackCoordinatorTests
    {
        [Test]
        public void DefaultAcquisitionClaimsInputUntilReleased()
        {
            var coordinator = new ModalStackCoordinator();
            var owner = new object();

            IModalRegistration registration = coordinator.Acquire(owner);

            Assert.That(coordinator.State.ActiveCount, Is.EqualTo(1));
            Assert.That(coordinator.State.TopOwner, Is.SameAs(owner));
            Assert.That(coordinator.State.BlocksUnderlyingInput, Is.True);

            registration.Dispose();

            Assert.That(coordinator.State.ActiveCount, Is.Zero);
            Assert.That(coordinator.State.TopOwner, Is.Null);
            Assert.That(coordinator.State.BlocksUnderlyingInput, Is.False);
        }

        [Test]
        public void NewestActiveOwnerIsTopAndNonTopReleaseDoesNotDisturbIt()
        {
            var coordinator = new ModalStackCoordinator();
            var settings = new object();
            var privacy = new object();

            IModalRegistration settingsRegistration = coordinator.Acquire(settings);
            IModalRegistration privacyRegistration = coordinator.Acquire(privacy);

            Assert.That(coordinator.State.ActiveCount, Is.EqualTo(2));
            Assert.That(coordinator.State.TopOwner, Is.SameAs(privacy));

            settingsRegistration.Dispose();

            Assert.That(coordinator.State.ActiveCount, Is.EqualTo(1));
            Assert.That(coordinator.State.TopOwner, Is.SameAs(privacy));
            Assert.That(coordinator.State.BlocksUnderlyingInput, Is.True);

            privacyRegistration.Dispose();
            Assert.That(coordinator.State.HasActiveModal, Is.False);
        }

        [Test]
        public void OverlappingInputAuthorityPersistsUntilEveryClaimReleases()
        {
            var coordinator = new ModalStackCoordinator();
            IModalRegistration inputOwner = coordinator.Acquire(
                new object(),
                ModalInteractionAuthority.BlocksUnderlyingInput);
            IModalRegistration secondInputOwner = coordinator.Acquire(new object());

            Assert.That(coordinator.State.BlocksUnderlyingInput, Is.True);
            secondInputOwner.Dispose();

            Assert.That(coordinator.State.BlocksUnderlyingInput, Is.True);

            inputOwner.Dispose();
            Assert.That(coordinator.State.Authority, Is.EqualTo(ModalInteractionAuthority.None));
        }

        [Test]
        public void RepeatedAcquisitionForSameOwnerIsIdempotent()
        {
            var coordinator = new ModalStackCoordinator();
            var owner = new object();
            var stateChangeCount = 0;
            coordinator.StateChanged += _ => stateChangeCount++;

            IModalRegistration first = coordinator.Acquire(owner);
            int revisionAfterFirstAcquire = coordinator.State.Revision;
            IModalRegistration second = coordinator.Acquire(
                owner,
                ModalInteractionAuthority.None);

            Assert.That(second, Is.SameAs(first));
            Assert.That(coordinator.State.ActiveCount, Is.EqualTo(1));
            Assert.That(coordinator.State.Revision, Is.EqualTo(revisionAfterFirstAcquire));
            Assert.That(coordinator.State.BlocksUnderlyingInput, Is.True,
                "The first acquisition owns authority until it is released.");
            Assert.That(stateChangeCount, Is.EqualTo(1));

            second.Dispose();
            first.Dispose();

            Assert.That(coordinator.State.ActiveCount, Is.Zero);
            Assert.That(stateChangeCount, Is.EqualTo(2));
        }

        [Test]
        public void ReleasedOwnerCanAcquireFreshRegistrationWithoutStaleRelease()
        {
            var coordinator = new ModalStackCoordinator();
            var owner = new object();
            IModalRegistration oldRegistration = coordinator.Acquire(owner);
            oldRegistration.Dispose();

            IModalRegistration currentRegistration = coordinator.Acquire(owner);
            oldRegistration.Dispose();

            Assert.That(currentRegistration, Is.Not.SameAs(oldRegistration));
            Assert.That(currentRegistration.IsActive, Is.True);
            Assert.That(coordinator.State.ActiveCount, Is.EqualTo(1));

            currentRegistration.Dispose();
            Assert.That(coordinator.State.ActiveCount, Is.Zero);
        }

        [Test]
        public void CoordinatorDisposalCleansRegistrationsAndRejectsNewOwners()
        {
            var coordinator = new ModalStackCoordinator();
            IModalRegistration first = coordinator.Acquire(new object());
            IModalRegistration second = coordinator.Acquire(new object());
            ModalStackState finalState = coordinator.State;
            coordinator.StateChanged += state => finalState = state;

            coordinator.Dispose();

            Assert.That(first.IsActive, Is.False);
            Assert.That(second.IsActive, Is.False);
            Assert.That(finalState.ActiveCount, Is.Zero);
            Assert.That(finalState.BlocksUnderlyingInput, Is.False);
            Assert.Throws<ObjectDisposedException>(() => coordinator.Acquire(new object()));

            Assert.DoesNotThrow(() => first.Dispose());
            Assert.DoesNotThrow(() => coordinator.Dispose());
        }

        [Test]
        public void NullOwnerIsRejectedWithoutChangingState()
        {
            var coordinator = new ModalStackCoordinator();

            Assert.Throws<ArgumentNullException>(() => coordinator.Acquire(null));
            Assert.That(coordinator.State.ActiveCount, Is.Zero);
            Assert.That(coordinator.State.Revision, Is.Zero);
        }

        [Test]
        public void InputBlockerTracksAuthorityAndCleansUpWhenDisabled()
        {
            var coordinator = new ModalStackCoordinator();
            var blockerObject = new GameObject(
                "Modal Input Blocker",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(ModalInputBlockerPresenter));

            try
            {
                var blocker = blockerObject.GetComponent<ModalInputBlockerPresenter>();
                var canvasGroup = blockerObject.GetComponent<CanvasGroup>();
                blocker.Construct(coordinator);

                Assert.That(blocker.IsBlocking, Is.False);
                Assert.That(canvasGroup.alpha, Is.Zero);

                IModalRegistration registration = coordinator.Acquire(new object());

                Assert.That(blocker.IsBlocking, Is.True);
                Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
                Assert.That(canvasGroup.interactable, Is.False,
                    "The scrim intercepts raycasts but never acts as a selectable control.");

                blocker.Dispose();
                blockerObject.SetActive(false);

                Assert.That(blocker.IsBlocking, Is.False);
                Assert.That(canvasGroup.alpha, Is.Zero);

                registration.Dispose();
                blockerObject.SetActive(true);
                Assert.That(blocker.IsBlocking, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(blockerObject);
                coordinator.Dispose();
            }
        }
    }
}
