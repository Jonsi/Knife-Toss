#if TEST_ENABLED
using UnityEngine;

namespace KnifeToss.Application
{




    internal static class TestRuntimeSettings
    {
        private const int TargetFps = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = TargetFps;
        }
    }
}
#endif
