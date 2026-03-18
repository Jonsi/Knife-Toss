namespace Game.Settings
{
    /// <summary>
    /// Runtime access point for game settings values that need static consumers.
    /// </summary>
    public static class GameSettingsAccess
    {
#if TEST_ENABLED
        private static bool _isTestLoggingEnabled;
#endif

        public static bool IsTestLoggingEnabled
        {
            get
            {
#if TEST_ENABLED
                return _isTestLoggingEnabled;
#else
                return false;
#endif
            }
        }

        public static void Initialize(GameSettings settings)
        {
#if TEST_ENABLED
            _isTestLoggingEnabled = settings != null && settings.EnableTestLogging;
#endif
        }

        public static void SetTestLoggingEnabled(bool enabled)
        {
#if TEST_ENABLED
            _isTestLoggingEnabled = enabled;
#endif
        }
    }
}
