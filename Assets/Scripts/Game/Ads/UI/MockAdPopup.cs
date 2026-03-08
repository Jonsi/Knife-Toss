using System.Threading;
using Common.Ads.Data;
using Cysharp.Threading.Tasks;
using Game.Ads.Data;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Ads.UI
{
    /// <summary>
    /// Full-screen mock ad overlay for QA testing without a real ad SDK.
    /// Presents the placement key, ad format, and manual outcome buttons so testers can
    /// control ad results interactively. An auto-dismiss timer resolves the ad automatically
    /// if no button is pressed within the configured delay window.
    ///
    /// Activated by <see cref="MockAdPopupBridge"/> when <see cref="Common.Ads.Providers.MockAdProvider"/>
    /// has its <c>PopupCallback</c> wired.
    /// </summary>
    public class MockAdPopup : BasePopup
    {
        [Header("Text")]
        [SerializeField] private TMP_Text _PlacementText;
        [SerializeField] private TMP_Text _FormatText;
        [SerializeField] private TMP_Text _TimerText;

        [Header("Buttons")]
        [SerializeField] private Button _CompleteButton;
        [SerializeField] private Button _SkipButton;
        [SerializeField] private Button _FailButton;

        private UniTaskCompletionSource<AdResult> _Tcs;
        private AdMockOutcome _AutoDismissOutcome;
        private CancellationTokenSource _TimerCts;

        private void OnEnable()
        {
            _CompleteButton.onClick.AddListener(OnCompleteClicked);
            _SkipButton.onClick.AddListener(OnSkipClicked);
            _FailButton.onClick.AddListener(OnFailClicked);
        }

        private void OnDisable()
        {
            _CompleteButton.onClick.RemoveListener(OnCompleteClicked);
            _SkipButton.onClick.RemoveListener(OnSkipClicked);
            _FailButton.onClick.RemoveListener(OnFailClicked);
        }

        /// <summary>
        /// Configures and shows the popup for a specific placement.
        /// The provided <see cref="UniTaskCompletionSource{AdResult}"/> is resolved when
        /// the player taps a button or the auto-dismiss timer expires.
        /// </summary>
        /// <param name="placement">Placement that triggered this ad show.</param>
        /// <param name="tcs">Completion source to resolve with the player's chosen outcome.</param>
        /// <param name="autoCloseSeconds">Seconds before the popup auto-resolves with the default outcome.</param>
        /// <param name="defaultOutcome">Outcome used when the timer expires without player input.</param>
        public void Present(
            AdPlacement placement,
            UniTaskCompletionSource<AdResult> tcs,
            float autoCloseSeconds,
            AdMockOutcome defaultOutcome)
        {
            _Tcs = tcs;
            _AutoDismissOutcome = defaultOutcome;

            if (_PlacementText != null)
                _PlacementText.text = placement.PlacementKey.ToString();

            if (_FormatText != null)
                _FormatText.text = GetFormatLabel(placement);

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
                if (ct.IsCancellationRequested) return;
                if (_TimerText != null)
                    _TimerText.text = $"Auto: {Mathf.CeilToInt(remaining)}s";

                await UniTask.Delay(100, cancellationToken: ct);
                remaining -= 0.1f;
            }

            Resolve(_AutoDismissOutcome.ToAdResult());
        }

        private void Resolve(AdResult result)
        {
            _TimerCts?.Cancel();
            Hide();
            _Tcs?.TrySetResult(result);
            _Tcs = null;
        }

        private void OnCompleteClicked() => Resolve(AdResult.Completed);
        private void OnSkipClicked() => Resolve(AdResult.Skipped);
        private void OnFailClicked() => Resolve(AdResult.Failed("Simulated failure."));

        private static string GetFormatLabel(AdPlacement placement)
        {
            return placement switch
            {
                RewardedPlacement => "Rewarded",
                InterstitialPlacement => "Interstitial",
                _ => "Unknown"
            };
        }

        private void OnDestroy()
        {
            _TimerCts?.Cancel();
            _Tcs?.TrySetResult(AdResult.Skipped);
        }
    }
}
