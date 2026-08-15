using System;
using CandyHaptics = CandyCoded.HapticFeedback.HapticFeedback;
using Game.Feedback.Presentation;

namespace Game.Feedback.Haptics
{
    /// <summary>Best-effort adapter over the installed Android/iOS haptic package.</summary>
    public sealed class CandyCodedHapticFeedbackPlayer : IHapticFeedbackPlayer
    {
        public void Play(FeedbackHapticStyle style)
        {
            try
            {
                switch (style)
                {
                    case FeedbackHapticStyle.Light:
                        CandyHaptics.LightFeedback();
                        break;
                    case FeedbackHapticStyle.Medium:
                        CandyHaptics.MediumFeedback();
                        break;
                    case FeedbackHapticStyle.Heavy:
                        CandyHaptics.HeavyFeedback();
                        break;
                }
            }
            catch (Exception)
            {
                // Optional presentation must never interrupt the authoritative action.
            }
        }
    }
}
