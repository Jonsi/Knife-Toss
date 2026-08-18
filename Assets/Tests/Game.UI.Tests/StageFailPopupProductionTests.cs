using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Events;
using Game.Ads.Controllers;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Flow.Events;
using Game.Flow.Stage;
using Game.Level.Configurations.Stage;
using Game.Level.Data;
using Game.Level.Interfaces;
using Game.UI.Common;
using Game.UI.Popups;
using Game.UI.Store;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI.Tests
{
    public sealed class StageFailPopupProductionTests
    {
        private const string StageScenePath = "Assets/Scenes/StagePlay.unity";
        private const string PopupTemplatePath = "Assets/Prefabs/UI/PopupTemplate.prefab";
        private const string ActionButtonPrefabPath = "Assets/Prefabs/UI/Buttons/FailureActionButton.prefab";
        private const string PricePillPrefabPath = "Assets/Prefabs/UI/Store/PricePillView.prefab";
        private const string LegacyFailureAtlasPath = "Assets/Art/UI/Failure/FailurePopupAtlas.png";
        private const string GameOverTitleSpritePath = "Assets/Art/UI/Failure/GameOverTitle_v2.png";
        private const string RetryButtonSpritePath = "Assets/Art/UI/Failure/FailureRetryButton_Source.png";
        private const string KnifeAnimationAtlasPath = "Assets/Art/UI/Failure/FailureKnivesAnimation.png";
        private const string SecondaryButtonSpritePath = "Assets/Art/UI/Core/ButtonSecondary.png";
        private const string DisplayFontPath = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular SDF.asset";
        private const string ImpactGlowPath = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Particles/Texture/fx_glow_small.png";

        [Test]
        public void StagePlayFailurePopupMatchesApprovedContentAndPrefabContract()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            string previousPath = previousScene.path;
            bool restorePrevious = !string.IsNullOrEmpty(previousPath) && previousPath != StageScenePath;

            try
            {
                EditorSceneManager.OpenScene(StageScenePath, OpenSceneMode.Single);
                StageFailPopup popup = UnityEngine.Object.FindObjectsByType<StageFailPopup>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Single();

                TMP_Text[] runtimeLabels = popup.GetComponentsInChildren<TMP_Text>(true);
                Assert.That(runtimeLabels, Has.Length.EqualTo(4));
                TMP_Text stageLevelLabel = runtimeLabels.Single(label => label.name == "StageLevelLabel");
                Assert.That(AssetDatabase.GetAssetPath(stageLevelLabel.font), Is.EqualTo(DisplayFontPath));
                Assert.That(runtimeLabels.Any(label => label.name == "GameOverTitle"), Is.False,
                    "The dimensional GAME OVER art must not regress to flat live text.");

                Image titleImage = popup.GetComponentsInChildren<Image>(true)
                    .Single(image => image.name == "GameOverTitle");
                Assert.That(AssetDatabase.GetAssetPath(titleImage.sprite), Is.EqualTo(GameOverTitleSpritePath));
                TextureImporter titleImporter = AssetImporter.GetAtPath(GameOverTitleSpritePath) as TextureImporter;
                Assert.That(titleImporter, Is.Not.Null);
                Assert.That(titleImporter.textureType, Is.EqualTo(TextureImporterType.Sprite));
                Assert.That(titleImporter.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
                Assert.That(titleImporter.alphaIsTransparency, Is.True);
                Assert.That(titleImporter.mipmapEnabled, Is.False);
                Assert.That(titleImporter.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));

                StorePricePillView retryPricePill = popup.GetComponentInChildren<StorePricePillView>(true);
                Assert.That(retryPricePill, Is.Not.Null);
                Assert.That(retryPricePill.AmountText, Is.Not.Null);
                Assert.That(retryPricePill.AmountText.text, Is.EqualTo("50"));
                GameObject pricePillSource = PrefabUtility.GetCorrespondingObjectFromSource(retryPricePill.gameObject);
                Assert.That(pricePillSource, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(pricePillSource), Is.EqualTo(PricePillPrefabPath));
                Assert.That(retryPricePill.FillImage.gameObject.activeSelf, Is.False,
                    "The generated Retry surface owns the inset pocket; shared price-pill fill must not be layered over it.");
                Assert.That(retryPricePill.BorderImage.gameObject.activeSelf, Is.False,
                    "The generated Retry surface owns the inset pocket; shared price-pill border must not be layered over it.");

                Assert.That(
                    runtimeLabels.Where(label => label != retryPricePill.AmountText)
                        .All(label => AssetDatabase.GetAssetPath(label.font) == DisplayFontPath),
                    Is.True,
                    "Progress and action labels must use the illustrated Main Menu display font.");

                Transform blackBackground = popup.transform.Find("BlackBackground");
                Assert.That(blackBackground, Is.Not.Null);
                Image scrim = blackBackground.GetComponent<Image>();
                Assert.That(scrim, Is.Not.Null);
                Assert.That(scrim.color.a, Is.InRange(0.8f, 0.84f));
                Assert.That(scrim.raycastTarget, Is.True);

                Transform holder = popup.transform.Find("Holder");
                Assert.That(holder, Is.Not.Null);
                Image holderImage = holder.GetComponent<Image>();
                Assert.That(holderImage, Is.Not.Null);
                Assert.That(holderImage.sprite, Is.Null);
                Assert.That(holderImage.color.a, Is.LessThanOrEqualTo(0.001f));

                Transform failureContent = popup.transform.Find("Holder/Content/FailureContent");
                Assert.That(failureContent, Is.Not.Null);
                Assert.That(stageLevelLabel.transform.IsChildOf(failureContent), Is.True);
                Assert.That(titleImage.transform.IsChildOf(failureContent), Is.True);
                Assert.That(retryPricePill.transform.IsChildOf(failureContent), Is.True);
                Assert.That(
                    popup.GetComponentsInChildren<Image>(true).Any(image => image.name == "NeutralGameOverShell"),
                    Is.False,
                    "Game Over must float over the scrim without an enclosing modal frame.");
                Assert.That(popup.GetComponent<PopupFadeInAnimation>(), Is.Not.Null);

                StageConfig[] activeStageConfigs = AssetDatabase
                    .FindAssets("t:StageConfig", new[] { "Assets/Configurations/Worlds" })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => !path.Contains("/Stages(OLD)/", StringComparison.Ordinal))
                    .Select(AssetDatabase.LoadAssetAtPath<StageConfig>)
                    .Where(stage => stage != null)
                    .ToArray();
                Assert.That(activeStageConfigs, Is.Not.Empty);
                Assert.That(activeStageConfigs.All(stage =>
                    stage.ContinueCosts is { Count: > 0 } && stage.ContinueCosts[0] > 0), Is.True);
                Assert.That(
                    popup.GetComponentsInChildren<Image>(true)
                        .Any(image => image.sprite != null && AssetDatabase.GetAssetPath(image.sprite) == LegacyFailureAtlasPath),
                    Is.False,
                    "The rejected baked title/button atlas must not return to the popup.");

                FailureKnifeSpriteAnimator knifeAnimator = popup.GetComponentInChildren<FailureKnifeSpriteAnimator>(true);
                Assert.That(knifeAnimator, Is.Not.Null);
                SerializedObject knifeAnimatorObject = new SerializedObject(knifeAnimator);
                SerializedProperty frames = knifeAnimatorObject.FindProperty("_Frames");
                Assert.That(frames.arraySize, Is.EqualTo(16));
                for (int index = 0; index < frames.arraySize; index++)
                {
                    Sprite frame = frames.GetArrayElementAtIndex(index).objectReferenceValue as Sprite;
                    Assert.That(frame, Is.Not.Null);
                    Assert.That(AssetDatabase.GetAssetPath(frame), Is.EqualTo(KnifeAnimationAtlasPath));
                    Assert.That(frame.name, Is.EqualTo($"FailureKnives_{index:00}"));
                }

                Assert.That(
                    popup.GetComponentsInChildren<UiElementAnimator>(true),
                    Has.Length.EqualTo(6),
                    "Progress, title, glow, impact art, Retry, and Menu must retain their presentation owners.");

                Image impactGlow = popup.GetComponentsInChildren<Image>(true)
                    .Single(image => image.name == "ImpactGlow");
                Assert.That(AssetDatabase.GetAssetPath(impactGlow.sprite), Is.EqualTo(ImpactGlowPath));
                Assert.That(impactGlow.raycastTarget, Is.False);

                Button[] buttons = popup.GetComponentsInChildren<Button>(true);
                Assert.That(buttons, Has.Length.EqualTo(2));
                CollectionAssert.AreEquivalent(
                    new[] { "RetryButton", "MenuButton" },
                    buttons.Select(button => button.name));

                foreach (Button button in buttons)
                {
                    TouchTargetPadding padding = button.GetComponent<TouchTargetPadding>();
                    Assert.That(padding, Is.Not.Null, $"{button.name} lost its shared touch-target padding.");
                    Assert.That(padding.MinimumSizeDp, Is.GreaterThanOrEqualTo(48f));

                    GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(button.gameObject);
                    Assert.That(source, Is.Not.Null, $"{button.name} lost its prefab link.");
                    Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(ActionButtonPrefabPath));

                    Image surface = button.transform.Find("VisualRoot").GetComponent<Image>();
                    TMP_Text label = button.transform.Find("VisualRoot/Label").GetComponent<TMP_Text>();
                    Assert.That(
                        AssetDatabase.GetAssetPath(surface.sprite),
                        Is.EqualTo(button.name == "RetryButton" ? RetryButtonSpritePath : SecondaryButtonSpritePath));
                    Assert.That(label.text, Is.EqualTo(button.name == "RetryButton" ? "RETRY" : "MENU"));
                    Assert.That(AssetDatabase.GetAssetPath(label.font), Is.EqualTo(DisplayFontPath));
                }

                GameObject popupSource = PrefabUtility.GetCorrespondingObjectFromSource(popup.gameObject);
                Assert.That(popupSource, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(popupSource), Is.EqualTo(PopupTemplatePath));

                SerializedObject popupObject = new SerializedObject(popup);
                Assert.That(popupObject.FindProperty("_StageLevelLabel").objectReferenceValue, Is.Not.Null);
                Assert.That(popupObject.FindProperty("_RetryButton").objectReferenceValue, Is.Not.Null);
                Assert.That(popupObject.FindProperty("_RetryPricePill").objectReferenceValue, Is.Not.Null);
                Assert.That(popupObject.FindProperty("_MenuButton").objectReferenceValue, Is.Not.Null);
                Assert.That(popupObject.FindProperty("_ContinueRequestedChannel").objectReferenceValue, Is.Not.Null);
                Assert.That(popupObject.FindProperty("_RetryLevelChannel"), Is.Null);
                Assert.That(popupObject.FindProperty("_RestartStageButton"), Is.Null);
                Assert.That(popupObject.FindProperty("_ContinueButton"), Is.Null);
                Assert.That(popupObject.FindProperty("_ContinueAmountText"), Is.Null);

                MonoBehaviour installer = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Single(behaviour => behaviour != null && behaviour.GetType().Name == "StageInstaller");
                SerializedObject installerObject = new SerializedObject(installer);
                Assert.That(installerObject.FindProperty("_ContinueRequestedChannel").objectReferenceValue, Is.Not.Null);
                Assert.That(installerObject.FindProperty("_ContinuePlacement").objectReferenceValue, Is.Not.Null);
                Assert.That(installerObject.FindProperty("_InterstitialPlacement").objectReferenceValue, Is.Not.Null);
            }
            finally
            {
                if (restorePrevious)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }
        }

        [Test]
        public void StageLevelLabelUsesOneBasedAuthoritativeStageData()
        {
            var root = new GameObject("StageFailPopupProgressTest", typeof(RectTransform), typeof(CanvasGroup));
            root.SetActive(false);

            try
            {
                StageFailPopup popup = root.AddComponent<StageFailPopup>();
                var labelObject = new GameObject("StageLevelLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(root.transform, false);
                TMP_Text label = labelObject.GetComponent<TMP_Text>();
                var stageData = new StageData(0, 2);
                typeof(StageData)
                    .GetProperty(nameof(StageData.CurrentLevelIndex))
                    .SetValue(stageData, 4);

                SerializedObject popupObject = new SerializedObject(popup);
                popupObject.FindProperty("_StageLevelLabel").objectReferenceValue = label;
                popupObject.ApplyModifiedPropertiesWithoutUndo();
                typeof(StageFailPopup)
                    .GetField("_StageData", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(popup, stageData);

                typeof(StageFailPopup)
                    .GetMethod("RefreshStageLevelLabel", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(popup, null);

                Assert.That(label.text, Is.EqualTo("STAGE 3  •  LEVEL 5"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PaidRetryRequestsTheTransactionOnlyOnceForDuplicateInput()
        {
            var retryChannel = ScriptableObject.CreateInstance<VoidEventChannel>();
            var root = new GameObject("StageFailPopupTest", typeof(RectTransform), typeof(CanvasGroup));
            root.SetActive(false);

            try
            {
                StageFailPopup popup = root.AddComponent<StageFailPopup>();
                Button retryButton = CreateButton("RetryButton", root.transform);
                Button menuButton = CreateButton("MenuButton", root.transform);

                SerializedObject popupObject = new SerializedObject(popup);
                popupObject.FindProperty("_RetryButton").objectReferenceValue = retryButton;
                popupObject.FindProperty("_MenuButton").objectReferenceValue = menuButton;
                popupObject.FindProperty("_ContinueRequestedChannel").objectReferenceValue = retryChannel;
                popupObject.ApplyModifiedPropertiesWithoutUndo();

                int raiseCount = 0;
                Action handler = () => raiseCount++;
                retryChannel.Subscribe(handler);

                root.SetActive(true);
                typeof(StageFailPopup)
                    .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(popup, null);
                retryButton.onClick.Invoke();
                retryButton.onClick.Invoke();

                Assert.That(raiseCount, Is.EqualTo(1));
                retryChannel.Unsubscribe(handler);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(retryChannel);
            }
        }

        [Test]
        public void RetryStatePresentsTheConfiguredCoinCost()
        {
            var root = new GameObject("StageFailPopupCostTest", typeof(RectTransform), typeof(CanvasGroup));
            root.SetActive(false);

            try
            {
                StageFailPopup popup = root.AddComponent<StageFailPopup>();
                Button retryButton = CreateButton("RetryButton", root.transform);
                GameObject priceObject = (GameObject)PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(PricePillPrefabPath));
                priceObject.transform.SetParent(retryButton.transform, false);
                StorePricePillView pricePill = priceObject.GetComponent<StorePricePillView>();

                SerializedObject popupObject = new SerializedObject(popup);
                popupObject.FindProperty("_RetryButton").objectReferenceValue = retryButton;
                popupObject.FindProperty("_RetryPricePill").objectReferenceValue = pricePill;
                popupObject.ApplyModifiedPropertiesWithoutUndo();

                popup.SetRetryState(true, true, 150);

                Assert.That(retryButton.interactable, Is.True);
                Assert.That(pricePill.AmountText.text, Is.EqualTo("150"));
                Assert.That(pricePill.IconImage.gameObject.activeSelf, Is.True);
                Assert.That(pricePill.IconImage.sprite, Is.SameAs(pricePill.CurrencyIconSprite));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PaidRetrySpendsTheConfiguredCostBeforeReloadingTheLevel()
        {
            var continueChannel = ScriptableObject.CreateInstance<VoidEventChannel>();
            var retryChannel = ScriptableObject.CreateInstance<VoidEventChannel>();
            var levelFailedChannel = ScriptableObject.CreateInstance<LevelFailedEventChannel>();
            var balanceChangedChannel = ScriptableObject.CreateInstance<CurrencyBalanceChangedEventChannel>();
            var stageConfig = ScriptableObject.CreateInstance<StageConfig>();
            var popupRoot = new GameObject("PaidRetryPopupTest", typeof(RectTransform), typeof(CanvasGroup));
            popupRoot.SetActive(false);

            try
            {
                typeof(StageConfig)
                    .GetProperty(nameof(StageConfig.ContinueCosts))
                    .SetValue(stageConfig, new List<int> { 50, 100 });
                var stageData = new StageData(0, 0);
                var currency = new FakeCurrencyService(200);
                var catalog = new FakeLevelCatalog(stageConfig);
                StageFailPopup popup = popupRoot.AddComponent<StageFailPopup>();
                var controller = new AdRewardController(
                    continueChannel,
                    retryChannel,
                    null,
                    levelFailedChannel,
                    balanceChangedChannel,
                    currency,
                    catalog,
                    stageData,
                    null,
                    popup);

                int retryCount = 0;
                Action retryHandler = () => retryCount++;
                retryChannel.Subscribe(retryHandler);
                controller.Start();

                continueChannel.Raise();

                Assert.That(currency.Balance, Is.EqualTo(150));
                Assert.That(currency.LastSpendAmount, Is.EqualTo(50));
                Assert.That(retryCount, Is.EqualTo(1));

                controller.Dispose();
                retryChannel.Unsubscribe(retryHandler);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(popupRoot);
                UnityEngine.Object.DestroyImmediate(stageConfig);
                UnityEngine.Object.DestroyImmediate(balanceChangedChannel);
                UnityEngine.Object.DestroyImmediate(levelFailedChannel);
                UnityEngine.Object.DestroyImmediate(retryChannel);
                UnityEngine.Object.DestroyImmediate(continueChannel);
            }
        }

        [Test]
        public void KnifeAnimationAdvancesInFrameOrderAndHoldsTheFinalPose()
        {
            var root = new GameObject("FailureKnifeAnimationTest", typeof(RectTransform), typeof(Image));
            root.SetActive(false);
            var texture = new Texture2D(16, 1);
            Sprite[] frames = Enumerable.Range(0, 16)
                .Select(index => Sprite.Create(texture, new Rect(index, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)))
                .ToArray();

            try
            {
                Image image = root.GetComponent<Image>();
                FailureKnifeSpriteAnimator animator = root.AddComponent<FailureKnifeSpriteAnimator>();
                SerializedObject animatorObject = new SerializedObject(animator);
                animatorObject.FindProperty("_TargetImage").objectReferenceValue = image;
                SerializedProperty frameProperty = animatorObject.FindProperty("_Frames");
                frameProperty.arraySize = frames.Length;
                for (int index = 0; index < frames.Length; index++)
                    frameProperty.GetArrayElementAtIndex(index).objectReferenceValue = frames[index];
                animatorObject.FindProperty("_StartDelay").floatValue = 0.1f;
                animatorObject.FindProperty("_SecondsPerFrame").floatValue = 0.05f;
                animatorObject.ApplyModifiedPropertiesWithoutUndo();

                root.SetActive(true);
                animator.Restart();
                Assert.That(image.sprite, Is.SameAs(frames[0]));

                animator.AdvanceAnimation(0.151f);
                Assert.That(animator.CurrentFrameIndex, Is.EqualTo(1));
                Assert.That(image.sprite, Is.SameAs(frames[1]));

                animator.AdvanceAnimation(1f);
                Assert.That(animator.CurrentFrameIndex, Is.EqualTo(15));
                Assert.That(image.sprite, Is.SameAs(frames[15]));
                Assert.That(animator.IsPlaying, Is.False);
            }
            finally
            {
                foreach (Sprite frame in frames)
                    UnityEngine.Object.DestroyImmediate(frame);
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Button CreateButton(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<Button>();
        }

        private sealed class FakeCurrencyService : ICurrencyService
        {
            public FakeCurrencyService(int balance)
            {
                Balance = balance;
            }

            public int Balance { get; private set; }
            public int LastSpendAmount { get; private set; }
            public int GetBalance(string currencyId) => Balance;
            public void AddBalance(string currencyId, int amount) => Balance += amount;
            public bool CanAfford(string currencyId, int amount) => Balance >= amount;

            public bool TrySpend(string currencyId, int amount)
            {
                LastSpendAmount = amount;
                if (!CanAfford(currencyId, amount))
                    return false;

                Balance -= amount;
                return true;
            }
        }

        private sealed class FakeLevelCatalog : ILevelCatalog
        {
            private readonly StageConfig _StageConfig;

            public FakeLevelCatalog(StageConfig stageConfig)
            {
                _StageConfig = stageConfig;
            }

            public int StageCount => 1;
            public int GetLevelCount(int stageIndex) => _StageConfig.ContinueCosts.Count;
            public LevelConfig GetLevelConfig(int stageIndex, int levelIndex) => null;
            public StageConfig GetStageConfig(int stageIndex) => _StageConfig;
        }
    }
}
