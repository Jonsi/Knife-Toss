using Cysharp.Threading.Tasks;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// Abstracts the source of remote configuration data.
    /// Implement for mock (ScriptableObject) or production (Firebase).
    /// </summary>
    public interface IRemoteConfigDataProvider
    {
        /// <summary>
        /// Fetches the JSON configuration string from the source.
        /// </summary>
        UniTask<string> FetchJsonAsync();
    }
}
