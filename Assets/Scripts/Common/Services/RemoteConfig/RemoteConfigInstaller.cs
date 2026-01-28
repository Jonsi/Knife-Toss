using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.Services.RemoteConfig
{
    /// <summary>
    /// Installer for remote configuration services.
    /// </summary>
    public class RemoteConfigInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            RegisterSettings(builder);
            RegisterDataProvider(builder);
            RegisterService(builder);
        }
        
        private void RegisterSettings(IContainerBuilder builder)
        {
            var settings = Resources.Load<RemoteConfigSettings>("RemoteConfig/RemoteConfigSettings");
            if (settings != null)
            {
                builder.RegisterInstance(settings);
            }
            else
            {
                // Fallback if asset is missing (though it should be created)
                Debug.LogWarning("RemoteConfigSettings asset not found in Resources/RemoteConfig/. Using default instance.");
                builder.RegisterInstance(ScriptableObject.CreateInstance<RemoteConfigSettings>());
            }
        }
        
        private void RegisterDataProvider(IContainerBuilder builder)
        {
            var dataAsset = Resources.Load<RemoteConfigDataAsset>("RemoteConfig/RemoteConfigData");
            if (dataAsset != null)
            {
                builder.Register<MockRemoteConfigDataProvider>(Lifetime.Singleton)
                    .WithParameter(dataAsset)
                    .As<IRemoteConfigDataProvider>();
            }
            else
            {
                Debug.LogWarning("RemoteConfigDataAsset not found in Resources/RemoteConfig/. Mock provider may return empty data.");
                builder.Register<MockRemoteConfigDataProvider>(Lifetime.Singleton)
                    .WithParameter<RemoteConfigDataAsset>(null)
                    .As<IRemoteConfigDataProvider>();
            }
        }
        
        private void RegisterService(IContainerBuilder builder)
        {
            builder.Register<RemoteConfigService>(Lifetime.Singleton)
                .As<IRemoteConfigService>()
                .AsImplementedInterfaces(); // For IStartable
        }
    }
}
