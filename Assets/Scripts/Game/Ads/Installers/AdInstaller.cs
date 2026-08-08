using Common.Ads.Data;
using Common.Ads.Events;
using Common.Ads.Interfaces;
using Common.Ads.Providers;
using Common.Ads.Services;
#if TEST_ENABLED
using Game.Ads.UI;
#endif
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Ads.Installers
{
    /// <summary>
    /// MonoBehaviour installer that owns all ad-system configuration.
    /// Attach to a GameObject in the persistent scene alongside <see cref="Game.Installers.GameInstaller"/>.
    /// Registers the interactive mock provider in TEST_ENABLED builds and a safe unavailable
    /// provider otherwise. A production SDK adapter can replace the unavailable provider
    /// without allowing the mock popup into a production runtime.
    /// </summary>
    public class AdInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private AdSettings _AdSettings;
#if TEST_ENABLED
        [SerializeField] private MockAdSettings _MockAdSettings;
#endif
        [SerializeField] private AdRequestedEventChannel _AdRequestedChannel;
        [SerializeField] private AdCompletedEventChannel _AdCompletedChannel;
        [SerializeField] private AdFailedEventChannel _AdFailedChannel;
#if TEST_ENABLED
        [SerializeField] private MockAdPopupBridge _MockAdPopupBridge;
#endif

        /// <summary>
        /// Registers ad system services, event channels, and scene components.
        /// </summary>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_AdSettings);
            builder.RegisterInstance(_AdRequestedChannel);
            builder.RegisterInstance(_AdCompletedChannel);
            builder.RegisterInstance(_AdFailedChannel);

#if TEST_ENABLED
            MockAdSettings mockSettings = _MockAdSettings != null
                ? _MockAdSettings
                : ScriptableObject.CreateInstance<MockAdSettings>();
            builder.RegisterInstance(mockSettings);
            builder.Register<MockAdProvider>(Lifetime.Singleton).As<IAdProvider>().AsSelf();
#else
            builder.Register<UnavailableAdProvider>(Lifetime.Singleton).As<IAdProvider>();
#endif
            builder.Register<NullAdTelemetryListener>(Lifetime.Singleton).As<IAdTelemetryListener>();
            builder.Register<AdService>(Lifetime.Singleton).As<IAdService>();

#if TEST_ENABLED
            if (_MockAdPopupBridge != null)
                builder.RegisterComponent(_MockAdPopupBridge);
#endif
        }
    }
}
