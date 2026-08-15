using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Store.Data;
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
    public sealed class StoreNeutralProductionTests
    {
        private const string ScenePath = "Assets/Scenes/StoreScene.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string ScreenPrefabPath = "Assets/Prefabs/UI/Store/StoreScreenView.prefab";
        private const string BalancePrefabPath = "Assets/Prefabs/UI/Coin/CoinUiDisaply.prefab";
        private const string KnifePrefabPath = "Assets/Prefabs/UI/Store/KnifeItemCardView.prefab";
        private const string PowerPrefabPath = "Assets/Prefabs/UI/Store/PowerUpItemCardView.prefab";
        private const string PricePrefabPath = "Assets/Prefabs/UI/Store/PricePillView.prefab";
        private const string StatusPrefabPath = "Assets/Prefabs/UI/Store/StoreStatusPillView.prefab";
        private const string NavigationPrefabPath = "Assets/Prefabs/UI/Store/StoreNavigationButton.prefab";
        private const string SectionPrefabPath = "Assets/Prefabs/UI/Store/StoreSectionHeaderView.prefab";
        private const string WatchAdPrefabPath = "Assets/Prefabs/UI/Store/StoreWatchAdCtaView.prefab";
        private const string RemoveAdsPrefabPath = "Assets/Prefabs/UI/Store/StoreRemoveAdsView.prefab";
        private const string ApprovedRoot = "Assets/Art/UI/StoreApproved/";
        private const string ExistingSecondarySurface = "Assets/Art/UI/Core/ButtonSecondary.png";

        [Test]
        public void StoreScene_IsOneComposedScreenPrefabWithNestedReusableElements()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                Transform safe = Find(scene, "Canvas/SafeAreaRoot");
                Assert.That(safe, Is.Not.Null);
                Assert.That(safe.GetComponent<StoreScreenView>(), Is.Not.Null);
                Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(safe.gameObject),
                    Is.EqualTo(ScreenPrefabPath));

                AssertNestedScenePrefab(safe.Find("BackButton"), NavigationPrefabPath);
                AssertNestedScenePrefab(safe.Find("KnifeSectionLabel"), SectionPrefabPath);
                AssertNestedScenePrefab(safe.Find("PowerUpSectionLabel"), SectionPrefabPath);
                AssertNestedScenePrefab(safe.Find("BottomActions/WatchAdAction"), WatchAdPrefabPath);
                AssertNestedScenePrefab(safe.Find("BottomActions/RemoveAdsAction"), RemoveAdsPrefabPath);
                AssertNestedScenePrefab(safe.Find("StoreBalanceChip"), BalancePrefabPath);

                Assert.That(TextAt(safe, "StoreTitlePlaque/TitleText"), Is.EqualTo("STORE"));
                Assert.That(TextAt(safe, "KnifeSectionLabel/Label"), Is.EqualTo("KNIVES"));
                Assert.That(TextAt(safe, "PowerUpSectionLabel/Label"), Is.EqualTo("POWER-UPS"));
                Assert.That(safe.Find("StoreBalanceChip").GetComponent<StoreBalanceChipView>(), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void StoreBalance_ReusesMainMenuPrefabWithoutVisualOverridesAndIsWired()
        {
            Scene storeScene = EditorSceneManager.OpenPreviewScene(ScenePath);
            Scene mainMenuScene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            GameObject screen = PrefabUtility.LoadPrefabContents(ScreenPrefabPath);
            try
            {
                StoreBalanceChipView storeBalance = Find(storeScene, "Canvas/SafeAreaRoot/StoreBalanceChip")
                    .GetComponent<StoreBalanceChipView>();
                StoreBalanceChipView mainMenuBalance = mainMenuScene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<StoreBalanceChipView>(true))
                    .Single();

                Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(storeBalance.gameObject),
                    Is.EqualTo(BalancePrefabPath));
                Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(mainMenuBalance.gameObject),
                    Is.EqualTo(BalancePrefabPath));
                Assert.That(VisualSignature(storeBalance.transform), Is.EqualTo(VisualSignature(mainMenuBalance.transform)));

                Transform nestedBalance = screen.transform.Find("StoreBalanceChip");
                PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(nestedBalance.gameObject);
                Assert.That(modifications.Where(modification =>
                        modification?.target is Graphic || modification?.target is TMP_Text),
                    Is.Empty,
                    "The Store may override placement, but all balance visuals must come from the shared Main Menu prefab.");

                RectTransform nestedRect = (RectTransform)nestedBalance;
                Assert.That(nestedRect.sizeDelta, Is.EqualTo(new Vector2(384f, 112f)));
                Assert.That(nestedRect.localScale, Is.EqualTo(new Vector3(0.86f, 0.86f, 1f)));

                StoreMenuController controller = storeScene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<StoreMenuController>(true))
                    .Single();
                SerializedObject controllerData = new SerializedObject(controller);
                Assert.That(controllerData.FindProperty("_StoreBalanceChipView").objectReferenceValue,
                    Is.EqualTo(storeBalance));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(screen);
                EditorSceneManager.ClosePreviewScene(mainMenuScene);
                EditorSceneManager.ClosePreviewScene(storeScene);
            }
        }

        [Test]
        public void StoreScene_WiresCanonicalCardPrefabsAndDoesNotOwnAStageBackground()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                StoreMenuController controller = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<StoreMenuController>(true))
                    .Single();
                Assert.That(AssetDatabase.GetAssetPath(controller.KnifeSkinCardPrefab), Is.EqualTo(KnifePrefabPath));
                Assert.That(AssetDatabase.GetAssetPath(controller.PowerUpCardPrefab), Is.EqualTo(PowerPrefabPath));

                Transform safe = Find(scene, "Canvas/SafeAreaRoot");
                Assert.That(controller.KnifeSkinsContentRoot, Is.EqualTo(safe.Find("KnifeCarousel/CardPool")));
                Assert.That(safe.Find("Background"), Is.Null,
                    "The Store must inherit the current stage background instead of authoring a themed one.");

                foreach (Image image in safe.GetComponentsInChildren<Image>(true))
                {
                    string path = AssetDatabase.GetAssetPath(image.sprite);
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    Assert.That(path, Does.Not.Match("(?i)(leaf|leaves|flower|vine|phase|worlds?/|biome)"),
                        $"Store-owned UI must stay theme-neutral: {path}");
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void StoreCards_NestTheSamePriceAndStatusPrefabSources()
        {
            AssertNestedCardPrefab(KnifePrefabPath, "PricePill", PricePrefabPath);
            AssertNestedCardPrefab(PowerPrefabPath, "PricePill", PricePrefabPath);
            AssertNestedCardPrefab(KnifePrefabPath, "StatusPill", StatusPrefabPath);
            AssertNestedCardPrefab(PowerPrefabPath, "OwnedPill", StatusPrefabPath);

            GameObject price = AssetDatabase.LoadAssetAtPath<GameObject>(PricePrefabPath);
            StorePricePillView priceView = price.GetComponent<StorePricePillView>();
            Assert.That(priceView, Is.Not.Null);
            Assert.That(priceView.AmountText, Is.Not.Null);
            Assert.That(priceView.AmountText, Is.TypeOf<TextMeshProUGUI>());
            Assert.That(price.GetComponent<Image>(), Is.Null,
                "The card art already contains its price plaque; the shared price prefab must only provide coin/text content.");

            AssertCardUsesIntegratedPricePlaque(KnifePrefabPath, "StoreKnifeCard.png", -135f);
            AssertCardUsesIntegratedPricePlaque(PowerPrefabPath, "StorePowerUpCard.png", -171f);
            AssertPowerUpContentStackHasNoOverlaps();
        }

        [Test]
        public void KnifeCard_UsesScaleSelectionWithoutADecorativeGlow()
        {
            GameObject knife = AssetDatabase.LoadAssetAtPath<GameObject>(KnifePrefabPath);
            Assert.That(knife.transform.Find("SelectedGlow"), Is.Null);
            Assert.That(knife.GetComponentsInChildren<Image>(true)
                .Any(image => image.name.Contains("Glow", StringComparison.OrdinalIgnoreCase)), Is.False);
        }

        [Test]
        public void KnifeCarousel_UsesTwoSlideContainersAndNeverExceedsSixCardsForOneThousandEntries()
        {
            GameObject screenPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ScreenPrefabPath);
            GameObject knifePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KnifePrefabPath);
            GameObject screen = UnityEngine.Object.Instantiate(screenPrefab);

            try
            {
                StoreKnifeCarouselView carousel = screen.GetComponentInChildren<StoreKnifeCarouselView>(true);
                KnifeSkinStoreCardView cardPrefab = knifePrefab.GetComponent<KnifeSkinStoreCardView>();
                var entries = new List<PurchasableEntry>(1000);
                for (var i = 0; i < 1000; i++)
                {
                    entries.Add(new PurchasableEntry());
                }

                carousel.Configure(entries, cardPrefab, null, null);
                Assert.That(carousel.EntryCount, Is.EqualTo(1000));
                Assert.That(carousel.PageCount, Is.EqualTo(334));
                Assert.That(carousel.ContainerPoolSize, Is.EqualTo(2));
                Assert.That(carousel.PoolCapacity, Is.EqualTo(6));
                Assert.That(carousel.CreatedCardCount, Is.EqualTo(3));

                SerializedObject state = new SerializedObject(carousel);
                SerializedProperty containers = state.FindProperty("_SlideContainers");
                RectTransform first = containers.GetArrayElementAtIndex(0).objectReferenceValue as RectTransform;
                RectTransform second = containers.GetArrayElementAtIndex(1).objectReferenceValue as RectTransform;
                Invoke(carousel, "PopulateContainer", second, 1);
                Assert.That(carousel.CreatedCardCount, Is.EqualTo(6));
                Assert.That(carousel.VisibleCardCount, Is.EqualTo(6));

                Invoke(carousel, "ReleaseContainerCards", first);
                Invoke(carousel, "PopulateContainer", first, 2);
                Assert.That(carousel.CreatedCardCount, Is.EqualTo(6),
                    "Additional pages must reuse released views instead of instantiating more cards.");

                int visibleDots = screen.transform.Find("PageDots") == null
                    ? screen.GetComponentsInChildren<Image>(true).Count(image => image.name.StartsWith("PageDot_"))
                    : 0;
                Assert.That(visibleDots, Is.LessThanOrEqualTo(5));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(screen);
            }
        }

        [Test]
        public void StoreScene_PreservesApprovedBlankSurfacesAndUnityText()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                Transform safe = Find(scene, "Canvas/SafeAreaRoot");
                Assert.That(AssetPathAt(safe, "StoreTitlePlaque"),
                    Is.EqualTo(ApprovedRoot + "StoreTitlePlaque.png"));
                Assert.That(AssetPathAt(safe, "BottomActions/RemoveAdsAction"),
                    Is.EqualTo(ApprovedRoot + "StoreRemoveAdsButton.png"));
                Assert.That(AssetPathAt(safe, "KnifeSectionLabel"), Is.EqualTo(ExistingSecondarySurface));
                Assert.That(AssetPathAt(safe, "PowerUpSectionLabel"), Is.EqualTo(ExistingSecondarySurface));
                Assert.That(AssetPathAt(safe, "BottomActions/WatchAdAction"), Is.EqualTo(ExistingSecondarySurface));

                Assert.That(safe.Find("StoreTitlePlaque/TitleText").GetComponent<TMP_Text>(), Is.Not.Null);
                Assert.That(safe.Find("KnifeSectionLabel/Label").GetComponent<TMP_Text>(), Is.Not.Null);
                Assert.That(safe.Find("PowerUpSectionLabel/Label").GetComponent<TMP_Text>(), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void AssertNestedCardPrefab(string cardPath, string childPath, string expectedSourcePath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(cardPath);
            try
            {
                Transform child = root.transform.Find(childPath);
                Assert.That(child, Is.Not.Null);
                Assert.That(PrefabUtility.IsPartOfPrefabInstance(child.gameObject), Is.True,
                    $"{cardPath}/{childPath} must remain a nested prefab instance.");
                UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(child.gameObject);
                Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(expectedSourcePath));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AssertCardUsesIntegratedPricePlaque(
            string cardPath,
            string expectedFrameAsset,
            float expectedPriceY)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(cardPath);
            try
            {
                Image frame = root.transform.Find("Frame").GetComponent<Image>();
                Assert.That(AssetDatabase.GetAssetPath(frame.sprite),
                    Is.EqualTo(ApprovedRoot + expectedFrameAsset));

                RectTransform price = root.transform.Find("PricePill") as RectTransform;
                Assert.That(price, Is.Not.Null);
                Assert.That(price.anchoredPosition.x, Is.EqualTo(0f).Within(0.01f));
                Assert.That(price.anchoredPosition.y, Is.EqualTo(expectedPriceY).Within(0.01f));
                Assert.That(price.sizeDelta, Is.EqualTo(new Vector2(156f, 52f)));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AssertPowerUpContentStackHasNoOverlaps()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PowerPrefabPath);
            try
            {
                RectTransform owned = root.transform.Find("OwnedPill") as RectTransform;
                RectTransform grant = root.transform.Find("GrantPill") as RectTransform;
                RectTransform price = root.transform.Find("PricePill") as RectTransform;

                Assert.That(owned.anchoredPosition.y, Is.EqualTo(-28f).Within(0.01f));
                Assert.That(grant.anchoredPosition.y, Is.EqualTo(-94f).Within(0.01f));
                Assert.That(price.anchoredPosition.y, Is.EqualTo(-171f).Within(0.01f));
                Assert.That(owned.anchoredPosition.y - owned.rect.height * 0.5f,
                    Is.GreaterThan(grant.anchoredPosition.y + grant.rect.height * 0.5f));
                Assert.That(grant.anchoredPosition.y - grant.rect.height * 0.5f,
                    Is.GreaterThan(price.anchoredPosition.y + price.rect.height * 0.5f));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AssertNestedScenePrefab(Transform instance, string expectedPath)
        {
            Assert.That(instance, Is.Not.Null);
            UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance.gameObject);
            Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(expectedPath));
        }

        private static void Invoke(StoreKnifeCarouselView carousel, string methodName, params object[] arguments)
        {
            MethodInfo method = typeof(StoreKnifeCarouselView).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(carousel, arguments);
        }

        private static string TextAt(Transform parent, string path)
        {
            return parent.Find(path).GetComponent<TMP_Text>().text;
        }

        private static string AssetPathAt(Transform parent, string path)
        {
            Image image = parent.Find(path).GetComponent<Image>();
            return AssetDatabase.GetAssetPath(image.sprite);
        }

        private static string VisualSignature(Transform balance)
        {
            Image background = balance.GetComponent<Image>();
            Image coin = balance.Find("CoinIcon").GetComponent<Image>();
            TMP_Text text = balance.Find("CountText").GetComponent<TMP_Text>();
            return string.Join("|",
                AssetDatabase.GetAssetPath(background.sprite),
                AssetDatabase.GetAssetPath(coin.sprite),
                AssetDatabase.GetAssetPath(text.font),
                text.color.ToString(),
                text.alignment.ToString());
        }

        private static Transform Find(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            Transform current = scene.GetRootGameObjects()
                .FirstOrDefault(root => root.name == parts[0])?.transform;
            for (int i = 1; i < parts.Length && current != null; i++)
            {
                current = current.Find(parts[i]);
            }

            return current;
        }
    }
}
