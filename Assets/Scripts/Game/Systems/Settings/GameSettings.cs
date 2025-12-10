using UnityEngine;

namespace Game.Systems.Settings
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

    /// <summary>
    /// Provides static access to game settings at runtime.
    /// </summary>
    public static class GameSettingsAccess
    {
        /// <summary>
        /// Gets the active settings instance.
        /// </summary>
        public static GameSettings Instance { get; private set; }

        /// <summary>
        /// Gets a value indicating whether test logging is enabled.
        /// </summary>
        public static bool IsTestLoggingEnabled => Instance != null && Instance.EnableTestLogging;

        /// <summary>
        /// Initializes the active settings reference.
        /// </summary>
        /// <param name="settings">The settings instance to use.</param>
        public static void Initialize(GameSettings settings)
        {
            Instance = settings;
        }
    }
}

