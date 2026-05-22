using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// Local fallback for remote config when Firebase native libraries are excluded (iOS Simulator builds).
    /// </summary>
    public class StubRemoteConfigService : IRemoteConfigService, IStartable
    {
        /// <summary>
        /// Whether the service is ready to serve configuration values.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Marks the service as initialized without contacting a remote backend.
        /// </summary>
        public void Start()
        {
            IsInitialized = true;
        }

        /// <summary>
        /// Completes immediately using local defaults only.
        /// </summary>
        public UniTask<bool> FetchAndActivateAsync()
        {
            IsInitialized = true;
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// Returns the provided default value.
        /// </summary>
        public int GetValue(string key, int defaultValue) => defaultValue;

        /// <summary>
        /// Returns the provided default value.
        /// </summary>
        public long GetValue(string key, long defaultValue) => defaultValue;

        /// <summary>
        /// Returns the provided default value.
        /// </summary>
        public string GetValue(string key, string defaultValue) => defaultValue;

        /// <summary>
        /// Returns the provided default value.
        /// </summary>
        public bool GetValue(string key, bool defaultValue) => defaultValue;
    }
}
