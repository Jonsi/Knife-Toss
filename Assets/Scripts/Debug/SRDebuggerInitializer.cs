#if TEST_ENABLED
using Game.Debugging.Tabs;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Debugging
{
    /// <summary>
    /// Initializes SRDebugger and registers debug tabs.
    /// </summary>
    public static class SRDebuggerInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if !DISABLE_SRDEBUGGER
            SRDebug.Init();
            DebugTabLoader.Initialize();
            CreateDebugButton();
#endif
        }

        private static void CreateDebugButton()
        {
            if (Object.FindAnyObjectByType<SRDebuggerToggleButton>(FindObjectsInactive.Include) != null)
                return;

            var prefab = Resources.Load<GameObject>("Debug/SRDebuggerToggleButton");
            if (prefab == null) return;

            var debugCanvasGO = new GameObject("SRDebuggerToggleButtonCanvas");
            Object.DontDestroyOnLoad(debugCanvasGO);

            var canvas = debugCanvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000; // Ensure it's above most UI

            var scaler = debugCanvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            debugCanvasGO.AddComponent<GraphicRaycaster>();

            var instance = Object.Instantiate(prefab, debugCanvasGO.transform);
            instance.name = prefab.name;

            // Reset transform to ensure it uses prefab's intended values
            var rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
            }
        }
    }
}
#endif
