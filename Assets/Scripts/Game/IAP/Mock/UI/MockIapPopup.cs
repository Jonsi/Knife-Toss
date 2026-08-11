#if TEST_ENABLED
using System;
using System.Threading;
using Common.IAP.Data;
using Cysharp.Threading.Tasks;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.IAP.UI
{
    /// <summary>
    /// Full-screen popup for interactive mock IAP outcomes.
    /// </summary>
    public class MockIapPopup : BasePopup
    {
        [Header("Text")]
        [SerializeField] private TMP_Text _ProductText;
        [SerializeField] private TMP_Text _TimerText;

        [Header("Buttons")]
        [SerializeField] private Button _SuccessButton;
        [SerializeField] private Button _CancelButton;
        [SerializeField] private Button _DeferredButton;
        [SerializeField] private Button _FailButton;

        private UniTaskCompletionSource<IapPurchaseResult> _Tcs;
        private IapMockOutcome _AutoDismissOutcome;
        private CancellationTokenSource _TimerCts;
        private IapProduct _CurrentProduct;

        private void OnEnable()
        {
            if (_SuccessButton != null) _SuccessButton.onClick.AddListener(OnSuccessClicked);
            if (_CancelButton != null) _CancelButton.onClick.AddListener(OnCancelledClicked);
            if (_DeferredButton != null) _DeferredButton.onClick.AddListener(OnDeferredClicked);
            if (_FailButton != null) _FailButton.onClick.AddListener(OnFailedClicked);
        }

        protected override void OnDisable()
        {
            if (_SuccessButton != null) _SuccessButton.onClick.RemoveListener(OnSuccessClicked);
            if (_CancelButton != null) _CancelButton.onClick.RemoveListener(OnCancelledClicked);
            if (_DeferredButton != null) _DeferredButton.onClick.RemoveListener(OnDeferredClicked);
            if (_FailButton != null) _FailButton.onClick.RemoveListener(OnFailedClicked);
            base.OnDisable();
        }

        public void Present(
            IapProduct product,
            UniTaskCompletionSource<IapPurchaseResult> tcs,
            float autoCloseSeconds,
            IapMockOutcome defaultOutcome)
        {
            _CurrentProduct = product;
            _Tcs = tcs;
            _AutoDismissOutcome = defaultOutcome;

            if (_ProductText != null)
                _ProductText.text = product != null ? product.name : "Unknown Product";

            Show();

            _TimerCts?.Cancel();
            _TimerCts = new CancellationTokenSource();

            if (autoCloseSeconds > 0f)
                RunAutoTimerAsync(autoCloseSeconds, _TimerCts.Token).Forget();
            else if (_TimerText != null)
                _TimerText.text = string.Empty;
        }

        private async UniTaskVoid RunAutoTimerAsync(float totalSeconds, CancellationToken ct)
        {
            var remaining = totalSeconds;

            while (remaining > 0f)
            {
                if (ct.IsCancellationRequested)
                    return;

                if (_TimerText != null)
                    _TimerText.text = $"Auto: {Mathf.CeilToInt(remaining)}s";

                await UniTask.Delay(100, cancellationToken: ct);
                remaining -= 0.1f;
            }

            Resolve(_AutoDismissOutcome.ToPurchaseResult(CreateReceipt()));
        }

        private void OnSuccessClicked() => Resolve(IapPurchaseResult.Purchased(CreateReceipt()));
        private void OnCancelledClicked() => Resolve(IapPurchaseResult.CancelledByUser);
        private void OnDeferredClicked() => Resolve(IapPurchaseResult.DeferredByStore);
        private void OnFailedClicked() => Resolve(IapPurchaseResult.Failed("Simulated failure."));

        private void Resolve(IapPurchaseResult result)
        {
            _TimerCts?.Cancel();
            Hide();
            _Tcs?.TrySetResult(result);
            _Tcs = null;
        }

        private string CreateReceipt()
        {
            var key = _CurrentProduct != null && !string.IsNullOrEmpty(_CurrentProduct.ProductKey)
                ? _CurrentProduct.ProductKey
                : "iap";
            return $"mock-popup:{key}:{Guid.NewGuid():N}";
        }

        protected override void OnDestroy()
        {
            _TimerCts?.Cancel();
            _Tcs?.TrySetResult(IapPurchaseResult.CancelledByUser);
            base.OnDestroy();
        }
    }
}
#endif
