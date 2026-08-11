using System;
using System.Collections.Generic;
using System.Linq;
using Game.Flow.Game;
using Game.Persistence.Data;
using Game.UI.HUD;
using Game.UI.MainMenu;
using Game.UI.Common;
using Game.UI.Popups;
using Game.UI.Services;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Feedback.Tests
{
    public class P1FoundationAssetIntegrityTests
    {
        private const string WorldStageRowPrefabPath = "Assets/Prefabs/UI/Stage/UiWorldStageRow.prefab";
        private const string StageItemPrefabPath = "Assets/Prefabs/UI/Stage/UiStageItem.prefab";
        private const string ClaimButtonPrefabPath = "Assets/Prefabs/UI/ClaimButton.prefab";
        private const string MenuButtonPrefabPath = "Assets/Prefabs/UI/Buttons/MenuButton.prefab";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string StagePlayScenePath = "Assets/Scenes/StagePlay.unity";
        private const string BuildProfilesFolder = "Assets/Settings/Build Profiles";

        private static readonly string[] ReachableScenePaths =
        {
            MainMenuScenePath,
            StagePlayScenePath,
            "Assets/Scenes/StagesMenu.unity",
            "Assets/Scenes/StoreScene.unity"
        };

        private static readonly HashSet<string> DeferredScenePaths = new HashSet<string>(StringComparer.Ordinal)
        {
            "Assets/Scenes/PVPPlay.unity",
            "Assets/Scenes/PowerUpsMenu.unity",
            "Assets/Scenes/Loading.unity"
        };

        private static readonly string[] RemovedTutorialAssetPaths =
        {
            "Assets/Prefabs/UI/GesturePopup.prefab",
            "Assets/Configurations/UI/GestureHintConfiguration.asset",
            "Assets/Scripts/Game/UI/Common/GesturePopupView.cs",
            "Assets/Scripts/Game/UI/Data/GestureHintConfiguration.cs",
            "Assets/Scripts/Game/UI/HUD/StageGestureHintController.cs",
            "Assets/Scripts/Game/Persistence/Data/PlayerOnboardingData.cs"
        };

        [Test]
        public void TutorialGuidance_IsAbsentFromAssetsScenesCompositionAndPersistence()
        {
            foreach (string assetPath in RemovedTutorialAssetPaths)
            {
                Assert.That(
                    AssetDatabase.LoadMainAssetAtPath(assetPath),
                    Is.Null,
                    $"Tutorial asset must remain removed: {assetPath}");
            }

            Assert.That(
                typeof(PlayerData).GetField("Onboarding"),
                Is.Null,
                "Player saves must not retain tutorial completion state.");

            foreach (string scenePath in new[] { MainMenuScenePath, StagePlayScenePath })
            {
                Scene scene = EditorSceneManager.OpenPreviewScene(scenePath);
                try
                {
                    string[] forbiddenObjects = scene
                        .GetRootGameObjects()
                        .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                        .Select(transform => transform.name)
                        .Where(name =>
                            name.IndexOf("gesture", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            name.IndexOf("tutorial", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            name.IndexOf("spotlight", StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                    Assert.That(
                        forbiddenObjects,
                        Is.Empty,
                        $"{scenePath} must not contain tutorial presentation objects.");

                    if (scenePath == MainMenuScenePath)
                    {
                        var controller = GetSceneComponents<MainMenuController>(scene).Single();
                        var serialized = new SerializedObject(controller);
                        Assert.That(serialized.FindProperty("_GesturePopupPrefab"), Is.Null);
                        Assert.That(serialized.FindProperty("_GestureHintConfiguration"), Is.Null);
                    }
                    else
                    {
                        MonoBehaviour installer = scene
                            .GetRootGameObjects()
                            .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                            .Single(component => component.GetType().Name == "StageInstaller");
                        var serialized = new SerializedObject(installer);
                        Assert.That(serialized.FindProperty("_GesturePopupPrefab"), Is.Null);
                        Assert.That(serialized.FindProperty("_GestureHintConfiguration"), Is.Null);
                    }
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(scene);
                }
            }
        }

        [Test]
        public void WorldStageRow_DeadWorldChestIsRemovedOrFullyInactive()
        {
            GameObject prefabRoot = LoadPrefabContents(WorldStageRowPrefabPath);
            try
            {
                Transform bigChest = prefabRoot
                    .GetComponentsInChildren<Transform>(true)
                    .SingleOrDefault(transform => transform.name == "BigChest");

                if (bigChest == null)
                {
                    Assert.Pass("The obsolete world-chest affordance has been removed from the row prefab.");
                }

                Assert.That(
                    bigChest.gameObject.activeSelf,
                    Is.False,
                    "BigChest has no authoritative world-reward action and must stay hidden until one exists.");

                Button[] chestButtons = bigChest.GetComponentsInChildren<Button>(true);
                Assert.That(chestButtons, Is.Not.Empty, "The guarded affordance changed; update this integrity check deliberately.");
                Assert.That(
                    chestButtons.All(button => !button.gameObject.activeInHierarchy && !button.isActiveAndEnabled),
                    Is.True,
                    "A hidden world-chest control must not retain an active input target.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        [Test]
        public void MenuButtonPrefab_HasOneSettingsOwnerWithOneValidButtonReference()
        {
            GameObject prefabRoot = LoadPrefabContents(MenuButtonPrefabPath);
            try
            {
                SettingsButtonView[] owners = prefabRoot.GetComponentsInChildren<SettingsButtonView>(true);
                Assert.That(
                    owners,
                    Has.Length.EqualTo(1),
                    "Each MenuButton instance must subscribe to its Button through exactly one SettingsButtonView.");

                var ownerState = new SerializedObject(owners[0]);
                SerializedProperty buttonProperty = ownerState.FindProperty("_Button");
                Assert.That(buttonProperty, Is.Not.Null, "SettingsButtonView._Button was renamed without updating its integrity test.");

                var button = buttonProperty.objectReferenceValue as Button;
                Assert.That(button, Is.Not.Null, "The single SettingsButtonView must reference its actionable Button.");
                Assert.That(
                    button.transform == owners[0].transform || button.transform.IsChildOf(owners[0].transform),
                    Is.True,
                    "SettingsButtonView must own a Button within the same reusable prefab instance.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        [Test]
        public void DynamicStageActions_HaveReusable48DpTouchPadding()
        {
            GameObject stageItemRoot = LoadPrefabContents(StageItemPrefabPath);
            GameObject claimButtonRoot = LoadPrefabContents(ClaimButtonPrefabPath);
            try
            {
                Button[] stageItemButtons = stageItemRoot.GetComponentsInChildren<Button>(true);
                Button stageButton = stageItemButtons
                    .Single(button => button.gameObject.name == "StageIcon");
                Button stageRewardButton = stageItemButtons
                    .Single(button => button.gameObject.name == "ClaimButton");
                Button claimButton = claimButtonRoot.GetComponentInChildren<Button>(true);

                AssertTouchPadding(stageButton, "Each dynamically spawned stage selector");
                AssertTouchPadding(stageRewardButton, "Each dynamically spawned stage reward claim");
                AssertTouchPadding(claimButton, "Each reusable claim button");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(claimButtonRoot);
                PrefabUtility.UnloadPrefabContents(stageItemRoot);
            }
        }

        [Test]
        public void MainMenu_SettingsFlowHasOneLocalOwnerAndCompleteSceneHost()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                SettingsButtonView[] owners = GetSceneComponents<SettingsButtonView>(scene);
                SettingsModalHost[] hosts = GetSceneComponents<SettingsModalHost>(scene);
                ModalInputBlockerPresenter[] blockers = GetSceneComponents<ModalInputBlockerPresenter>(scene);

                Assert.That(owners, Has.Length.EqualTo(1),
                    "MainMenu must inject exactly one local Settings button owner.");
                Assert.That(hosts, Has.Length.EqualTo(1),
                    "MainMenu must own its Settings flow instead of resolving hidden StagePlay UI.");
                Assert.That(hosts[0].IsConfigured, Is.True,
                    "Settings, Privacy, and Report presentations must all be wired on the host.");
                Assert.That(blockers, Has.Length.EqualTo(1),
                    "MainMenu needs one shared underlying-input blocker.");

                var settingsState = new SerializedObject(hosts[0].SettingsPopup);
                Assert.That(settingsState.FindProperty("_CloseButton")?.objectReferenceValue, Is.Not.Null,
                    "The scene-owned Settings popup must provide a usable close action.");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void StagePlay_AllTmpTextHasExplicitResolvableFontAndMaterialReferences()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(StagePlayScenePath);
            try
            {
                TMP_Text[] textComponents = GetSceneComponents<TMP_Text>(scene);
                Assert.That(textComponents, Is.Not.Empty, "StagePlay must contain player-facing TMP text.");

                var missingReferences = new List<string>();
                foreach (TMP_Text text in textComponents)
                {
                    var serializedText = new SerializedObject(text);
                    SerializedProperty fontProperty = serializedText.FindProperty("m_fontAsset");
                    SerializedProperty materialProperty = serializedText.FindProperty("m_sharedMaterial");

                    if (fontProperty == null || fontProperty.objectReferenceValue == null)
                    {
                        missingReferences.Add($"{GetHierarchyPath(text.transform)}: font asset");
                    }

                    if (materialProperty == null || materialProperty.objectReferenceValue == null)
                    {
                        missingReferences.Add($"{GetHierarchyPath(text.transform)}: shared material");
                    }
                }

                Assert.That(
                    missingReferences,
                    Is.Empty,
                    "StagePlay TMP references must be explicit and resolvable:\n" + string.Join("\n", missingReferences));

                TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
                Assert.That(defaultFont, Is.Not.Null, "TMP Settings must define an explicit default font asset.");

                bool hasGlobalFallback = TMP_Settings.fallbackFontAssets != null &&
                                         TMP_Settings.fallbackFontAssets.Any(font => font != null);
                bool hasDefaultFontFallback = defaultFont.fallbackFontAssetTable != null &&
                                              defaultFont.fallbackFontAssetTable.Any(font => font != null);

                Assert.That(
                    hasGlobalFallback || hasDefaultFontFallback,
                    Is.True,
                    "TMP Settings must provide an explicit, resolvable fallback chain.");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void AndroidBuildProfilesAndMainMenu_KeepDeferredScenesUnreachable()
        {
            UnityEngine.Object[] androidProfiles = AssetDatabase
                .FindAssets(string.Empty, new[] { BuildProfilesFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".asset", StringComparison.Ordinal))
                .Select(AssetDatabase.LoadMainAssetAtPath)
                .Where(asset => asset != null && asset.name.StartsWith("Android ", StringComparison.Ordinal))
                .ToArray();

            Assert.That(androidProfiles, Has.Length.EqualTo(3), "Development, QA, and Production Android profiles are required.");

            var violations = new List<string>();
            foreach (UnityEngine.Object profile in androidProfiles)
            {
                var serializedProfile = new SerializedObject(profile);
                SerializedProperty overrideScenes = serializedProfile.FindProperty("m_OverrideGlobalSceneList");
                SerializedProperty scenes = serializedProfile.FindProperty("m_Scenes");

                if (overrideScenes == null || !overrideScenes.boolValue)
                {
                    violations.Add($"{profile.name}: does not override the global scene list");
                    continue;
                }

                if (scenes == null || !scenes.isArray)
                {
                    violations.Add($"{profile.name}: has no serialized scene list");
                    continue;
                }

                for (int index = 0; index < scenes.arraySize; index++)
                {
                    SerializedProperty scene = scenes.GetArrayElementAtIndex(index);
                    SerializedProperty enabled = scene.FindPropertyRelative("m_enabled");
                    SerializedProperty path = scene.FindPropertyRelative("m_path");
                    if (enabled != null && enabled.boolValue && path != null && DeferredScenePaths.Contains(path.stringValue))
                    {
                        violations.Add($"{profile.name}: enables deferred scene {path.stringValue}");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Deferred prototypes must remain outside every Android player profile:\n" + string.Join("\n", violations));

            Scene mainMenu = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                MainMenuController[] controllers = GetSceneComponents<MainMenuController>(mainMenu);
                Assert.That(controllers, Has.Length.EqualTo(1), "MainMenu must have exactly one MainMenuController.");

                var controllerState = new SerializedObject(controllers[0]);
                Assert.That(
                    controllerState.FindProperty("_PvpButton")?.objectReferenceValue,
                    Is.Null,
                    "Deferred PVP must not have a live Main Menu navigation control.");
                Assert.That(
                    controllerState.FindProperty("_PowerUpsButton")?.objectReferenceValue,
                    Is.Null,
                    "Deferred PowerUpsMenu must not have a live Main Menu navigation control.");

                bool exposesLoadingRoute = typeof(SceneLoaderService)
                    .GetMethods()
                    .Any(method => string.Equals(method.Name, "LoadLoading", StringComparison.Ordinal));
                Assert.That(exposesLoadingRoute, Is.False, "The Loading prototype must not be exposed as a navigation route.");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(mainMenu);
            }
        }

        [Test]
        public void ReachableScenes_HaveOneSafeAreaRootAndNoUnresolvedTouchTargets()
        {
            foreach (string scenePath in ReachableScenePaths)
            {
                Scene scene = EditorSceneManager.OpenPreviewScene(scenePath);
                try
                {
                    SafeAreaRoot[] safeAreaRoots = GetSceneComponents<SafeAreaRoot>(scene);
                    TouchTargetValidator[] validators = GetSceneComponents<TouchTargetValidator>(scene);

                    Assert.That(safeAreaRoots, Has.Length.EqualTo(1),
                        $"{scenePath} must have exactly one reusable safe-area root.");
                    Assert.That(validators, Has.Length.EqualTo(1),
                        $"{scenePath} must have exactly one screen-level touch-target validator.");

                    foreach (RectTransform canvasRoot in GetSceneComponents<Canvas>(scene)
                                 .Select(canvas => canvas.transform as RectTransform)
                                 .Where(root => root != null))
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRoot);
                    }
                    Canvas.ForceUpdateCanvases();

                    TouchTargetValidationResult[] failures = validators[0]
                        .ValidateTargets()
                        .Where(result => !result.IsAccepted)
                        .ToArray();
                    Assert.That(failures, Is.Empty,
                        $"{scenePath} has unresolved sub-48 dp actions: " +
                        string.Join(", ", failures.Select(result => GetHierarchyPath(result.Target))));
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(scene);
                }
            }
        }

        private static GameObject LoadPrefabContents(string assetPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Assert.That(prefab, Is.Not.Null, $"Required prefab is missing: {assetPath}");
            return PrefabUtility.LoadPrefabContents(assetPath);
        }

        private static T[] GetSceneComponents<T>(Scene scene) where T : Component
        {
            return scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void AssertTouchPadding(Button button, string actionDescription)
        {
            Assert.That(button, Is.Not.Null, $"{actionDescription} must retain a Button action.");
            TouchTargetPadding padding = button.GetComponent<TouchTargetPadding>();
            Assert.That(padding, Is.Not.Null,
                $"{actionDescription} must expand its hit area without resizing the visible artwork.");
            Assert.That(padding.MinimumSizeDp, Is.GreaterThanOrEqualTo(48f),
                $"{actionDescription} must meet the 48 dp minimum target.");
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            while (transform != null)
            {
                names.Push(transform.name);
                transform = transform.parent;
            }

            return string.Join("/", names);
        }
    }
}
