namespace Game.PowerUps.Data
{
    /// <summary>
    /// Stable presentation-facing outcomes emitted by the power-up domain.
    /// </summary>
    public enum PowerUpOutcome
    {
        Activated = 0,
        Consumed = 1,
        Cancelled = 2,
        Expired = 3,
        Cleared = 4,
        Unavailable = 5,
        Blocked = 6,
        InvalidConfiguration = 7,
        Unsupported = 8,
        ExecutionFailed = 9
    }
}
