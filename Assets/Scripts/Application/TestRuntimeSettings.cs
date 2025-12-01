#if TEST_ENABLED
using UnityEngine;

namespace KnifeToss.Application
{
    /// <summary>
    /// Applies runtime-only tweaks when the TEST build flag is present.
    /// Ensures consistent frame pacing on devices that otherwise clamp to 30 FPS.
    /// </summary>
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
