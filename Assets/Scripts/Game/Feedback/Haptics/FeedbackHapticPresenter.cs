using System;
using Common.Events;
using Game.Feedback.Data;
using Game.Feedback.Presentation;
using VContainer.Unity;

namespace Game.Feedback.Haptics
{
    /// <summary>Independent shared-feedback haptic subscriber.</summary>
    public sealed class FeedbackHapticPresenter : IStartable, IDisposable
    {
        private readonly FeedbackOutputSubscription<FeedbackHapticStyle> _Subscription;

        public FeedbackHapticPresenter(
            IEventChannel<FeedbackEventData> channel,
            FeedbackOutputCatalog catalog,
            IHapticFeedbackPlayer player,
            IFeedbackPresentationClock clock)
        {
            _Subscription = new FeedbackOutputSubscription<FeedbackHapticStyle>(
                channel,
                (FeedbackEventData feedback, out FeedbackHapticStyle style) =>
                    TryResolve(catalog, feedback, out style),
                (style, _) => player?.Play(style),
                clock);
        }

        public void Start() => _Subscription.Start();
        public void Dispose() => _Subscription.Dispose();

        private static bool TryResolve(
            FeedbackOutputCatalog catalog,
            FeedbackEventData feedback,
            out FeedbackHapticStyle style)
        {
            if (catalog != null)
            {
                return catalog.TryGetHaptic(feedback, out style);
            }

            style = FeedbackHapticStyle.None;
            return false;
        }
    }
}
