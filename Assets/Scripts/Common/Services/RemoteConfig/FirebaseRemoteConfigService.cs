using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.RemoteConfig;
using VContainer.Unity;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// Remote configuration service using Firebase Remote Config.
    /// Wraps Firebase SDK behind IRemoteConfigService interface.
    /// </summary>
    public class FirebaseRemoteConfigService : IRemoteConfigService, IStartable
    {
        private readonly RemoteConfigSettings _settings;

        /// <summary>
        /// Whether the service has successfully fetched and activated configuration.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes a new instance of the FirebaseRemoteConfigService.
        /// </summary>
        /// <param name="settings">The configuration settings.</param>
        public FirebaseRemoteConfigService(RemoteConfigSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Starts the service initialization process.
        /// </summary>
        public void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                await FetchAndActivateAsync();
            }
            else
            {
                LogError("Init", $"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }

        /// <summary>
        /// Fetches and activates the latest configuration from the remote source.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public async UniTask<bool> FetchAndActivateAsync()
        {
            IsInitialized = false;

            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            if (remoteConfig == null)
            {
                LogError("Fetch", "FirebaseRemoteConfig.DefaultInstance is null");
                return false;
            }

            SetFetchSettings(remoteConfig);

            if (!await TryFetchConfig(remoteConfig)) return false;
            if (!await TryActivateConfig(remoteConfig)) return false;

            IsInitialized = true;

#if TEST_ENABLED
            var allValues = string.Join(", ", remoteConfig.AllValues.Select(kv => $"{kv.Key}: {kv.Value.StringValue}"));
            UnityEngine.Debug.Log($"[RemoteConfig] Fetch and Activate success. Values: {{ {allValues} }}");
#endif
            return true;
        }

        private void SetFetchSettings(FirebaseRemoteConfig remoteConfig)
        {
            var configSettings = new ConfigSettings
            {
                FetchTimeoutInMilliseconds = (ulong)(_settings.FetchTimeoutSeconds * 1000),
                MinimumFetchIntervalInMilliseconds = (ulong)(_settings.MinimumFetchIntervalSeconds * 1000)
            };
            remoteConfig.SetConfigSettingsAsync(configSettings);
        }

        private async UniTask<bool> TryFetchConfig(FirebaseRemoteConfig remoteConfig)
        {
            try
            {
                await remoteConfig.FetchAsync(TimeSpan.FromSeconds(_settings.FetchTimeoutSeconds));
                return true;
            }
            catch (Exception ex)
            {
                LogError("Fetch", "Failed to fetch configuration", ex);
                return false;
            }
        }

        private async UniTask<bool> TryActivateConfig(FirebaseRemoteConfig remoteConfig)
        {
            try
            {
                await remoteConfig.ActivateAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogError("Activate", "Failed to activate configuration", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets an integer value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        public int GetValue(string key, int defaultValue)
        {
            return GetConfigValue(key)?.LongValue is long value ? (int)value : defaultValue;
        }

        /// <summary>
        /// Gets a long value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        public long GetValue(string key, long defaultValue)
        {
            return GetConfigValue(key)?.LongValue ?? defaultValue;
        }

        /// <summary>
        /// Gets a string value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        public string GetValue(string key, string defaultValue)
        {
            return GetConfigValue(key)?.StringValue ?? defaultValue;
        }

        /// <summary>
        /// Gets a boolean value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        public bool GetValue(string key, bool defaultValue)
        {
            return GetConfigValue(key)?.BooleanValue ?? defaultValue;
        }

        private ConfigValue? GetConfigValue(string key)
        {
            var instance = FirebaseRemoteConfig.DefaultInstance;
            if (instance == null)
            {
                LogError("GetValue", $"DefaultInstance is null when retrieving key: {key}");
                return null;
            }
            return instance.GetValue(key);
        }

        private void LogError(string context, string message, Exception ex = null)
        {
#if UNITY_EDITOR || TEST_ENABLED
            if (ex != null)
                UnityEngine.Debug.LogError($"[RemoteConfig] [{context}] {message}. Exception: {ex}");
            else
                UnityEngine.Debug.LogError($"[RemoteConfig] [{context}] {message}");
#endif
        }
    }
}