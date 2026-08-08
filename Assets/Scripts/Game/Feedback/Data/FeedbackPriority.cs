namespace Game.Feedback.Data
{
    /// <summary>
    /// Relative presentation importance. Higher values may interrupt lower values.
    /// </summary>
    public enum FeedbackPriority
    {
        Low = 100,
        Normal = 200,
        Elevated = 300,
        High = 400,
        Critical = 500
    }
}
