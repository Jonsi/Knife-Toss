using UnityEngine;

namespace Common.Ads.Data
{
    /// <summary>
    /// Global ad system configuration ScriptableObject.
    /// Acts as the master control surface: kill switch, timeout budget, and default cooldown.
    /// </summary>
    [CreateAssetMenu(fileName = "AdSettings", menuName = "Knife Toss/Ads/Ad Settings")]
    public class AdSettings : ScriptableObject
    {
        /// <summary>Master kill switch. When false, all ad show requests return AdResult.Disabled.</summary>
        [field: SerializeField] public bool AdsEnabled { get; private set; }

        /// <summary>When true, AdService uses MockAdProvider instead of a real SDK provider.</summary>
        [field: SerializeField] public bool UseMockProvider { get; private set; }

        /// <summary>
        /// Maximum seconds AdService waits for a provider show call before returning AdResult.Timeout.
        /// </summary>
        [field: SerializeField] public float GlobalTimeoutSeconds { get; private set; } = 30f;

        /// <summary>
        /// Fallback cooldown in seconds applied when a placement's CooldownSeconds is zero.
        /// </summary>
        [field: SerializeField] public float DefaultCooldownSeconds { get; private set; }
    }
}
