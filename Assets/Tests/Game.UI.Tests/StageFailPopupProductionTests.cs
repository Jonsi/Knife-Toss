using System;
using System.Linq;
using System.Reflection;
using Common.Events;
using Game.UI.Common;
using Game.UI.Popups;
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
        private const string FailureAtlasPath = "Assets/Art/UI/Failure/FailurePopupAtlas.png";
        private const string KnifeAnimationAtlasPath = "Assets/Art/UI/Failure/FailureKnivesAnimation.png";

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

                Assert.That(
                    popup.GetComponentsInChildren<TMP_Text>(true),
                    Is.Empty,
                    "Reference-derived title/button lettering must not be replaced by runtime typography.");

                string[] actualAtlasSprites = popup.GetComponentsInChildren<Image>(true)
                    .Where(image => image.sprite != null && AssetDatabase.GetAssetPath(image.sprite) == FailureAtlasPath)
                    .Select(image => image.sprite.name)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToArray();
                CollectionAssert.AreEqual(
                    new[] { "GameOverTitle", "MenuButtonArt", "RetryButtonArt" },
                    actualAtlasSprites,
                    "The popup must use the direct-reference title and action art by responsibility.");

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
                    Has.Length.EqualTo(4),
                    "Title, impact art, Retry, and Menu must retain their stagger/settle presentation owners.");

                Button[] buttons = popup.GetComponentsInChildren<Button>(true);
                Assert.That(buttons, Has.Length.EqualTo(2));
                CollectionAssert.AreEquivalent(
                    new[] { "RetryButton", "MenuButton" },
                    buttons.Select(button => button.name));

                foreach (Button button in buttons)
                {
                    GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(button.gameObject);
                    Assert.That(source, Is.Not.Null, $"{button.name} lost its prefab link.");
                    Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(ActionButtonPrefabPath));
                }

                GameObject popupSource = PrefabUtility.GetCorrespondingObjectFromSource(popup.gameObject);
                Assert.That(popupSource, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(popupSource), Is.EqualTo(PopupTemplatePath));

                SerializedObject popupObject = new SerializedObject(popup);
                Assert.That(popupObject.FindProperty("_RetryButton").objectReferenceValue, Is.Not.Null);
                Assert.That(popupObject.FindProperty("_MenuButton").objectReferenceValue, Is.Not.Null);
                Assert.That(popupObject.FindProperty("_RestartStageButton"), Is.Null);
                Assert.That(popupObject.FindProperty("_ContinueButton"), Is.Null);
                Assert.That(popupObject.FindProperty("_ContinueAmountText"), Is.Null);

                MonoBehaviour installer = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Single(behaviour => behaviour != null && behaviour.GetType().Name == "StageInstaller");
                SerializedObject installerObject = new SerializedObject(installer);
                Assert.That(installerObject.FindProperty("_ContinueRequestedChannel"), Is.Null);
                Assert.That(installerObject.FindProperty("_ContinuePlacement"), Is.Null);
                Assert.That(installerObject.FindProperty("_InterstitialPlacement"), Is.Null);
            }
            finally
            {
                if (restorePrevious)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }
        }

        [Test]
        public void RetryCommitsOnlyOnceForDuplicateInput()
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
                popupObject.FindProperty("_RetryLevelChannel").objectReferenceValue = retryChannel;
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
    }
}
