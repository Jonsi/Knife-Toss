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
        [field: SerializeField, Range(1, 10)]
        [field: Tooltip("Timeout in seconds for fetching remote configuration.")]
        public float FetchTimeoutSeconds { get; private set; } = 2f;
    }
}
