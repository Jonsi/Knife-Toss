using UnityEngine;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// Settings for the remote configuration service.
    /// </summary>
    [CreateAssetMenu(fileName = "RemoteConfigSettings", menuName = "Knife Toss/Remote Config/Settings")]
    public class RemoteConfigSettings : ScriptableObject
    {
        /// <summary>
        /// Timeout in seconds for fetching remote configuration.
        /// </summary>
        [field: SerializeField, Range(1, 30)]
        [field: Tooltip("Timeout in seconds for fetching remote configuration.")]
        public float FetchTimeoutSeconds { get; private set; } = 5f;
        
        /// <summary>
        /// Minimum interval between fetches in seconds (to avoid throttling).
        /// </summary>
        [field: SerializeField, Range(0, 3600)]
        [field: Tooltip("Minimum interval between fetches in seconds.")]
        public float MinimumFetchIntervalSeconds { get; private set; } = 0f;
    }
}
