using System;
using System.Linq;
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

                AssertRect(safe.Find("ContinueButton") as RectTransform, new Vector2(720f, 176f), new Vector2(0f, 272f));
                AssertRect(safe.Find("StagesButton") as RectTransform, new Vector2(510f, 112f), new Vector2(0f, 112f));
                Assert.That(safe.Find("StageSign").gameObject.activeSelf, Is.False);
                AssertRect(safe.Find("KnifeSkinSlelector") as RectTransform, new Vector2(920f, 560f), new Vector2(0f, -245f));

                AssertRect(safe.Find("MenuButton") as RectTransform, new Vector2(128f, 128f), new Vector2(96f, -96f));
                AssertRect(safe.Find("ShopButton") as RectTransform, new Vector2(152f, 188f), new Vector2(104f, -280f));
                AssertRect(safe.Find("DailyMissionButton") as RectTransform, new Vector2(152f, 188f), new Vector2(104f, -478f));
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

                AssertIconButton(safe.Find("MenuButton/Holder/Button"), "IconSettings", tokens, usesReferenceSurface: true);
                AssertIconButton(safe.Find("ShopButton"), "IconStore", tokens, usesReferenceSurface: true);
                AssertIconButton(safe.Find("DailyMissionButton"), "IconMissions", tokens, usesReferenceSurface: true);
                AssertIconButton(safe.Find("RemoveAdsButton"), "IconRemoveAds", tokens, usesReferenceSurface: true);
                AssertIconButton(safe.Find("KnifeSkinSlelector/LeftButton"), "IconPrevious", tokens);
                AssertIconButton(safe.Find("KnifeSkinSlelector/RightButton"), "IconNext", tokens);

                Image currencyIcon = safe.Find("CoinUiDisaply/CoinIcon").GetComponent<Image>();
                Assert.That(currencyIcon, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(currencyIcon.sprite),
                    Is.EqualTo("Assets/Art/UI/Icons/IconCurrency.png"));
                Assert.That(currencyIcon.raycastTarget, Is.False);

                Image currencySurface = safe.Find("CoinUiDisaply").GetComponent<Image>();
                Image stageSurface = safe.Find("StageSign").GetComponent<Image>();
                Assert.That(AssetDatabase.GetAssetPath(currencySurface.sprite),
                    Is.EqualTo("Assets/Art/UI/MainMenuReference/MainMenuCurrencyBar.png"));
                Assert.That(stageSurface.sprite, Is.EqualTo(tokens.Buttons.GetSurface(UiButtonStyle.Price)));

                TMP_FontAsset actionFont = tokens.Typography.Get(UiTypographyRole.Action).FontAsset;
                Assert.That(safe.Find("ContinueButton").GetComponentInChildren<TMP_Text>(true).font, Is.EqualTo(actionFont));
                Assert.That(safe.Find("StagesButton").GetComponentInChildren<TMP_Text>(true).font, Is.EqualTo(actionFont));
                Assert.That(safe.Find("StageSign").GetComponentInChildren<TMP_Text>(true).font, Is.EqualTo(actionFont));

                string[] newlyProducedChromePaths =
                {
                    "KnifeSkinSlelector/LeftButton/PresentationRoot/Surface",
                    "KnifeSkinSlelector/RightButton/PresentationRoot/Surface",
                    "StageSign"
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
                    "MenuButton/Holder/Button/PresentationRoot/Surface",
                    "ShopButton/PresentationRoot/Surface",
                    "DailyMissionButton/PresentationRoot/Surface",
                    "RemoveAdsButton/PresentationRoot/Surface",
                    "CoinUiDisaply",
                    "ContinueButton/PresentationRoot/Surface",
                    "StagesButton/PresentationRoot/Surface",
                    "KnifeSkinSlelector/ReferenceSelectorFrame"
                };
                foreach (string path in referenceChromePaths)
                {
                    Image image = safe.Find(path).GetComponent<Image>();
                    Assert.That(AssetDatabase.GetAssetPath(image.sprite),
                        Does.StartWith("Assets/Art/UI/MainMenuReference/"));
                }

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
            Assert.That(presenter.SkinStyle, Is.EqualTo(UiButtonStyle.IconOnly));
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
