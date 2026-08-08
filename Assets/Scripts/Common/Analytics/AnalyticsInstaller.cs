using Common.Analytics.Interfaces;
using Common.Analytics.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.Analytics
{
    /// <summary>
    /// Registers analytics services and settings.
    /// </summary>
    public class AnalyticsInstaller : IInstaller
    {
        private readonly AnalyticsSettings _settings;

        public AnalyticsInstaller(AnalyticsSettings settings)
        {
            _settings = settings;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings != null
                ? _settings
                : ScriptableObject.CreateInstance<AnalyticsSettings>());

#if TEST_ENABLED
            builder.Register<MockAnalyticsService>(Lifetime.Singleton)
                .As<IAnalyticsService>()
                .AsSelf();
#else
            builder.Register<NullAnalyticsService>(Lifetime.Singleton)
                .As<IAnalyticsService>();
#endif
        }
    }
}
