#if TEST_ENABLED
using System.Collections.Generic;
using UnityEngine;

namespace Common.Ads.Data
{
    /// <summary>
    /// Configuration ScriptableObject for MockAdProvider.
    /// Supports a default outcome, a simulated display delay, and an ordered sequence of outcomes
    /// for QA scenarios that need deterministic ad behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "MockAdSettings", menuName = "Knife Toss/Ads/Mock Ad Settings")]
    public class MockAdSettings : ScriptableObject
    {
        /// <summary>
        /// Outcome returned by every show call when no sequenced outcomes are configured.
        /// </summary>
        [field: SerializeField] public AdMockOutcome DefaultOutcome { get; private set; }

        /// <summary>
        /// Simulated delay in seconds applied to both LoadAsync and ShowAsync.
        /// Mimics real SDK network and display latency.
        /// </summary>
        [field: SerializeField] public float SimulatedDelaySeconds { get; private set; }

        /// <summary>
        /// Optional ordered list of outcomes cycled through per placement key.
        /// When non-empty, overrides DefaultOutcome. Cycles back to index 0 after exhaustion.
        /// </summary>
        [field: SerializeField] public List<AdMockOutcome> SequencedOutcomes { get; private set; }
    }
}
#endif
