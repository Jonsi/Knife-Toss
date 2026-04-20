#if TEST_ENABLED
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Common.IAP.Providers;
using Game.IAP.UI;
using VContainer;
using VContainer.Unity;

namespace Game.IAP.Mock.Installers
{
    /// <summary>
    /// Test-only installer that wires mock IAP provider/settings and optional popup bridge.
    /// </summary>
    public class IapMockInstaller : IInstaller
    {
        private readonly MockIapSettings _MockIapSettings;
        private readonly MockIapPopupBridge _MockIapPopupBridge;

        public IapMockInstaller(MockIapSettings mockIapSettings, MockIapPopupBridge mockIapPopupBridge)
        {
            _MockIapSettings = mockIapSettings;
            _MockIapPopupBridge = mockIapPopupBridge;
        }

        public void Install(IContainerBuilder builder)
        {
            if (_MockIapSettings == null)
            {
                return;
            }

            builder.RegisterInstance(_MockIapSettings);
            builder.Register<MockIapProvider>(Lifetime.Singleton).As<IIapProvider>().AsSelf();

            if (_MockIapPopupBridge != null)
            {
                builder.RegisterComponent(_MockIapPopupBridge);
            }
        }
    }
}
#endif
