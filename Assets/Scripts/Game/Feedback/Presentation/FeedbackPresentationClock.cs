using UnityEngine;

namespace Game.Feedback.Presentation
{
    public interface IFeedbackPresentationClock
    {
        double Now { get; }
    }

    /// <summary>Unscaled clock shared by instantaneous feedback output gates.</summary>
    public sealed class UnityFeedbackPresentationClock : IFeedbackPresentationClock
    {
        public double Now => Time.unscaledTimeAsDouble;
    }
}
