#if TEST_ENABLED
using Common.IAP.Data;
using Common.IAP.Providers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.IAP.UI
{
    /// <summary>
    /// Wires MockIapProvider purchase requests to an interactive popup.
    /// </summary>
    public class MockIapPopupBridge : MonoBehaviour
    {
        [SerializeField] private MockIapPopup _Popup;

        private MockIapProvider _Provider;
        private MockIapSettings _Settings;

        [VContainer.Inject]
        public void Construct(MockIapProvider provider, MockIapSettings settings)
        {
            _Provider = provider;
            _Settings = settings;
        }

        private void Start()
        {
            if (_Provider != null)
                _Provider.PopupCallback = OnPurchaseRequested;
        }

        private UniTaskCompletionSource<IapPurchaseResult> OnPurchaseRequested(IapProduct product)
        {
            var tcs = new UniTaskCompletionSource<IapPurchaseResult>();

            if (_Popup != null)
                _Popup.Present(product, tcs, _Settings.SimulatedDelaySeconds, _Settings.DefaultOutcome);
            else
                tcs.TrySetResult(_Settings.DefaultOutcome.ToPurchaseResult("mock-no-popup"));

            return tcs;
        }

        private void OnDestroy()
        {
            if (_Provider != null)
                _Provider.PopupCallback = null;
        }
    }
}
#endif
