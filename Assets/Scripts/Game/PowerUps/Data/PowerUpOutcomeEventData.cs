using Game.PowerUps.Configs;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Typed power-up lifecycle/result payload consumed by shared feedback presenters.
    /// A remaining count of -1 means that inventory was not sampled for this lifecycle event.
    /// </summary>
    public readonly struct PowerUpOutcomeEventData
    {
        public PowerUpConfigBase Config { get; }
        public PowerUpOutcome Outcome { get; }
        public int RemainingCount { get; }
        public string FailureReason { get; }

        public PowerUpOutcomeEventData(
            PowerUpConfigBase config,
            PowerUpOutcome outcome,
            int remainingCount,
            string failureReason = null)
        {
            Config = config;
            Outcome = outcome;
            RemainingCount = remainingCount;
            FailureReason = failureReason;
        }
    }
}
