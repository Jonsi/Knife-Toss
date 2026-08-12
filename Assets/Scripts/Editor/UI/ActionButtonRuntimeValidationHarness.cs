using System;
using System.Linq;
using Game.Feedback.Data;
using Game.UI.Common;
using Game.UI.Tokens;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Editor.UI
{
    /// <summary>
    /// Temporary, non-shipping P2 button evidence surface. It exists only in Play Mode, uses the
    /// production token/sprite references, and never invokes screen actions or player-data services.
    /// </summary>
    [InitializeOnLoad]
    public static class ActionButtonRuntimeValidationHarness
    {
        private const string RootName = "__P2_BUTTON_VALIDATION__";
        private const string PrimaryName = "P2 Primary Action";
        private const string BannerName = "P2 State Banner";
        private const string PrimaryLabelName = "P2 Primary Live Label";
        private const HideFlags TemporaryFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

        private static int _SubmissionAttempts;
        private static int _AcceptedSubmissions;
        private static bool _LastDuplicateResult;

        static ActionButtonRuntimeValidationHarness()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Spawn or Reset")]
        public static void SpawnOrReset()
        {
            if (!EnsurePlayMode())
                return;

            Remove();
            _SubmissionAttempts = 0;
            _AcceptedSubmissions = 0;
            _LastDuplicateResult = false;

            if (EventSystem.current == null)
            {
                Debug.LogError("P2 button validation requires the scene's existing EventSystem.");
                return;
            }

            UiSemanticTokens tokens = AssetDatabase.LoadAssetAtPath<UiSemanticTokens>(
                UiSemanticTokens.ProductionAssetPath);
            if (tokens == null || tokens.GetValidationIssues().Count != 0)
            {
                Debug.LogError("P2 button validation requires the complete production UiSemanticTokens asset.");
                return;
            }

            GameObject root = CreateObject(
                RootName,
                null,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = tokens.Accessibility.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = tokens.Accessibility.CanvasWidthMatch;

            Image backdrop = CreateImage("P2 Validation Backdrop", root.transform as RectTransform);
            Stretch(backdrop.rectTransform);
            backdrop.color = new Color32(0x07, 0x17, 0x0F, 0xFF);
            backdrop.raycastTarget = true;

            CreateText(
                "P2 Validation Title",
                root.transform as RectTransform,
                "PRODUCTION BUTTON FAMILY",
                tokens,
                UiTypographyRole.Heading,
                60f,
                new Vector2(0f, 810f),
                new Vector2(900f, 100f));
            CreateText(
                BannerName,
                root.transform as RectTransform,
                "STANDARD | READY | LIVE CONTENT",
                tokens,
                UiTypographyRole.Body,
                30f,
                new Vector2(0f, 720f),
                new Vector2(920f, 72f));

            CreateButton(tokens, PrimaryName, UiButtonStyle.Primary, "THROW", new Vector2(0f, 520f), new Vector2(600f, 150f));
            CreateButton(tokens, "P2 Secondary Action", UiButtonStyle.Secondary, "STAGES", new Vector2(0f, 330f), new Vector2(560f, 140f));
            CreateButton(tokens, "P2 Danger Action", UiButtonStyle.Danger, "RETRY", new Vector2(0f, 150f), new Vector2(560f, 140f));
            CreateButton(tokens, "P2 Icon Action", UiButtonStyle.IconOnly, "+", new Vector2(-350f, -75f), new Vector2(150f, 150f));
            CreateButton(tokens, "P2 Price Action", UiButtonStyle.Price, "BUY   1,250", new Vector2(120f, -75f), new Vector2(600f, 150f));
            CreateButton(
                tokens,
                "P2 Owned Action",
                UiButtonStyle.Status,
                "OWNED",
                new Vector2(-230f, -340f),
                new Vector2(430f, 140f),
                UiButtonOwnershipMark.Owned);
            CreateButton(
                tokens,
                "P2 Equipped Action",
                UiButtonStyle.Status,
                "EQUIPPED",
                new Vector2(230f, -340f),
                new Vector2(430f, 140f),
                UiButtonOwnershipMark.Equipped);

            CreateText(
                "P2 Validation Footer",
                root.transform as RectTransform,
                "TEMPORARY EDITOR PREVIEW | NO SAVE / ECONOMY / NAVIGATION MUTATION",
                tokens,
                UiTypographyRole.Caption,
                24f,
                new Vector2(0f, -790f),
                new Vector2(980f, 64f));

            Debug.Log("P2_BUTTON_TRACE spawn standard ready; submitAttempts=0 accepted=0");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Set Standard Ready")]
        public static void SetStandardReady()
        {
            foreach (ActionButtonPresenter presenter in Presenters())
            {
                presenter.SetReducedMotionPreview(false);
                presenter.SetAvailable(true);
                presenter.CancelSubmission();
                presenter.OnPointerExit(Pointer());
                presenter.AdvancePresentation(1f);
            }

            _SubmissionAttempts = 0;
            _AcceptedSubmissions = 0;
            _LastDuplicateResult = false;
            SetBanner("STANDARD | READY | LIVE CONTENT");
            Debug.Log("P2_BUTTON_TRACE standard ready; submitAttempts=0 accepted=0");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Pointer Down")]
        public static void PointerDown()
        {
            ActionButtonPresenter primary = Primary();
            primary?.OnPointerDown(Pointer());
            SetBanner(IsReduced(primary) ? "REDUCED PREVIEW | PRESSED" : "STANDARD | PRESSED");
            Debug.Log($"P2_BUTTON_TRACE pointerDown state={primary?.State} submitAttempts={_SubmissionAttempts}");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Pointer Exit Cancel")]
        public static void PointerExitCancel()
        {
            ActionButtonPresenter primary = Primary();
            primary?.OnPointerExit(Pointer());
            SetBanner(IsReduced(primary) ? "REDUCED PREVIEW | POINTER CANCEL -> READY" : "STANDARD | POINTER CANCEL -> READY");
            Debug.Log($"P2_BUTTON_TRACE pointerExit state={primary?.State} submitAttempts={_SubmissionAttempts}");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Begin Loading + Duplicate Probe")]
        public static void BeginLoading()
        {
            ActionButtonPresenter primary = Primary();
            if (primary == null)
                return;

            _SubmissionAttempts++;
            bool accepted = primary.TryBeginSubmission();
            if (accepted)
                _AcceptedSubmissions++;

            _SubmissionAttempts++;
            _LastDuplicateResult = primary.TryBeginSubmission();
            if (_LastDuplicateResult)
                _AcceptedSubmissions++;

            SetBanner(IsReduced(primary)
                ? "REDUCED PREVIEW | LOADING (STATIC CUE)"
                : "STANDARD | LOADING (DUPLICATE REJECTED)");
            Debug.Log(
                $"P2_BUTTON_TRACE loading state={primary.State} attempts={_SubmissionAttempts} " +
                $"accepted={_AcceptedSubmissions} duplicateResult={_LastDuplicateResult}");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Cancel Loading")]
        public static void CancelLoading()
        {
            ActionButtonPresenter primary = Primary();
            primary?.CancelSubmission();
            SetBanner(IsReduced(primary) ? "REDUCED PREVIEW | ASYNC CANCEL -> READY" : "STANDARD | ASYNC CANCEL -> READY");
            Debug.Log($"P2_BUTTON_TRACE cancelLoading state={primary?.State} accepted={_AcceptedSubmissions}");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Present Blocked")]
        public static void PresentBlocked()
        {
            ActionButtonPresenter primary = Primary();
            if (primary == null)
                return;

            primary.PresentBlocked(new FeedbackEventData(
                FeedbackKind.InsufficientFunds,
                "validation.insufficient_funds",
                "p2.button.preview"));
            SetBanner(IsReduced(primary)
                ? "REDUCED PREVIEW | BLOCKED (NO SHAKE)"
                : "STANDARD | BLOCKED");
            Debug.Log($"P2_BUTTON_TRACE blocked state={primary.State} reduced={primary.IsReducedMotionPreview}");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Set Disabled")]
        public static void SetDisabled()
        {
            ActionButtonPresenter primary = Primary();
            primary?.SetAvailable(false);
            SetBanner(IsReduced(primary) ? "REDUCED PREVIEW | DISABLED" : "STANDARD | DISABLED");
            Debug.Log($"P2_BUTTON_TRACE disabled state={primary?.State}");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Set Reduced Preview")]
        public static void SetReducedPreview()
        {
            foreach (ActionButtonPresenter presenter in Presenters())
            {
                presenter.SetReducedMotionPreview(true);
                presenter.SetAvailable(true);
                presenter.CancelSubmission();
                presenter.OnPointerExit(Pointer());
                presenter.AdvancePresentation(1f);
            }

            _SubmissionAttempts = 0;
            _AcceptedSubmissions = 0;
            _LastDuplicateResult = false;
            SetBanner("REDUCED-MOTION PREVIEW | NON-PERSISTENT | READY");
            Debug.Log("P2_BUTTON_TRACE reducedPreview ready; persistence=false policyOwner=UX-406");
        }

        [MenuItem("Tools/Knife Toss/UI Validation/P2 Buttons/Remove")]
        public static void Remove()
        {
            GameObject root = FindRoot();
            if (root != null)
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        public static void AdvancePrimary(float unscaledDeltaTime)
        {
            Primary()?.AdvancePresentation(unscaledDeltaTime);
        }

        public static string Snapshot()
        {
            GameObject root = FindRoot();
            ActionButtonPresenter primary = Primary();
            if (root == null || primary == null)
                return JsonUtility.ToJson(new ValidationSnapshot { overlayExists = false });

            Transform visual = primary.transform.Find("PresentationRoot");
            Transform indicator = visual != null ? visual.Find("LoadingIndicator") : null;
            CanvasGroup loadingGroup = indicator != null ? indicator.GetComponent<CanvasGroup>() : null;
            TMP_Text label = primary.GetComponentsInChildren<TMP_Text>(true)
                .FirstOrDefault(candidate => candidate.name == PrimaryLabelName);
            RectTransform visualRect = visual as RectTransform;
            var snapshot = new ValidationSnapshot
            {
                overlayExists = true,
                state = primary.State.ToString(),
                submitAttempts = _SubmissionAttempts,
                acceptedSubmissions = _AcceptedSubmissions,
                duplicateResult = _LastDuplicateResult,
                reducedPreview = primary.IsReducedMotionPreview,
                semanticSkinApplied = primary.IsSemanticSkinApplied,
                buttonInteractable = primary.Button != null && primary.Button.interactable,
                groupInteractable = primary.GetComponent<CanvasGroup>().interactable,
                groupBlocksRaycasts = primary.GetComponent<CanvasGroup>().blocksRaycasts,
                labelEnabled = label != null && label.enabled,
                loadingActive = indicator != null && indicator.gameObject.activeSelf,
                loadingAlpha = loadingGroup != null ? loadingGroup.alpha : -1f,
                visualScale = visual != null ? visual.localScale : Vector3.zero,
                visualPosition = visualRect != null ? visualRect.anchoredPosition : Vector2.zero
            };
            return JsonUtility.ToJson(snapshot);
        }

        public static void SetTraceLabel(string value)
        {
            SetBanner(value);
        }

        private static ActionButtonPresenter CreateButton(
            UiSemanticTokens tokens,
            string name,
            UiButtonStyle style,
            string liveText,
            Vector2 position,
            Vector2 size,
            UiButtonOwnershipMark ownershipMark = UiButtonOwnershipMark.None)
        {
            RectTransform canvasRoot = FindRoot().transform as RectTransform;
            GameObject buttonObject = CreateObject(
                name,
                canvasRoot,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(CanvasGroup));
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = position;

            GameObject visualObject = CreateObject("PresentationRoot", buttonRect, typeof(RectTransform));
            RectTransform visualRoot = visualObject.GetComponent<RectTransform>();
            Stretch(visualRoot);

            Image surface = CreateImage("Surface", visualRoot);
            Stretch(surface.rectTransform);
            surface.color = Color.white;
            surface.raycastTarget = true;

            Image hitArea = buttonObject.GetComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = hitArea;
            button.transition = Selectable.Transition.ColorTint;

            TextMeshProUGUI label = CreateText(
                name == PrimaryName ? PrimaryLabelName : $"{name} Live Label",
                visualRoot,
                liveText,
                tokens,
                UiTypographyRole.Action,
                style == UiButtonStyle.IconOnly ? 58f : 42f,
                Vector2.zero,
                size);
            label.color = tokens.Colors.Get(
                style == UiButtonStyle.Danger || style == UiButtonStyle.Status
                    ? UiColorRole.Ivory
                    : UiColorRole.Ink);
            label.raycastTarget = false;

            if (ownershipMark != UiButtonOwnershipMark.None)
            {
                label.rectTransform.sizeDelta = new Vector2(size.x - 132f, size.y);
                label.rectTransform.anchoredPosition = new Vector2(54f, 0f);
            }

            Image ownershipImage = null;
            if (ownershipMark != UiButtonOwnershipMark.None)
            {
                ownershipImage = CreateImage("OwnershipMark", visualRoot);
                RectTransform ownershipRect = ownershipImage.rectTransform;
                ownershipRect.anchorMin = ownershipRect.anchorMax = new Vector2(0f, 0.5f);
                ownershipRect.pivot = new Vector2(0.5f, 0.5f);
                ownershipRect.sizeDelta = new Vector2(76f, 76f);
                ownershipRect.anchoredPosition = new Vector2(58f, 0f);
                ownershipImage.raycastTarget = false;
            }

            ActionButtonPresenter presenter = buttonObject.AddComponent<ActionButtonPresenter>();
            bool applied = presenter.ConfigureSkin(
                tokens,
                style,
                surface,
                visualRoot,
                ownershipMark,
                ownershipImage,
                hitArea);
            if (!applied)
            {
                Debug.LogError($"P2 button validation failed to apply production skin {style} to {name}.");
            }

            return presenter;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            RectTransform parent,
            string value,
            UiSemanticTokens tokens,
            UiTypographyRole role,
            float size,
            Vector2 position,
            Vector2 dimensions)
        {
            GameObject textObject = CreateObject(
                name,
                parent,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = position;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.alignment = TextAlignmentOptions.Center;
            text.font = tokens.Typography.Get(role).FontAsset;
            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(size, 20f);
            text.fontSizeMax = size;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.color = tokens.Colors.Get(UiColorRole.Ivory);
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(string name, RectTransform parent)
        {
            GameObject imageObject = CreateObject(
                name,
                parent,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            return imageObject.GetComponent<Image>();
        }

        private static GameObject CreateObject(
            string name,
            RectTransform parent,
            params Type[] components)
        {
            var gameObject = new GameObject(name, components);
            gameObject.hideFlags = TemporaryFlags;
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }
            return gameObject;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static bool EnsurePlayMode()
        {
            if (EditorApplication.isPlaying)
                return true;

            Debug.LogWarning("P2 button validation is Play Mode only.");
            return false;
        }

        private static PointerEventData Pointer()
        {
            return new PointerEventData(EventSystem.current);
        }

        private static ActionButtonPresenter Primary()
        {
            GameObject primary = GameObject.Find(PrimaryName);
            return primary != null ? primary.GetComponent<ActionButtonPresenter>() : null;
        }

        private static ActionButtonPresenter[] Presenters()
        {
            GameObject root = FindRoot();
            return root != null
                ? root.GetComponentsInChildren<ActionButtonPresenter>(true)
                : Array.Empty<ActionButtonPresenter>();
        }

        private static bool IsReduced(ActionButtonPresenter presenter)
        {
            return presenter != null && presenter.IsReducedMotionPreview;
        }

        private static GameObject FindRoot()
        {
            return GameObject.Find(RootName);
        }

        private static void SetBanner(string value)
        {
            GameObject banner = GameObject.Find(BannerName);
            TMP_Text text = banner != null ? banner.GetComponent<TMP_Text>() : null;
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                Remove();
            }
        }

        [Serializable]
        private sealed class ValidationSnapshot
        {
            public bool overlayExists;
            public string state;
            public int submitAttempts;
            public int acceptedSubmissions;
            public bool duplicateResult;
            public bool reducedPreview;
            public bool semanticSkinApplied;
            public bool buttonInteractable;
            public bool groupInteractable;
            public bool groupBlocksRaycasts;
            public bool labelEnabled;
            public bool loadingActive;
            public float loadingAlpha;
            public Vector3 visualScale;
            public Vector2 visualPosition;
        }
    }
}
