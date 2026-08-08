#if TEST_ENABLED
using System;
using System.Collections.Generic;
using System.Threading;
using Common.Ads.Data;
using Common.Ads.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Ads.Providers
{
    /// <summary>
    /// Development and QA ad provider. Simulates loading and display delays using
    /// <see cref="MockAdSettings"/> without contacting any real ad SDK.
    ///
    /// Supports three operating modes:
    /// 1. Default outcome — every show returns <see cref="MockAdSettings.DefaultOutcome"/>.
    /// 2. Sequenced outcomes — cycles through <see cref="MockAdSettings.SequencedOutcomes"/> per placement key.
    /// 3. Popup callback — Phase 2 wires <see cref="PopupCallback"/> to show a UI overlay and
    ///    manually resolve the outcome, enabling interactive QA without a real SDK.
    /// </summary>
    public class MockAdProvider : IAdProvider
    {
        private readonly MockAdSettings _Settings;
        private readonly HashSet<AdPlacementKey> _LoadedPlacements = new();
        private readonly Dictionary<AdPlacementKey, int> _SequenceIndices = new();

        private AdMockOutcome? _ForcedNextOutcome;

        /// <summary>
        /// Optional callback set by Phase 2 to intercept show calls with a UI popup.
        /// When set, <see cref="ShowAsync"/> invokes this instead of auto-resolving.
        /// The returned <see cref="UniTaskCompletionSource{AdResult}"/> is awaited for the manual outcome.
        /// </summary>
        public Func<AdPlacement, UniTaskCompletionSource<AdResult>> PopupCallback { get; set; }

        /// <summary>
        /// Forces the next <see cref="ShowAsync"/> call to return the given outcome, regardless of
        /// sequenced or default outcomes. The override is consumed after one use.
        /// Used by debug tools to test specific ad result paths.
        /// </summary>
        public void ForceNextOutcome(AdMockOutcome outcome) => _ForcedNextOutcome = outcome;

        public MockAdProvider(MockAdSettings settings)
        {
            _Settings = settings;
        }

        /// <inheritdoc/>
        public UniTask InitializeAsync(CancellationToken ct = default) => UniTask.CompletedTask;

        /// <inheritdoc/>
        public async UniTask LoadAsync(AdPlacement placement, CancellationToken ct = default)
        {
            await SimulateDelayAsync(ct);
            _LoadedPlacements.Add(placement.PlacementKey);
        }

        /// <inheritdoc/>
        public bool IsLoaded(AdPlacement placement) =>
            _LoadedPlacements.Contains(placement.PlacementKey);

        /// <inheritdoc/>
        public async UniTask<AdResult> ShowAsync(AdPlacement placement, CancellationToken ct = default)
        {
            Debug.Log(
                $"[MockAdProvider] ShowAsync called. Placement={placement?.PlacementKey}, Type={placement?.GetType().Name}, PopupCallbackSet={PopupCallback != null}");

            if (PopupCallback != null)
            {
                Debug.Log($"[MockAdProvider] Forwarding placement {placement?.PlacementKey} to popup callback.");
                var tcs = PopupCallback.Invoke(placement);
                return await tcs.Task;
            }

            await SimulateDelayAsync(ct);

            _LoadedPlacements.Remove(placement.PlacementKey);
            return ResolveNextOutcome(placement.PlacementKey).ToAdResult();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _LoadedPlacements.Clear();
            _SequenceIndices.Clear();
        }

        private UniTask SimulateDelayAsync(CancellationToken ct) =>
            UniTask.Delay(
                TimeSpan.FromSeconds(_Settings.SimulatedDelaySeconds),
                cancellationToken: ct);

        /// <summary>
        /// Returns the next outcome for the given placement key.
        /// A forced outcome (set via <see cref="ForceNextOutcome"/>) takes priority and is consumed after one use.
        /// Otherwise cycles through <see cref="MockAdSettings.SequencedOutcomes"/> per key if configured;
        /// falls back to <see cref="MockAdSettings.DefaultOutcome"/>.
        /// </summary>
        private AdMockOutcome ResolveNextOutcome(AdPlacementKey key)
        {
            if (_ForcedNextOutcome.HasValue)
            {
                var forced = _ForcedNextOutcome.Value;
                _ForcedNextOutcome = null;
                return forced;
            }

            var sequenced = _Settings.SequencedOutcomes;
            if (sequenced == null || sequenced.Count == 0)
                return _Settings.DefaultOutcome;

            if (!_SequenceIndices.TryGetValue(key, out var index))
                index = 0;

            var outcome = sequenced[index % sequenced.Count];
            _SequenceIndices[key] = (index + 1) % sequenced.Count;
            return outcome;
        }
    }
}
#endif
