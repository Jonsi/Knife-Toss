#if TEST_ENABLED
using Game.Debugging.Tabs;
using UnityEngine;

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
#endif
        }
    }
}
#endif
