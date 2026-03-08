using UnityEngine;

namespace Common.Ads.Data
{
    /// <summary>
    /// Base ScriptableObject for all ad placement configurations.
    /// Each distinct ad surface in the game gets its own asset instance.
    /// Configuration values can be overridden by Remote Config in Phase 2.
    /// </summary>
    public abstract class AdPlacement : ScriptableObject
    {
        /// <summary>Enum identifier used for cooldown and frequency tracking.</summary>
        [field: SerializeField] public AdPlacementKey PlacementKey { get; private set; }

        /// <summary>
        /// Minimum seconds that must elapse after a successful show before this placement can show again.
        /// When zero, falls back to AdSettings.DefaultCooldownSeconds.
        /// </summary>
        [field: SerializeField] public float CooldownSeconds { get; private set; }

        /// <summary>
        /// Minimum number of game attempts (via IncrementAttemptCounter) required between ad shows.
        /// Zero disables frequency gating for this placement.
        /// </summary>
        [field: SerializeField] public int MinAttemptsBetweenAds { get; private set; }

        /// <summary>When true, this placement can only be shown once per run/session.</summary>
        [field: SerializeField] public bool OncePerRun { get; private set; }
    }
}
