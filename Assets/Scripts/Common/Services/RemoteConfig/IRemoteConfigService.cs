using Cysharp.Threading.Tasks;

namespace Common.Services.RemoteConfig
{
    public interface IRemoteConfigService
    {
        bool IsInitialized { get; }
        
        UniTask<bool> FetchAndActivateAsync();
        
        /// <summary>
        /// Returns the cached JSON parsed into the type T provided by the caller.
        /// </summary>
        T GetConfig<T>();

        /// <summary>
        /// Returns the raw JSON string if you want to parse it yourself.
        /// </summary>
        string GetRawJson();
    }
}