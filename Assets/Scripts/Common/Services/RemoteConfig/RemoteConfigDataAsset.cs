using UnityEngine;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// ScriptableObject for storing mock remote configuration JSON.
    /// Edit the JSON directly in the Unity Inspector for testing.
    /// </summary>
    [CreateAssetMenu(fileName = "RemoteConfigData", menuName = "Knife Toss/Remote Config/Data Asset")]
    public class RemoteConfigDataAsset : ScriptableObject
    {
        /// <summary>
        /// The raw JSON configuration string.
        /// </summary>
        [field: SerializeField, TextArea(10, 30)]
        [field: Tooltip("JSON configuration data. Edit directly to test remote config behavior.")]
        public string JsonData { get; private set; } = "{}";
        
        /// <summary>
        /// Simulated network delay for testing async behavior.
        /// </summary>
        [field: SerializeField, Range(0, 2000)]
        [field: Tooltip("Simulated network delay in milliseconds for testing.")]
        public int SimulatedDelayMs { get; private set; } = 100;
    }
}
