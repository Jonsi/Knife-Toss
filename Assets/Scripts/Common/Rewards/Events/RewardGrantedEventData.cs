using System;
using System.Collections.Generic;

namespace Common.Rewards.Events
{
    /// <summary>
    /// Payload describing rewards that were granted for analytics and UI followers.
    /// </summary>
    [Serializable]
    public struct RewardGrantedEventData
    {
        /// <summary>
        /// Display lines for each granted reward (same order as granted).
        /// </summary>
        public IReadOnlyList<string> DisplayTexts { get; }

        /// <summary>
        /// Creates event data with the given display snapshot.
        /// </summary>
        public RewardGrantedEventData(IReadOnlyList<string> displayTexts)
        {
            DisplayTexts = displayTexts ?? Array.Empty<string>();
        }
    }
}
