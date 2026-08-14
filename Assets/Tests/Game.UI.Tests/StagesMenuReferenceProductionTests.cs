using Game.UI.Stages;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI.Tests
{
    public sealed class StagesMenuReferenceProductionTests
    {
        private const string ScenePath = "Assets/Scenes/StagesMenu.unity";
        private const string ArtFolder = "Assets/Art/UI/StagesReference/";
        private const string MainMenuArtFolder = "Assets/Art/UI/MainMenuReference/";
        private const string IconFolder = "Assets/Art/UI/Icons/";

        [Test]
        public void StagesMenu_UsesApprovedTitleAndCompactScrollGeometry()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            try
            {
                Transform safe = Find(scene, "Canvas/SafeAreaRoot");
                RectTransform title = safe.Find("StagesTitle") as RectTransform;
                Assert.That(title.sizeDelta, Is.EqualTo(new Vector2(620f, 112f)));
                Assert.That(AssetDatabase.GetAssetPath(title.GetComponent<Image>().sprite),
                    Is.EqualTo(ArtFolder + "StagesTitlePlaque.png"));
                Assert.That(title.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("STAGES"));

                Transform backButton = safe.Find("ReturnButton");
                Assert.That(AssetDatabase.GetAssetPath(backButton.GetComponent<Image>().sprite),
                    Is.EqualTo(MainMenuArtFolder + "MainMenuWoodIconSocket.png"));
                Assert.That(AssetDatabase.GetAssetPath(backButton.Find("Glyph").GetComponent<Image>().sprite),
                    Is.EqualTo(IconFolder + "IconPrevious.png"));

                RectTransform scroll = safe.Find("StagesScrollView") as RectTransform;
                Assert.That(scroll.sizeDelta.x, Is.EqualTo(1020f).Within(0.01f));
                Assert.That(scroll.sizeDelta.y, Is.EqualTo(-324f).Within(0.01f));

                RectTransform footer = safe.Find("WorldUnlockFooter") as RectTransform;
                Assert.That(footer, Is.Not.Null);
                Assert.That(footer.sizeDelta, Is.EqualTo(new Vector2(720f, 126f)));
                Assert.That(AssetDatabase.GetAssetPath(footer.GetComponent<Image>().sprite),
                    Is.EqualTo(ArtFolder + "StagesUnlockPlaque.png"));
                Assert.That(footer.GetComponent<Image>().raycastTarget, Is.False);
                Assert.That(footer.Find("Label").GetComponent<TMP_Text>().text,
                    Is.EqualTo("COMPLETE STAGE 4\nTO UNLOCK EMBER PEAKS"));

                SerializedObject stageList = new SerializedObject(scroll.GetComponent<StageListController>());
                Assert.That(stageList.FindProperty("_WorldUnlockFooter").objectReferenceValue,
                    Is.EqualTo(footer.gameObject));
                Assert.That(stageList.FindProperty("_WorldUnlockFooterLabel").objectReferenceValue,
                    Is.EqualTo(footer.Find("Label").GetComponent<TMP_Text>()));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void StagePrefabs_KeepBehaviorOwnersAndUseReferenceTokens()
        {
            GameObject row = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Stage/UiWorldStageRow.prefab");
            GameObject item = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Stage/UiStageItem.prefab");
            Assert.That(row.GetComponent<UiWorldStageRow>(), Is.Not.Null);
            Assert.That(item.GetComponent<UiStageItem>(), Is.Not.Null);

            Image frame = row.transform.Find("ReferenceWorldFrame").GetComponent<Image>();
            Assert.That(AssetDatabase.GetAssetPath(frame.sprite), Is.EqualTo(ArtFolder + "StagesWorldFrame.png"));

            SerializedObject serialized = new SerializedObject(item.GetComponent<UiStageItem>());
            AssertToken(serialized, "_CompleteTokenSprite", "StagesTokenComplete.png");
            AssertToken(serialized, "_CurrentTokenSprite", "StagesTokenCurrent.png");
            AssertToken(serialized, "_LockedTokenSprite", "StagesTokenLocked.png");
        }

        private static void AssertToken(SerializedObject serialized, string property, string filename)
        {
            Object value = serialized.FindProperty(property).objectReferenceValue;
            Assert.That(value, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(value), Is.EqualTo(ArtFolder + filename));
        }

        private static Transform Find(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            Transform current = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == parts[0])
                {
                    current = root.transform;
                    break;
                }
            }

            for (int i = 1; i < parts.Length && current != null; i++)
            {
                current = current.Find(parts[i]);
            }

            return current;
        }
    }
}
