using System;
using System.Linq;
using DG.Tweening;
using Game.UI.Common;
using Game.UI.MainMenu;
using Game.UI.Tokens;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI.Tests
{
    public sealed class MainMenuModernForestArcadeTests
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        [Test]
        public void MainMenu_UsesTheApprovedSafeHierarchyAndExactActionGeometry()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                Transform safe = Find(scene, "Canvas/Layer1/SafeAreaRoot");
                Assert.That(safe, Is.Not.Null);

                AssertRect(safe.Find("ContinueButton") as RectTransform, new Vector2(600f, 136f), new Vector2(0f, 256f));
                AssertRect(safe.Find("StagesButton") as RectTransform, new Vector2(510f, 112f), new Vector2(0f, 112f));
                AssertRect(safe.Find("Logo") as RectTransform, new Vector2(773.682f, 490.02905f), new Vector2(0f, -345f));
                AssertRect(safe.Find("Logo/Toss") as RectTransform, new Vector2(444f, 185f), Vector2.zero);
                AssertRect(safe.Find("Logo/Madness") as RectTransform, new Vector2(529f, 144f), new Vector2(0f, -164f));
                Assert.That(safe.Find("StageSign").gameObject.activeSelf, Is.True,
                    "The wooden stage sign is part of the Main Menu composition.");
                AssertRect(safe.Find("StageSign") as RectTransform, new Vector2(320f, 320f), new Vector2(232f, 488f));
                AssertRect(safe.Find("KnifeSkinSlelector") as RectTransform, new Vector2(920f, 560f), new Vector2(0f, -245f));
                AssertRect(safe.Find("KnifeSkinSlelector/LeftButton") as RectTransform,
                    new Vector2(128f, 128f), new Vector2(-250f, 180f));
                AssertRect(safe.Find("KnifeSkinSlelector/RightButton") as RectTransform,
                    new Vector2(128f, 128f), new Vector2(250f, 180f));
                Assert.That(safe.Find("KnifeSkinSlelector/GiftBox_Effect_8").gameObject.activeSelf,
                    Is.False, "Knife-selector particles are intentionally disabled.");

                SerializedObject logoAnimator = new SerializedObject(safe.Find("Logo").GetComponent<UiElementAnimator>());
                Assert.That(logoAnimator.FindProperty("_IdleAnimation._Mode").enumValueIndex,
                    Is.EqualTo(2), "The original per-word logo idle animation must remain enabled.");
                Assert.That(Find(scene, "BackgroundEffectHolder").gameObject.activeSelf,
                    Is.False, "World-themed Main Menu particles are intentionally disabled.");

                AssertRect(safe.Find("MenuButton") as RectTransform, new Vector2(128f, 128f), new Vector2(96f, -96f));
                AssertRect(safe.Find("ShopButton") as RectTransform, new Vector2(128f, 128f), new Vector2(96f, -256f));
                AssertRect(safe.Find("DailyMissionButton") as RectTransform, new Vector2(128f, 128f), new Vector2(96f, -416f));
                Assert.That(safe.Find("ShopButton/PresentationRoot/MenuLabel").gameObject.activeSelf, Is.False);
                Assert.That(safe.Find("DailyMissionButton/PresentationRoot/MenuLabel").gameObject.activeSelf, Is.False);
                AssertRect(safe.Find("ShopButton/PresentationRoot/Glyph") as RectTransform,
                    new Vector2(90f, 90f), Vector2.zero);
                AssertRect(safe.Find("DailyMissionButton/PresentationRoot/Glyph") as RectTransform,
                    new Vector2(90f, 90f), Vector2.zero);
                AssertRect(safe.Find("CoinUiDisaply") as RectTransform, new Vector2(384f, 112f), new Vector2(-220f, -92f));
                AssertRect(safe.Find("RemoveAdsButton") as RectTransform, new Vector2(124f, 124f), new Vector2(-94f, -254f));

                Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)).Count(), Is.EqualTo(1));
                Assert.That(safe.GetComponentsInChildren<RectTransform>(true).All(rect => rect == safe || rect.IsChildOf(safe)), Is.True);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void MainMenu_ChromeUsesIllustratedSpritesAndApprovedSemanticSurfaces()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                Transform safe = Find(scene, "Canvas/Layer1/SafeAreaRoot");
                UiSemanticTokens tokens = AssetDatabase.LoadAssetAtPath<UiSemanticTokens>(UiSemanticTokens.ProductionAssetPath);
                Assert.That(tokens, Is.Not.Null);

                AssertIconButton(safe.Find("MenuButton/Holder/Button"), "IconSettings", tokens);
                AssertIconButton(safe.Find("ShopButton"), "IconStore", tokens);
                AssertIconButton(safe.Find("DailyMissionButton"), "IconMissions", tokens);
                AssertIconButton(safe.Find("RemoveAdsButton"), "IconRemoveAds", tokens);
                AssertIconButton(safe.Find("KnifeSkinSlelector/LeftButton"), "IconPrevious", tokens);
                AssertIconButton(safe.Find("KnifeSkinSlelector/RightButton"), "IconNext", tokens);

                Image currencyIcon = safe.Find("CoinUiDisaply/CoinIcon").GetComponent<Image>();
                Assert.That(currencyIcon, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(currencyIcon.sprite),
                    Is.EqualTo("Assets/Art/UI/Icons/IconCurrency.png"));
                Assert.That(currencyIcon.raycastTarget, Is.False);
                AssertRect(safe.Find("CoinUiDisaply/CoinIcon") as RectTransform,
                    new Vector2(108f, 108f), new Vector2(-54f, -15f));

                TMP_Text balanceText = safe.Find("CoinUiDisaply/CountText").GetComponent<TMP_Text>();
                Assert.That(AssetDatabase.GetAssetPath(balanceText.font),
                    Is.EqualTo("Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular SDF.asset"));
                Assert.That(balanceText.horizontalAlignment, Is.EqualTo(HorizontalAlignmentOptions.Center));
                Assert.That(balanceText.verticalAlignment, Is.EqualTo(VerticalAlignmentOptions.Middle));

                Image currencySurface = safe.Find("CoinUiDisaply").GetComponent<Image>();
                Image stageSurface = safe.Find("StageSign").GetComponent<Image>();
                Assert.That(AssetDatabase.GetAssetPath(currencySurface.sprite),
                    Is.EqualTo("Assets/Art/UI/MainMenuReference/MainMenuCurrencyBar.png"));
                Assert.That(AssetDatabase.GetAssetPath(stageSurface.sprite),
                    Is.EqualTo("Assets/Art/stage_sign.png"));

                TMP_Text stageLabel = safe.Find("StageSign/Text (TMP)").GetComponent<TMP_Text>();
                Assert.That(AssetDatabase.GetAssetPath(stageLabel.font),
                    Is.EqualTo("Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular SDF.asset"));
                Assert.That(stageLabel.horizontalAlignment, Is.EqualTo(HorizontalAlignmentOptions.Center));
                Assert.That(stageLabel.verticalAlignment, Is.EqualTo(VerticalAlignmentOptions.Middle));

                TMP_FontAsset actionFont = tokens.Typography.Get(UiTypographyRole.Action).FontAsset;
                TMP_Text playLabel = safe.Find("ContinueButton").GetComponentInChildren<TMP_Text>(true);
                Assert.That(playLabel.font, Is.EqualTo(actionFont));
                Assert.That(playLabel.fontSizeMax, Is.EqualTo(66f).Within(0.01f));
                Assert.That(playLabel.margin, Is.EqualTo(Vector4.zero));
                Assert.That(safe.Find("StagesButton").GetComponentInChildren<TMP_Text>(true).font, Is.EqualTo(actionFont));

                string[] sceneOwnedStaticButtons =
                {
                    "ContinueButton",
                    "StagesButton",
                    "MenuButton/Holder/Button",
                    "ShopButton",
                    "DailyMissionButton",
                    "RemoveAdsButton",
                    "KnifeSkinSlelector/LeftButton",
                    "KnifeSkinSlelector/RightButton"
                };
                foreach (string path in sceneOwnedStaticButtons)
                {
                    ActionButtonPresenter presenter = safe.Find(path).GetComponent<ActionButtonPresenter>();
                    Assert.That(presenter.SkinStyle, Is.EqualTo(UiButtonStyle.Unskinned),
                        $"{path} must preserve its scene-authored artwork at runtime.");
                    SerializedObject presenterState = new SerializedObject(presenter);
                    Assert.That(presenterState.FindProperty("_SemanticTokens").objectReferenceValue, Is.Null,
                        $"{path} must not replace static scene artwork from semantic tokens at runtime.");
                }

                string[] newlyProducedChromePaths =
                {
                    "MenuButton/Holder/Button/PresentationRoot/Surface",
                    "ShopButton/PresentationRoot/Surface",
                    "DailyMissionButton/PresentationRoot/Surface",
                    "RemoveAdsButton/PresentationRoot/Surface",
                    "KnifeSkinSlelector/LeftButton/PresentationRoot/Surface",
                    "KnifeSkinSlelector/RightButton/PresentationRoot/Surface",
                    "ContinueButton/PresentationRoot/Surface",
                    "StagesButton/PresentationRoot/Surface"
                };
                foreach (string path in newlyProducedChromePaths)
                {
                    Image image = safe.Find(path).GetComponent<Image>();
                    string assetPath = AssetDatabase.GetAssetPath(image.sprite);
                    Assert.That(assetPath, Does.StartWith("Assets/Art/UI/Core/"), $"{path} must not ship blocked legacy pixels.");
                    Assert.That(assetPath, Does.Not.Contain("Layer Lab"));
                }

                string[] referenceChromePaths =
                {
                    "CoinUiDisaply"
                };
                foreach (string path in referenceChromePaths)
                {
                    Image image = safe.Find(path).GetComponent<Image>();
                    Assert.That(AssetDatabase.GetAssetPath(image.sprite),
                        Does.StartWith("Assets/Art/UI/MainMenuReference/"));
                }

                Assert.That(safe.Find("KnifeSkinSlelector/ReferenceSelectorFrame").gameObject.activeSelf,
                    Is.False, "The high-contrast selector frame is intentionally disabled.");

                string[] illustratedIconPaths =
                {
                    "MenuButton/Holder/Button/PresentationRoot/Glyph",
                    "ShopButton/PresentationRoot/Glyph",
                    "DailyMissionButton/PresentationRoot/Glyph",
                    "RemoveAdsButton/PresentationRoot/Glyph",
                    "KnifeSkinSlelector/LeftButton/PresentationRoot/Glyph",
                    "KnifeSkinSlelector/RightButton/PresentationRoot/Glyph",
                    "CoinUiDisaply/CoinIcon"
                };
                foreach (string path in illustratedIconPaths)
                {
                    Image image = safe.Find(path).GetComponent<Image>();
                    Assert.That(image, Is.Not.Null, $"{path} must use the production illustrated icon component.");
                    Assert.That(AssetDatabase.GetAssetPath(image.sprite), Does.StartWith("Assets/Art/UI/Icons/"));
                    Assert.That(image.preserveAspect, Is.True);
                    Assert.That(image.raycastTarget, Is.False);
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void MainMenu_ControllerReferencesRemainBehaviorOwnedAndSelectorHidesInvalidCycling()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                Transform safe = Find(scene, "Canvas/Layer1/SafeAreaRoot");
                MainMenuController controller = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MainMenuController>(true))
                    .Single();
                var serialized = new SerializedObject(controller);

                Assert.That(serialized.FindProperty("_PlayButton").objectReferenceValue, Is.EqualTo(safe.Find("ContinueButton").GetComponent<Button>()));
                Assert.That(serialized.FindProperty("_StagesButton").objectReferenceValue, Is.EqualTo(safe.Find("StagesButton").GetComponent<Button>()));
                Assert.That(serialized.FindProperty("_ShopButton").objectReferenceValue, Is.EqualTo(safe.Find("ShopButton").GetComponent<Button>()));
                Assert.That(serialized.FindProperty("_MissionsButton").objectReferenceValue, Is.EqualTo(safe.Find("DailyMissionButton").GetComponent<Button>()));
                Assert.That(serialized.FindProperty("_RemoveAdsButton").objectReferenceValue, Is.EqualTo(safe.Find("RemoveAdsButton").GetComponent<Button>()));
                Assert.That(serialized.FindProperty("_CurrentStageText").objectReferenceValue, Is.EqualTo(safe.Find("StageSign").GetComponentInChildren<TMP_Text>(true)));

                KnifeSkinSelectorView selector = safe.Find("KnifeSkinSlelector").GetComponent<KnifeSkinSelectorView>();
                Button previous = safe.Find("KnifeSkinSlelector/LeftButton").GetComponent<Button>();
                Button next = safe.Find("KnifeSkinSlelector/RightButton").GetComponent<Button>();

                selector.SetDisplay(null, canCycle: false, canInteract: true);
                Assert.That(previous.gameObject.activeSelf, Is.False);
                Assert.That(next.gameObject.activeSelf, Is.False);

                selector.SetDisplay(null, canCycle: true, canInteract: false);
                Assert.That(previous.gameObject.activeSelf, Is.True);
                Assert.That(next.gameObject.activeSelf, Is.True);
                Assert.That(previous.interactable, Is.False);
                Assert.That(next.interactable, Is.False);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void KnifeSelector_ArrowPressLocksInputAndCompletesCarouselSwap()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            KnifeSkinSelectorView selector = null;
            try
            {
                Transform safe = Find(scene, "Canvas/Layer1/SafeAreaRoot");
                selector = safe.Find("KnifeSkinSlelector").GetComponent<KnifeSkinSelectorView>();
                Image preview = safe.Find("KnifeSkinSlelector/PreviewImage").GetComponent<Image>();
                Button previous = safe.Find("KnifeSkinSlelector/LeftButton").GetComponent<Button>();
                Button next = safe.Find("KnifeSkinSlelector/RightButton").GetComponent<Button>();
                Sprite initialSprite = preview.sprite;
                Sprite replacementSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icons/IconStore.png");
                Vector2 basePosition = preview.rectTransform.anchoredPosition;
                Vector3 baseScale = preview.rectTransform.localScale;
                Color baseColor = preview.color;
                int swaps = 0;

                Assert.That(initialSprite, Is.Not.Null);
                Assert.That(replacementSprite, Is.Not.Null);

                SerializedObject selectorState = new SerializedObject(selector);
                selectorState.FindProperty("_PreviewFloatDistance").floatValue = 0f;
                selectorState.ApplyModifiedPropertiesWithoutUndo();

                selector.SetDisplay(initialSprite, canCycle: true, canInteract: true);
                selector.Bind(null, () =>
                {
                    swaps++;
                    selector.SetDisplay(replacementSprite, canCycle: true, canInteract: true);
                });

                next.onClick.Invoke();

                Assert.That(selector.IsSwapAnimating, Is.True);
                Assert.That(swaps, Is.Zero, "The knife must change only after the outgoing preview clears the center.");
                Assert.That(previous.interactable, Is.True, "Arrow artwork must stay in its normal visible state during a swap.");
                Assert.That(next.interactable, Is.True, "Arrow artwork must stay in its normal visible state during a swap.");

                next.onClick.Invoke();

                DOTween.Complete(selector, withCallbacks: true);

                Assert.That(swaps, Is.EqualTo(1), "Repeated presses must not interrupt or stack carousel swaps.");
                Assert.That(preview.sprite, Is.EqualTo(replacementSprite));
                Assert.That(selector.IsSwapAnimating, Is.False);
                Assert.That(previous.interactable, Is.True);
                Assert.That(next.interactable, Is.True);
                Assert.That(preview.rectTransform.anchoredPosition, Is.EqualTo(basePosition));
                Assert.That(preview.rectTransform.localScale, Is.EqualTo(baseScale));
                Assert.That(preview.color, Is.EqualTo(baseColor));
            }
            finally
            {
                if (selector != null)
                {
                    DOTween.Kill(selector);
                }

                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void AssertIconButton(
            Transform transform,
            string expectedSpriteName,
            UiSemanticTokens tokens,
            bool usesReferenceSurface = false)
        {
            Button button = transform.GetComponent<Button>();
            ActionButtonPresenter presenter = transform.GetComponent<ActionButtonPresenter>();
            Transform presentation = transform.Find("PresentationRoot");
            Image surface = presentation.Find("Surface").GetComponent<Image>();
            Image glyph = presentation.Find("Glyph").GetComponent<Image>();

            Assert.That(button, Is.Not.Null);
            Assert.That(presenter, Is.Not.Null);
            Assert.That(presenter.SkinStyle, Is.EqualTo(UiButtonStyle.Unskinned));
            if (usesReferenceSurface)
            {
                Assert.That(AssetDatabase.GetAssetPath(surface.sprite),
                    Does.StartWith("Assets/Art/UI/MainMenuReference/"));
            }
            else
            {
                Assert.That(surface.sprite, Is.EqualTo(tokens.Buttons.GetSurface(UiButtonStyle.IconOnly)));
            }
            Assert.That(glyph.sprite.name, Is.EqualTo(expectedSpriteName));
            Assert.That(AssetDatabase.GetAssetPath(glyph.sprite),
                Is.EqualTo($"Assets/Art/UI/Icons/{expectedSpriteName}.png"));
            Assert.That(glyph.preserveAspect, Is.True);
            Assert.That(glyph.raycastTarget, Is.False);
            Assert.That((transform as RectTransform).rect.width, Is.GreaterThanOrEqualTo(124f));
            Assert.That((transform as RectTransform).rect.height, Is.GreaterThanOrEqualTo(124f));
        }

        private static void AssertRect(RectTransform rect, Vector2 size, Vector2 anchoredPosition)
        {
            Assert.That(rect, Is.Not.Null);
            Assert.That(rect.sizeDelta.x, Is.EqualTo(size.x).Within(0.01f));
            Assert.That(rect.sizeDelta.y, Is.EqualTo(size.y).Within(0.01f));
            Assert.That(rect.anchoredPosition.x, Is.EqualTo(anchoredPosition.x).Within(0.01f));
            Assert.That(rect.anchoredPosition.y, Is.EqualTo(anchoredPosition.y).Within(0.01f));
        }

        private static Transform Find(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            Transform current = scene.GetRootGameObjects().Single(root => root.name == parts[0]).transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }
    }
}
