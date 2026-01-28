using Cysharp.Threading.Tasks;
using VContainer.Unity;
using System;
using UnityEngine;

namespace Common.Services.RemoteConfig
{
    public class RemoteConfigService : IRemoteConfigService, IStartable
    {
        private readonly IRemoteConfigDataProvider _dataProvider;
        private readonly RemoteConfigSettings _settings;
        
        private string _cachedJson;
        private bool _isInitialized;
        
        public bool IsInitialized => _isInitialized;
        
        public RemoteConfigService(
            IRemoteConfigDataProvider dataProvider,
            RemoteConfigSettings settings)
        {
            _dataProvider = dataProvider;
            _settings = settings;
        }
        
        public void Start()
        {
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            await FetchAndActivateAsync();
        }
        
        public async UniTask<bool> FetchAndActivateAsync()
        {
            string json = await FetchWithTimeout();
            if (string.IsNullOrEmpty(json))
            {
                _isInitialized = false;
                return false;
            }

            _cachedJson = json;
            _isInitialized = true;
            return true;
        }

        private async UniTask<string> FetchWithTimeout()
        {
            try
            {
                return await _dataProvider.FetchJsonAsync()
                    .Timeout(TimeSpan.FromSeconds(_settings.FetchTimeoutSeconds));
            }
            catch
            {
                return null;
            }
        }

        public T GetConfig<T>()
        {
            if (!_isInitialized || string.IsNullOrEmpty(_cachedJson))
            {
                return default;
            }

            try
            {
                return JsonUtility.FromJson<T>(_cachedJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RemoteConfigService] Failed to parse config to {typeof(T).Name}: {e.Message}");
                return default;
            }
        }

        public string GetRawJson()
        {
            return _cachedJson;
        }
    }
}