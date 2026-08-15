using Game.Feedback.Presentation;

namespace Game.Feedback.Haptics
{
    public interface IHapticFeedbackPlayer
    {
        void Play(FeedbackHapticStyle style);
    }
}
