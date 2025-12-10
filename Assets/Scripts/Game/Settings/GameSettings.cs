using UnityEngine;

namespace Game.Settings
{
    /// <summary>
    /// Holds global game configuration flags.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Knife Toss/Settings/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        /// <summary>
        /// Enables logging when the TEST_ENV scripting define is present.
        /// </summary>
        [field: SerializeField] public bool EnableTestLogging { get; private set; }
    }
}