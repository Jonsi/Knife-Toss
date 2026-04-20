using System;
using System.Collections.Generic;
using System.Threading;
using Common.Ads.Data;
using Common.Ads.Events;
using Common.Ads.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using StackTrace = System.Diagnostics.StackTrace;

namespace Common.Ads.Services
{
    /// <summary>
    /// Core ad system service. Owns all policy decisions: master kill switch, cooldown gating,
    /// frequency gating, once-per-run enforcement, timeout, event dispatch, and telemetry.
    /// Delegates actual ad loading and display to the injected <see cref="IAdProvider"/>.
    /// </summary>
    public class AdService : IAdService
    {
        private readonly IAdProvider _Provider;
        private readonly AdSettings _Settings;
        private readonly AdRequestedEventChannel _RequestedChannel;
        private readonly AdCompletedEventChannel _CompletedChannel;
        private readonly AdFailedEventChannel _FailedChannel;
        private readonly IAdTelemetryListener _Telemetry;
        private readonly IAdEntitlementGate _EntitlementGate;

        private readonly Dictionary<AdPlacementKey, float> _LastShownTimestamps = new();
        private readonly Dictionary<AdPlacementKey, int> _AttemptCounters = new();
        private readonly HashSet<AdPlacementKey> _UsedOncePerRunPlacements = new();

        /// <summary>
        /// Primary constructor. Pass null for <paramref name="telemetry"/> when no analytics
        /// listener is registered (unit tests, pre-Phase-3 configurations).
        /// </summary>
        public AdService(
            IAdProvider provider,
            AdSettings settings,
            AdRequestedEventChannel requestedChannel,
            AdCompletedEventChannel completedChannel,
            AdFailedEventChannel failedChannel,
            IAdTelemetryListener telemetry,
            IAdEntitlementGate entitlementGate)
        {
            _Provider = provider;
            _Settings = settings;
            _RequestedChannel = requestedChannel;
            _CompletedChannel = completedChannel;
            _FailedChannel = failedChannel;
            _Telemetry = telemetry;
            _EntitlementGate = entitlementGate;
        }

        public AdService(
            IAdProvider provider,
            AdSettings settings,
            AdRequestedEventChannel requestedChannel,
            AdCompletedEventChannel completedChannel,
            AdFailedEventChannel failedChannel,
            IAdTelemetryListener telemetry)
            : this(provider, settings, requestedChannel, completedChannel, failedChannel, telemetry, null) { }

        /// <summary>
        /// Constructor overload for contexts where no telemetry listener is available.
        /// VContainer installers that register an <see cref="IAdTelemetryListener"/> should use
        /// the six-parameter overload so VContainer resolves it automatically.
        /// </summary>
        public AdService(
            IAdProvider provider,
            AdSettings settings,
            AdRequestedEventChannel requestedChannel,
            AdCompletedEventChannel completedChannel,
            AdFailedEventChannel failedChannel)
            : this(provider, settings, requestedChannel, completedChannel, failedChannel, null, null) { }

        /// <inheritdoc/>
        public bool IsAdAvailable(AdPlacement placement) => _Provider.IsLoaded(placement);

        /// <inheritdoc/>
        public bool CanShowAd(AdPlacement placement)
        {
            if (!_Settings.AdsEnabled) return false;
            if (IsInterstitialBlockedByEntitlement(placement)) return false;
            if (_UsedOncePerRunPlacements.Contains(placement.PlacementKey)) return false;
            if (IsCooldownActive(placement)) return false;
            if (IsFrequencyGateBlocking(placement)) return false;
            return true;
        }

        /// <inheritdoc/>
        public UniTask<AdResult> ShowInterstitialAsync(AdPlacement placement, CancellationToken ct = default) =>
            ShowAdAsync(placement, ct);

        /// <inheritdoc/>
        public UniTask<AdResult> ShowRewardedAsync(AdPlacement placement, CancellationToken ct = default) =>
            ShowAdAsync(placement, ct);

        /// <inheritdoc/>
        public void ResetRunState() => _UsedOncePerRunPlacements.Clear();

        /// <inheritdoc/>
        public void ResetCooldowns()
        {
            _LastShownTimestamps.Clear();
            _AttemptCounters.Clear();
        }

        /// <inheritdoc/>
        public void IncrementAttemptCounter(AdPlacement placement)
        {
            _AttemptCounters[placement.PlacementKey] = GetAttemptCount(placement.PlacementKey) + 1;
        }

        private async UniTask<AdResult> ShowAdAsync(AdPlacement placement, CancellationToken ct)
        {
            Debug.Log(
                $"[AdService] Show requested. Placement={placement?.PlacementKey}, Type={placement?.GetType().Name}, OncePerRun={placement?.OncePerRun}, Cooldown={placement?.CooldownSeconds}, AttemptsRequired={placement?.MinAttemptsBetweenAds}\n" +
                $"{GetCallerStackTrace()}");

            if (!_Settings.AdsEnabled) return AdResult.Disabled;
            if (IsInterstitialBlockedByEntitlement(placement)) return AdResult.Disabled;

            if (_UsedOncePerRunPlacements.Contains(placement.PlacementKey)) return AdResult.OnCooldown;
            if (IsCooldownActive(placement)) return AdResult.OnCooldown;
            if (IsFrequencyGateBlocking(placement)) return AdResult.OnCooldown;

            var loadBlock = await TryEnsureLoadedAsync(placement, ct);
            if (loadBlock.HasValue) return loadBlock.Value;

            RaiseAdRequested(placement);

            var result = await ExecuteShowAsync(placement, ct);
            DispatchResult(placement, result);
            return result;
        }

        /// <summary>
        /// Awaits the provider show call under a merged timeout+caller cancellation token.
        /// Returns a terminal <see cref="AdResult"/> on cancellation or timeout.
        /// </summary>
        private async UniTask<AdResult> ExecuteShowAsync(AdPlacement placement, CancellationToken ct)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_Settings.GlobalTimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
            try
            {
                _Telemetry?.OnAdShown(placement);
                return await _Provider.ShowAsync(placement, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                return timeoutCts.IsCancellationRequested ? AdResult.Timeout : AdResult.Skipped;
            }
        }

        private void RaiseAdRequested(AdPlacement placement)
        {
            Debug.Log($"[AdService] Raising ad requested event for placement {placement?.PlacementKey}.");
            _RequestedChannel.Raise(new AdEventData(placement, default));
            _Telemetry?.OnAdRequested(placement);
        }

        private static string GetCallerStackTrace()
        {
            var stackTrace = new StackTrace(2, true);
            return stackTrace.ToString();
        }

        private void DispatchResult(AdPlacement placement, AdResult result)
        {
            if (result.Success)
            {
                _LastShownTimestamps[placement.PlacementKey] = Time.realtimeSinceStartup;
                _AttemptCounters[placement.PlacementKey] = 0;

                if (placement.OncePerRun)
                    _UsedOncePerRunPlacements.Add(placement.PlacementKey);

                _CompletedChannel.Raise(new AdEventData(placement, result));
                _Telemetry?.OnAdCompleted(placement, result);

                _Provider.LoadAsync(placement).Forget();
            }
            else
            {
                _FailedChannel.Raise(new AdEventData(placement, result));
                _Telemetry?.OnAdFailed(placement, result.FailureReason);
            }
        }

        private bool IsCooldownActive(AdPlacement placement)
        {
            var cooldown = placement.CooldownSeconds > 0f
                ? placement.CooldownSeconds
                : _Settings.DefaultCooldownSeconds;

            if (!_LastShownTimestamps.TryGetValue(placement.PlacementKey, out var lastShown))
                return false;

            return Time.realtimeSinceStartup - lastShown < cooldown;
        }

        private bool IsFrequencyGateBlocking(AdPlacement placement)
        {
            if (placement.MinAttemptsBetweenAds <= 0) return false;
            return GetAttemptCount(placement.PlacementKey) < placement.MinAttemptsBetweenAds;
        }

        private bool IsInterstitialBlockedByEntitlement(AdPlacement placement)
        {
            return placement is InterstitialPlacement &&
                   _EntitlementGate != null &&
                   !_EntitlementGate.CanShowInterstitial();
        }

        private int GetAttemptCount(AdPlacementKey key) =>
            _AttemptCounters.TryGetValue(key, out var count) ? count : 0;

        /// <summary>
        /// Attempts to load the placement if it is not already loaded.
        /// Returns a blocking <see cref="AdResult"/> if loading failed or was cancelled;
        /// returns null if the ad is ready to show.
        /// </summary>
        private async UniTask<AdResult?> TryEnsureLoadedAsync(AdPlacement placement, CancellationToken ct)
        {
            if (_Provider.IsLoaded(placement)) return null;

            try
            {
                await _Provider.LoadAsync(placement, ct);
            }
            catch (OperationCanceledException)
            {
                return AdResult.Skipped;
            }
            catch (Exception ex)
            {
                _Telemetry?.OnAdFailed(placement, ex.Message);
                return AdResult.Unavailable;
            }

            return _Provider.IsLoaded(placement) ? (AdResult?)null : AdResult.Unavailable;
        }
    }
}
