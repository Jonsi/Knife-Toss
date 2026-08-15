using System;
using Common.Events;
using Game.Feedback.Data;
using Game.Feedback.Presentation;
using VContainer.Unity;

namespace Game.Feedback.VFX.Services
{
    /// <summary>Independent shared-feedback VFX subscriber.</summary>
    public sealed class FeedbackVfxPresenter : IStartable, IDisposable
    {
        private readonly FeedbackOutputSubscription<FeedbackVfxCue> _Subscription;

        public FeedbackVfxPresenter(
            IEventChannel<FeedbackEventData> channel,
            FeedbackOutputCatalog catalog,
            IFeedbackVfxPlayer player,
            IFeedbackPresentationClock clock)
        {
            _Subscription = new FeedbackOutputSubscription<FeedbackVfxCue>(
                channel,
                (FeedbackEventData feedback, out FeedbackVfxCue cue) =>
                    TryResolve(catalog, feedback, out cue),
                (cue, feedback) => player?.Play(cue, feedback),
                clock);
        }

        public void Start() => _Subscription.Start();
        public void Dispose() => _Subscription.Dispose();

        private static bool TryResolve(
            FeedbackOutputCatalog catalog,
            FeedbackEventData feedback,
            out FeedbackVfxCue cue)
        {
            if (catalog != null)
            {
                return catalog.TryGetVfx(feedback, out cue);
            }

            cue = default;
            return false;
        }
    }
}
