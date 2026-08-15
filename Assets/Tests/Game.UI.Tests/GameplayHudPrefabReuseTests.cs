using System.Linq;
using Game.UI.Common;
using Game.UI.HUD;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.Tests
{
    public sealed class GameplayHudPrefabReuseTests
    {
        private const string ScenePath = "Assets/Scenes/StagePlay.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string SharedCurrencyPrefabPath =
            "Assets/Prefabs/UI/Coin/CoinUiDisaply.prefab";

        [Test]
        public void StagePlay_AndMainMenuUseTheSameCurrencyPrefabSource()
        {
            Scene stageScene = EditorSceneManager.OpenPreviewScene(ScenePath);
            Scene mainMenuScene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                GameObject stageCurrency = Find(stageScene, "CoinUiDisaply").gameObject;
                GameObject mainMenuCurrency = Find(mainMenuScene, "CoinUiDisaply").gameObject;
                Assert.That(
                    AssetDatabase.GetAssetPath(
                        PrefabUtility.GetCorrespondingObjectFromOriginalSource(stageCurrency)),
                    Is.EqualTo(SharedCurrencyPrefabPath));
                Assert.That(
                    AssetDatabase.GetAssetPath(
                        PrefabUtility.GetCorrespondingObjectFromOriginalSource(mainMenuCurrency)),
                    Is.EqualTo(SharedCurrencyPrefabPath));

                Assert.That(stageCurrency.transform.Find("CoinIcon"), Is.Not.Null);
                Assert.That(stageCurrency.transform.Find("CountText"), Is.Not.Null);
                Assert.That(stageCurrency.GetComponent<CurrencyBalanceDisplay>(), Is.Not.Null);
                Assert.That(stageCurrency.GetComponent<CoinsUiDisplay>(), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(mainMenuScene);
                EditorSceneManager.ClosePreviewScene(stageScene);
            }
        }

        private static Transform Find(Scene scene, string name)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Single(transform => transform.name == name);
        }
    }
}
