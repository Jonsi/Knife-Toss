using System.Reflection;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.UI.Common;
using Game.UI.Tokens;
using NUnit.Framework;
using TMPro;
using UnityEditor;
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
        public void ConfigureAndDisablePreserveExternalCanvasGroupStateWhenLoadingNeverOwnedIt()
        {
            UiSemanticTokens tokens = LoadProductionTokens();
            GameObject root = CreateSkinnableButton(
                out _,
                out ActionButtonPresenter presenter,
                out RectTransform presentationRoot,
                out _,
                out Image surface,
                out _,
                out _);
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0.73f;

            try
            {
                Assert.That(presenter.ConfigureSkin(tokens, UiButtonStyle.Primary, surface, presentationRoot), Is.True);
                Assert.That(group.interactable, Is.False);
                Assert.That(group.blocksRaycasts, Is.False);
                Assert.That(group.alpha, Is.EqualTo(0.73f));

                root.SetActive(false);

                Assert.That(group.interactable, Is.False);
                Assert.That(group.blocksRaycasts, Is.False);
                Assert.That(group.alpha, Is.EqualTo(0.73f));
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
        public void PointerDownRaisesSharedUiPressFeedback()
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
                presenter.OnPointerDown(new PointerEventData(null));

                Assert.That(wasRaised, Is.True);
                Assert.That(received.Kind, Is.EqualTo(FeedbackKind.Navigation));
                Assert.That(received.CueId, Is.EqualTo(ActionButtonPresenter.PressFeedbackCueId));
                Assert.That(received.ContextId, Is.EqualTo(root.name));
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

                root.SetActive(false);
                // EditMode does not dispatch MonoBehaviour lifecycle messages consistently;
                // invoke the callback explicitly, as the semantic lifecycle tests below do.
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

        [Test]
        public void SemanticSkinOwnsProductionVisualsAndRestoresOriginalButtonStateOnDestroy()
        {
            UiSemanticTokens tokens = LoadProductionTokens();
            GameObject root = CreateSkinnableButton(
                out Button button,
                out ActionButtonPresenter presenter,
                out RectTransform presentationRoot,
                out Image originalTarget,
                out Image surface,
                out Image ownership,
                out _);
            Sprite originalSurfaceSprite = tokens.Buttons.GetSurface(UiButtonStyle.Secondary);
            Sprite originalOwnershipSprite = tokens.Buttons.GetSurface(UiButtonStyle.Danger);
            Color originalSurfaceColor = new Color(0.23f, 0.41f, 0.67f, 0.83f);
            Color originalHitAreaColor = new Color(0.31f, 0.27f, 0.22f, 0.91f);

            surface.sprite = originalSurfaceSprite;
            surface.type = Image.Type.Filled;
            surface.color = originalSurfaceColor;
            ownership.sprite = originalOwnershipSprite;
            ownership.enabled = false;
            ownership.gameObject.SetActive(false);
            originalTarget.color = originalHitAreaColor;
            button.targetGraphic = originalTarget;
            button.transition = Selectable.Transition.ColorTint;

            try
            {
                Assert.That(
                    presenter.ConfigureSkin(
                        tokens,
                        UiButtonStyle.Primary,
                        surface,
                        presentationRoot,
                        UiButtonOwnershipMark.Equipped,
                        ownership),
                    Is.True);

                Assert.That(presenter.IsSemanticSkinApplied, Is.True);
                Assert.That(surface.sprite, Is.SameAs(tokens.Buttons.GetSurface(UiButtonStyle.Primary)));
                Assert.That(surface.type, Is.EqualTo(Image.Type.Sliced));
                Assert.That(surface.color, Is.EqualTo(Color.white));
                Assert.That(surface.raycastTarget, Is.False, "Scaled presentation graphics must not own the hit area.");
                Assert.That(originalTarget.raycastTarget, Is.True);
                Assert.That(originalTarget.color, Is.EqualTo(Color.clear), "The root hit Graphic must not bleed through transparent sprite corners.");
                Assert.That(button.targetGraphic, Is.SameAs(originalTarget));
                Assert.That(button.transition, Is.EqualTo(Selectable.Transition.None));
                Assert.That(ownership.sprite, Is.SameAs(tokens.Buttons.GetOwnershipMark(UiButtonOwnershipMark.Equipped)));
                Assert.That(ownership.enabled, Is.True);
                Assert.That(ownership.gameObject.activeSelf, Is.True);

                InvokePrivate(presenter, "OnDestroy");

                Assert.That(surface.sprite, Is.SameAs(originalSurfaceSprite));
                Assert.That(surface.type, Is.EqualTo(Image.Type.Filled));
                Assert.That(surface.color, Is.EqualTo(originalSurfaceColor));
                Assert.That(surface.raycastTarget, Is.True);
                Assert.That(originalTarget.color, Is.EqualTo(originalHitAreaColor));
                Assert.That(button.targetGraphic, Is.SameAs(originalTarget));
                Assert.That(button.transition, Is.EqualTo(Selectable.Transition.ColorTint));
                Assert.That(ownership.sprite, Is.SameAs(originalOwnershipSprite));
                Assert.That(ownership.enabled, Is.False);
                Assert.That(ownership.gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void InvalidOrMissingSemanticSkinIsAtomicAndLeavesColorTintVisualsUntouched()
        {
            var missingReferenceTokens = ScriptableObject.CreateInstance<UiSemanticTokens>();
            UiSemanticTokens productionTokens = LoadProductionTokens();
            GameObject root = CreateSkinnableButton(
                out Button button,
                out ActionButtonPresenter presenter,
                out RectTransform presentationRoot,
                out Image originalTarget,
                out Image surface,
                out Image ownership,
                out _);
            Sprite originalSurfaceSprite = productionTokens.Buttons.GetSurface(UiButtonStyle.Secondary);
            Sprite originalOwnershipSprite = productionTokens.Buttons.GetOwnershipMark(UiButtonOwnershipMark.Owned);
            Color originalSurfaceColor = new Color(0.19f, 0.38f, 0.57f, 0.76f);
            ColorBlock originalColors = button.colors;

            surface.sprite = originalSurfaceSprite;
            surface.type = Image.Type.Filled;
            surface.color = originalSurfaceColor;
            ownership.sprite = originalOwnershipSprite;
            ownership.enabled = true;
            ownership.gameObject.SetActive(true);
            button.targetGraphic = originalTarget;
            button.transition = Selectable.Transition.ColorTint;

            try
            {
                Assert.That(
                    presenter.ConfigureSkin(
                        missingReferenceTokens,
                        UiButtonStyle.Primary,
                        surface,
                        presentationRoot,
                        UiButtonOwnershipMark.Owned,
                        ownership),
                    Is.False,
                    "A token set with missing production sprites must fall back atomically.");
                AssertFallbackVisualsAreUntouched();

                originalTarget.raycastTarget = false;
                Assert.That(
                    presenter.ConfigureSkin(
                        productionTokens,
                        UiButtonStyle.Primary,
                        surface,
                        presentationRoot,
                        UiButtonOwnershipMark.Owned,
                        ownership),
                    Is.False,
                    "A scaled visual cannot substitute for a root-sized raycast target.");
                Assert.That(originalTarget.raycastTarget, Is.False);
                originalTarget.raycastTarget = true;
                AssertFallbackVisualsAreUntouched();

                Assert.That(
                    presenter.ConfigureSkin(
                        productionTokens,
                        UiButtonStyle.Primary,
                        surface,
                        root.GetComponent<RectTransform>(),
                        UiButtonOwnershipMark.Owned,
                        ownership),
                    Is.False,
                    "Using the hit rectangle as the presentation root is invalid.");
                AssertFallbackVisualsAreUntouched();

                Assert.That(
                    presenter.ConfigureSkin(
                        productionTokens,
                        UiButtonStyle.Primary,
                        surface,
                        presentationRoot,
                        UiButtonOwnershipMark.Owned,
                        surface),
                    Is.False,
                    "The surface and ownership mark must not alias the same Image.");
                AssertFallbackVisualsAreUntouched();
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(missingReferenceTokens);
            }

            void AssertFallbackVisualsAreUntouched()
            {
                Assert.That(presenter.IsSemanticSkinApplied, Is.False);
                Assert.That(surface.sprite, Is.SameAs(originalSurfaceSprite));
                Assert.That(surface.type, Is.EqualTo(Image.Type.Filled));
                Assert.That(surface.color, Is.EqualTo(originalSurfaceColor));
                Assert.That(surface.raycastTarget, Is.True);
                Assert.That(button.targetGraphic, Is.SameAs(originalTarget));
                Assert.That(button.transition, Is.EqualTo(Selectable.Transition.ColorTint));
                Assert.That(button.colors, Is.EqualTo(originalColors));
                Assert.That(ownership.sprite, Is.SameAs(originalOwnershipSprite));
                Assert.That(ownership.enabled, Is.True);
                Assert.That(ownership.gameObject.activeSelf, Is.True);
            }
        }

        [Test]
        public void SemanticSkinCanBeConfiguredWhileButtonHierarchyIsInactive()
        {
            UiSemanticTokens tokens = LoadProductionTokens();
            GameObject root = CreateSkinnableButton(
                out Button button,
                out ActionButtonPresenter presenter,
                out RectTransform presentationRoot,
                out Image hitArea,
                out Image surface,
                out _,
                out _);

            try
            {
                InvokePrivate(presenter, "OnDisable");

                Assert.That(presenter.ConfigureSkin(tokens, UiButtonStyle.Primary, surface, presentationRoot), Is.True);
                Assert.That(presenter.IsSemanticSkinApplied, Is.True);
                Assert.That(button.targetGraphic, Is.SameAs(hitArea));
                Assert.That(surface.raycastTarget, Is.False);

                root.SetActive(true);

                Assert.That(presenter.IsSemanticSkinApplied, Is.True);
                Assert.That(button.targetGraphic, Is.SameAs(hitArea));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SemanticPriceLabelStaysLiveDuringLoadingAndCancelRestoresReady()
        {
            UiSemanticTokens tokens = LoadProductionTokens();
            GameObject root = CreateSkinnableButton(
                out _,
                out ActionButtonPresenter presenter,
                out RectTransform presentationRoot,
                out _,
                out Image surface,
                out _,
                out TextMeshProUGUI label);

            try
            {
                Assert.That(
                    presenter.ConfigureSkin(tokens, UiButtonStyle.Price, surface, presentationRoot),
                    Is.True);
                Assert.That(label.enabled, Is.True);
                Assert.That(presenter.TryBeginSubmission(), Is.True);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Loading));
                Assert.That(label.enabled, Is.True, "Price/action truth must remain visible while progress is additive.");
                Assert.That(presenter.TryBeginSubmission(), Is.False, "Loading must reject duplicate submissions.");

                presenter.AdvancePresentation(0.09f);
                Assert.That(label.enabled, Is.True);

                presenter.CancelSubmission();

                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));
                Assert.That(presenter.IsLoading, Is.False);
                Assert.That(label.enabled, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SemanticPresenterPreservesBlockedLoadingDisabledPressedReadyPrecedence()
        {
            UiSemanticTokens tokens = LoadProductionTokens();
            GameObject root = CreateSkinnableButton(
                out _,
                out ActionButtonPresenter presenter,
                out RectTransform presentationRoot,
                out _,
                out Image surface,
                out _,
                out _);
            var pointer = new PointerEventData(null);

            try
            {
                Assert.That(presenter.ConfigureSkin(tokens, UiButtonStyle.Primary, surface, presentationRoot), Is.True);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));

                presenter.OnPointerDown(pointer);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Pressed));

                presenter.SetAvailable(false);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Disabled));

                presenter.SetAvailable(true);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));

                presenter.OnPointerDown(pointer);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Pressed));
                presenter.OnPointerUp(pointer);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));

                Assert.That(presenter.TryBeginSubmission(), Is.True);
                presenter.SetAvailable(false);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Loading));

                presenter.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.InsufficientFunds,
                    "store.insufficient_funds",
                    "power.super_knife"));
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Blocked));

                presenter.AdvancePresentation(tokens.Motion.EmphasisSeconds + 0.01f);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Disabled));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ReducedPreviewMakesLoadingStaticAndBlockedPositionExactWhileStandardBlockedMoves()
        {
            UiSemanticTokens tokens = LoadProductionTokens();
            GameObject reducedRoot = CreateSkinnableButton(
                out _,
                out ActionButtonPresenter reducedPresenter,
                out RectTransform reducedPresentationRoot,
                out _,
                out Image reducedSurface,
                out _,
                out _);
            GameObject standardRoot = CreateSkinnableButton(
                out _,
                out ActionButtonPresenter standardPresenter,
                out RectTransform standardPresentationRoot,
                out _,
                out Image standardSurface,
                out _,
                out _);
            Vector2 reducedBasePosition = new Vector2(13f, -29f);
            Vector2 standardBasePosition = new Vector2(-17f, 31f);
            reducedPresentationRoot.anchoredPosition = reducedBasePosition;
            standardPresentationRoot.anchoredPosition = standardBasePosition;

            try
            {
                Assert.That(reducedPresenter.ConfigureSkin(tokens, UiButtonStyle.Primary, reducedSurface, reducedPresentationRoot), Is.True);
                Assert.That(reducedPresenter.TryBeginSubmission(), Is.True);
                reducedPresenter.AdvancePresentation(0.137f);

                CanvasGroup loadingGroup = GetPrivateField<CanvasGroup>(reducedPresenter, "_LoadingCanvasGroup");
                Assert.That(loadingGroup.alpha, Is.Not.EqualTo(1f));
                reducedPresenter.CancelSubmission();
                reducedPresenter.SetReducedMotionPreview(true);
                Assert.That(reducedPresenter.TryBeginSubmission(), Is.True);
                Assert.That(loadingGroup.alpha, Is.EqualTo(1f), "Reduced loading must not expose one stale standard-pulse frame.");
                reducedPresenter.AdvancePresentation(0f);
                Assert.That(loadingGroup.alpha, Is.EqualTo(1f));
                reducedPresenter.AdvancePresentation(0.137f);
                Assert.That(loadingGroup.alpha, Is.EqualTo(1f), "Reduced-motion loading must remain static.");

                reducedPresenter.CancelSubmission();
                reducedPresenter.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.Blocked,
                    "button.reduced",
                    "reduced"));
                reducedPresenter.AdvancePresentation(0.025f);
                Assert.That(reducedPresentationRoot.anchoredPosition, Is.EqualTo(reducedBasePosition));

                Assert.That(standardPresenter.ConfigureSkin(tokens, UiButtonStyle.Primary, standardSurface, standardPresentationRoot), Is.True);
                standardPresenter.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.Blocked,
                    "button.standard",
                    "standard"));
                standardPresenter.AdvancePresentation(0.025f);
                Assert.That(
                    Mathf.Abs(standardPresentationRoot.anchoredPosition.x - standardBasePosition.x),
                    Is.GreaterThan(0.1f),
                    "The deterministic standard-motion sample must exercise the authored blocked shake.");
                Assert.That(standardPresentationRoot.anchoredPosition.y, Is.EqualTo(standardBasePosition.y));
            }
            finally
            {
                Object.DestroyImmediate(reducedRoot);
                Object.DestroyImmediate(standardRoot);
            }
        }

        [Test]
        public void PointerExitCancelsSemanticPressedStateWithoutStartingSubmission()
        {
            UiSemanticTokens tokens = LoadProductionTokens();
            GameObject root = CreateSkinnableButton(
                out Button button,
                out ActionButtonPresenter presenter,
                out RectTransform presentationRoot,
                out Image hitArea,
                out Image surface,
                out _,
                out _);
            var pointer = new PointerEventData(null);

            try
            {
                Assert.That(presenter.ConfigureSkin(tokens, UiButtonStyle.Secondary, surface, presentationRoot), Is.True);

                presenter.OnPointerDown(pointer);
                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Pressed));
                Assert.That(presenter.IsLoading, Is.False);
                presenter.AdvancePresentation(tokens.Motion.PressSeconds);
                Assert.That(presentationRoot.localScale, Is.EqualTo(Vector3.one * tokens.Buttons.PressedScale));
                Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(surface.raycastTarget, Is.False);
                Assert.That(hitArea.raycastTarget, Is.True);
                Assert.That(button.targetGraphic, Is.SameAs(hitArea));

                presenter.OnPointerExit(pointer);

                Assert.That(presenter.State, Is.EqualTo(ActionButtonPresentationState.Ready));
                Assert.That(presenter.IsLoading, Is.False);
                Assert.That(presenter.TryBeginSubmission(), Is.True, "Pointer cancellation must not consume an action submission.");
                presenter.CancelSubmission();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void UnskinnedFallbackRetainsLegacyLoadingPulseAndBlockedShake()
        {
            GameObject root = CreateButton(out _, out ActionButtonPresenter presenter);
            RectTransform rect = root.GetComponent<RectTransform>();

            try
            {
                Assert.That(presenter.TryBeginSubmission(), Is.True);
                presenter.AdvancePresentation(0.1f);

                CanvasGroup loadingGroup = GetPrivateField<CanvasGroup>(presenter, "_LoadingCanvasGroup");
                float expectedLoadingAlpha = 0.65f + Mathf.PingPong(0.1f * 1.8f, 0.35f);
                Assert.That(loadingGroup.alpha, Is.EqualTo(expectedLoadingAlpha).Within(0.0001f));

                presenter.CancelSubmission();
                presenter.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.Blocked,
                    "button.legacy",
                    "fallback"));
                presenter.AdvancePresentation(0.025f);

                const float blockedSeconds = 0.38f;
                const float shakeDistance = 8f;
                float remaining = blockedSeconds - 0.025f;
                float expectedOffset = Mathf.Sin(0.025f * 70f) * shakeDistance * (remaining / blockedSeconds);
                Assert.That(rect.anchoredPosition.x, Is.EqualTo(expectedOffset).Within(0.0001f));
                Assert.That(rect.anchoredPosition.y, Is.Zero.Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void UnskinnedFallbackDoesNotRollBackLiveGraphicColorChanges()
        {
            GameObject root = CreateButton(out _, out ActionButtonPresenter presenter);
            Image target = root.GetComponent<Image>();
            var pointer = new PointerEventData(null);
            Color pressedLiveColor = new Color(0.17f, 0.29f, 0.43f, 0.71f);
            Color disabledLiveColor = new Color(0.61f, 0.37f, 0.19f, 0.83f);

            try
            {
                presenter.OnPointerDown(pointer);
                target.color = pressedLiveColor;
                presenter.OnPointerExit(pointer);
                Assert.That(target.color, Is.EqualTo(pressedLiveColor));

                presenter.SetAvailable(false);
                target.color = disabledLiveColor;
                presenter.SetAvailable(true);
                Assert.That(target.color, Is.EqualTo(disabledLiveColor));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ReducedMotionPreviewIsEditorConditionalAndNeverSerialized()
        {
            FieldInfo field = typeof(ActionButtonPresenter).GetField("_ReducedMotionPreview", PrivateInstance);
            MethodInfo method = typeof(ActionButtonPresenter).GetMethod(
                nameof(ActionButtonPresenter.SetReducedMotionPreview),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(field, Is.Not.Null);
            Assert.That(field.IsDefined(typeof(System.NonSerializedAttribute), false), Is.True);
            Assert.That(method, Is.Not.Null);
            var conditional = method.GetCustomAttribute<System.Diagnostics.ConditionalAttribute>();
            Assert.That(conditional, Is.Not.Null);
            Assert.That(conditional.ConditionString, Is.EqualTo("UNITY_EDITOR"));
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

        private static GameObject CreateSkinnableButton(
            out Button button,
            out ActionButtonPresenter presenter,
            out RectTransform presentationRoot,
            out Image originalTarget,
            out Image surface,
            out Image ownership,
            out TextMeshProUGUI label)
        {
            var root = new GameObject(
                "Skinnable Action Button",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            button = root.GetComponent<Button>();
            originalTarget = root.GetComponent<Image>();
            button.targetGraphic = originalTarget;

            var visualRoot = new GameObject("Visual Root", typeof(RectTransform));
            visualRoot.transform.SetParent(root.transform, false);
            presentationRoot = visualRoot.GetComponent<RectTransform>();

            var surfaceObject = new GameObject("Surface", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            surfaceObject.transform.SetParent(visualRoot.transform, false);
            surface = surfaceObject.GetComponent<Image>();

            var ownershipObject = new GameObject("Ownership", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ownershipObject.transform.SetParent(visualRoot.transform, false);
            ownership = ownershipObject.GetComponent<Image>();

            var labelObject = new GameObject("Content Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(visualRoot.transform, false);
            label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "250";

            presenter = root.AddComponent<ActionButtonPresenter>();
            presenter.Configure(button);
            return root;
        }

        private static UiSemanticTokens LoadProductionTokens()
        {
            UiSemanticTokens tokens = AssetDatabase.LoadAssetAtPath<UiSemanticTokens>(
                UiSemanticTokens.ProductionAssetPath);
            Assert.That(tokens, Is.Not.Null, $"Missing production tokens at {UiSemanticTokens.ProductionAssetPath}.");
            return tokens;
        }

        private static T GetPrivateField<T>(object target, string fieldName) where T : class
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Private field {fieldName} was not found.");
            return field.GetValue(target) as T;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.That(method, Is.Not.Null, $"Private method {methodName} was not found.");
            method.Invoke(target, null);
        }
    }
}
