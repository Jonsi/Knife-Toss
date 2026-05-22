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
        private readonly RemoteConfigSettings _settings;

        public RemoteConfigInstaller(RemoteConfigSettings settings)
        {
            _settings = settings;
        }

        public void Install(IContainerBuilder builder)
        {
            RegisterSettings(builder);
            RegisterService(builder);
        }
        
        private void RegisterSettings(IContainerBuilder builder)
        {
            if (_settings != null)
            {
                builder.RegisterInstance(_settings);
            }
            else
            {
                builder.RegisterInstance(ScriptableObject.CreateInstance<RemoteConfigSettings>());
            }
        }
        
        private void RegisterService(IContainerBuilder builder)
        {
#if DISABLE_FIREBASE
            builder.Register<StubRemoteConfigService>(Lifetime.Singleton)
#else
            builder.Register<FirebaseRemoteConfigService>(Lifetime.Singleton)
#endif
                .AsImplementedInterfaces();
        }
    }
}
