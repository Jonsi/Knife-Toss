using Cysharp.Threading.Tasks;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// Abstraction for remote configuration services.
    /// Provides typed access to configuration values.
    /// </summary>
    public interface IRemoteConfigService
    {
        /// <summary>
        /// Whether the service has successfully fetched and activated configuration.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Fetches and activates the latest configuration from the remote source.
        /// </summary>
        UniTask<bool> FetchAndActivateAsync();
        
        /// <summary>
        /// Gets an integer value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        int GetValue(string key, int defaultValue);
        
        /// <summary>
        /// Gets a long value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        long GetValue(string key, long defaultValue);
        
        /// <summary>
        /// Gets a string value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        string GetValue(string key, string defaultValue);
        
        /// <summary>
        /// Gets a boolean value from remote config.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">Fallback if key not found.</param>
        bool GetValue(string key, bool defaultValue);
    }
}
