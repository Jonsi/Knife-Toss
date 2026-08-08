#if TEST_ENABLED
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Game.Ads.UI;
using Game.Ads.Installers;

namespace Editor.Ads
{
    /// <summary>
    /// Fixes GameInstaller prefab to match AdSystemUnitySetup.md section 3.3:
    /// Creates MockAdSystem child, adds MockAd as nested instance, wires MockAdPopupBridge correctly.
    /// </summary>
    public static class FixMockAdSetup
    {
        private const string GameInstallerPath = "Assets/Prefabs/GameInstaller.prefab";
        private const string MockAdPrefabPath = "Assets/Prefabs/UI/MockAd.prefab";

        [MenuItem("Knife Toss/Ads/Fix MockAd Setup in GameInstaller")]
        public static void Execute()
        {
            var mockAdPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MockAdPrefabPath);
            if (mockAdPrefab == null)
            {
                Debug.LogError($"[FixMockAdSetup] MockAd prefab not found at {MockAdPrefabPath}");
                return;
            }

            if (mockAdPrefab.GetComponent<MockAdPopup>() == null)
            {
                Debug.LogError("[FixMockAdSetup] MockAd prefab has no MockAdPopup component on root");
                return;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(GameInstallerPath))
            {
                var root = scope.prefabContentsRoot.transform;

                var existingMockAdSystem = root.Find("MockAdSystem");
                if (existingMockAdSystem != null)
                {
                    Object.DestroyImmediate(existingMockAdSystem.gameObject);
                }

                var mockAdSystemGo = new GameObject("MockAdSystem");
                mockAdSystemGo.transform.SetParent(root, false);

                var canvas = mockAdSystemGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;
                mockAdSystemGo.AddComponent<CanvasScaler>();
                mockAdSystemGo.AddComponent<GraphicRaycaster>();

                var mockAdInstance = (GameObject)PrefabUtility.InstantiatePrefab(mockAdPrefab, mockAdSystemGo.transform);
                mockAdInstance.name = "MockAd";
                mockAdInstance.SetActive(false);

                var bridge = mockAdSystemGo.AddComponent<MockAdPopupBridge>();
                var popupField = typeof(MockAdPopupBridge).GetField("_Popup",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (popupField != null)
                {
                    var mockAdPopupInstance = mockAdInstance.GetComponent<MockAdPopup>();
                    popupField.SetValue(bridge, mockAdPopupInstance);
                }

                var rootBridge = root.GetComponent<MockAdPopupBridge>();
                if (rootBridge != null)
                {
                    Object.DestroyImmediate(rootBridge);
                }

                var adInstaller = root.GetComponent<AdInstaller>();
                if (adInstaller != null)
                {
                    var adInstallerField = typeof(AdInstaller).GetField("_MockAdPopupBridge",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (adInstallerField != null)
                        adInstallerField.SetValue(adInstaller, bridge);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[FixMockAdSetup] GameInstaller prefab updated. MockAdSystem created with nested MockAd instance.");
        }
    }
}
#endif
