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
            var scope = Object.FindAnyObjectByType<LifetimeScope>();
            if (scope == null) return;

            var resolver = scope.Container;
            if (resolver == null) return;

            SRDebug.Init();
            
            DebugTabRegistry.Register(new PlayerDataTab(resolver));
            DebugTabRegistry.Register(new CheatsTab(resolver));
            DebugTabLoader.Initialize();
            
            CreateDebugButton();
#endif
        }

        private static void CreateDebugButton()
        {
            var prefab = Resources.Load<GameObject>("Debug/SRDebuggerToggleButton");
            if (prefab == null) return;

            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                Object.Instantiate(prefab, canvas.transform);
            }
        }
    }
}
#endif
