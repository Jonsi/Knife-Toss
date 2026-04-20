#if TEST_ENABLED
using System.Collections.Generic;
using UnityEngine;

namespace Common.IAP.Data
{
    /// <summary>
    /// Configuration for mock IAP behavior used in QA.
    /// </summary>
    [CreateAssetMenu(fileName = "MockIapSettings", menuName = "Knife Toss/IAP/Mock IAP Settings")]
    public class MockIapSettings : ScriptableObject
    {
        [field: SerializeField] public IapMockOutcome DefaultOutcome { get; private set; } = IapMockOutcome.Success;
        [field: SerializeField] public string DefaultLocalizedPriceLabel { get; private set; } = "$0.99";
        [field: SerializeField] public float SimulatedDelaySeconds { get; private set; } = 0.5f;
        [field: SerializeField] public List<IapMockOutcome> SequencedOutcomes { get; private set; } = new List<IapMockOutcome>();
        [field: SerializeField] public List<IapProduct> SeededOwnedProducts { get; private set; } = new List<IapProduct>();
    }
}
#endif
