using System;

namespace Common.Ads.Data
{
    /// <summary>
    /// Serializable struct representing a configurable mock ad outcome.
    /// Use static factories for named outcomes; set via Unity Inspector for sequenced lists.
    /// </summary>
    [Serializable]
    public struct AdMockOutcome
    {
        /// <summary>Outcome type selected from the Inspector dropdown.</summary>
        public AdMockOutcomeType Type;

        /// <summary>Optional failure message used when Type is Failed.</summary>
        public string FailureReason;

        private AdMockOutcome(AdMockOutcomeType type, string failureReason = null)
        {
            Type = type;
            FailureReason = failureReason;
        }

        /// <summary>Ad played to completion; reward should be granted.</summary>
        public static AdMockOutcome Completed => new AdMockOutcome(AdMockOutcomeType.Completed);

        /// <summary>User dismissed the ad before completion.</summary>
        public static AdMockOutcome Skipped => new AdMockOutcome(AdMockOutcomeType.Skipped);

        /// <summary>Provider had no ad loaded (e.g. network unavailable).</summary>
        public static AdMockOutcome Unavailable => new AdMockOutcome(AdMockOutcomeType.Unavailable);

        /// <summary>Provider simulates a timeout condition.</summary>
        public static AdMockOutcome Timeout => new AdMockOutcome(AdMockOutcomeType.Timeout);

        /// <summary>Provider-level failure with a descriptive reason.</summary>
        public static AdMockOutcome Failed(string reason) => new AdMockOutcome(AdMockOutcomeType.Failed, reason);

        /// <summary>Converts this outcome to the corresponding <see cref="AdResult"/>.</summary>
        public AdResult ToAdResult()
        {
            return Type switch
            {
                AdMockOutcomeType.Completed   => AdResult.Completed,
                AdMockOutcomeType.Skipped     => AdResult.Skipped,
                AdMockOutcomeType.Unavailable => AdResult.Unavailable,
                AdMockOutcomeType.Timeout     => AdResult.Timeout,
                AdMockOutcomeType.Failed      => AdResult.Failed(FailureReason),
                _                             => AdResult.Failed($"Unknown mock outcome type: '{Type}'.")
            };
        }
    }
}
