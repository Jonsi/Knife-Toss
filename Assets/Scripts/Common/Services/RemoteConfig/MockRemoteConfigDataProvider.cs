using Cysharp.Threading.Tasks;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// Provides remote config JSON from a ScriptableObject for development/testing.
    /// </summary>
    public class MockRemoteConfigDataProvider : IRemoteConfigDataProvider
    {
        private readonly RemoteConfigDataAsset _dataAsset;
        
        public MockRemoteConfigDataProvider(RemoteConfigDataAsset dataAsset)
        {
            _dataAsset = dataAsset;
        }
        
        public async UniTask<string> FetchJsonAsync()
        {
            if (_dataAsset == null)
                return null;
            
            await SimulateNetworkDelay();
            return _dataAsset.JsonData;
        }
        
        private async UniTask SimulateNetworkDelay()
        {
            if (_dataAsset.SimulatedDelayMs > 0)
                await UniTask.Delay(_dataAsset.SimulatedDelayMs);
        }
    }
}
