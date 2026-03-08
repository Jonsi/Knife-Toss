using Common.Ads.Data;
using Common.Ads.Events;
using Common.Ads.Interfaces;
using Common.Ads.Providers;
using Common.Ads.Services;
using Game.Ads.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Ads.Installers
{
    /// <summary>
    /// MonoBehaviour installer that owns all ad-system configuration.
    /// Attach to a GameObject in the persistent scene alongside <see cref="Game.Installers.GameInstaller"/>.
    /// Registers <see cref="MockAdProvider"/> as <see cref="IAdProvider"/>,
    /// <see cref="AdService"/> as <see cref="IAdService"/>, and all supporting assets.
    /// </summary>
    public class AdInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private AdSettings _AdSettings;
        [SerializeField] private MockAdSettings _MockAdSettings;
        [SerializeField] private AdRequestedEventChannel _AdRequestedChannel;
        [SerializeField] private AdCompletedEventChannel _AdCompletedChannel;
        [SerializeField] private AdFailedEventChannel _AdFailedChannel;
        [SerializeField] private MockAdPopupBridge _MockAdPopupBridge;

        /// <summary>
        /// Registers ad system services, event channels, and scene components.
        /// </summary>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_AdSettings);
            builder.RegisterInstance(_MockAdSettings);
            builder.RegisterInstance(_AdRequestedChannel);
            builder.RegisterInstance(_AdCompletedChannel);
            builder.RegisterInstance(_AdFailedChannel);

            builder.Register<MockAdProvider>(Lifetime.Singleton).As<IAdProvider>().AsSelf();
            builder.Register<NullAdTelemetryListener>(Lifetime.Singleton).As<IAdTelemetryListener>();
            builder.Register<AdService>(Lifetime.Singleton).As<IAdService>();

            if (_MockAdPopupBridge != null)
                builder.RegisterComponent(_MockAdPopupBridge);
        }
    }
}
