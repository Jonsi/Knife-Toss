namespace Game.Feedback.Data
{
    /// <summary>
    /// Presentation-neutral meanings that shared feedback presenters can react to.
    /// Domain-specific event payloads remain the source of truth for gameplay data.
    /// </summary>
    public enum FeedbackKind
    {
        Success = 0,
        Blocked = 1,
        InsufficientFunds = 2,
        Purchase = 3,
        Reward = 4,
        Consume = 5,
        Navigation = 6,
        Failure = 7,
        Completion = 8
    }
}
