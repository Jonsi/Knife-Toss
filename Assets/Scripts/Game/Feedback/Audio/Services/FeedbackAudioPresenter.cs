using System;
using Common.Events;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Data;
using Game.Feedback.Presentation;
using VContainer.Unity;

namespace Game.Feedback.Audio.Services
{
    /// <summary>Independent shared-feedback audio subscriber.</summary>
    public sealed class FeedbackAudioPresenter : IStartable, IDisposable
    {
        private readonly FeedbackOutputSubscription<FeedbackAudioCue> _Subscription;

        public FeedbackAudioPresenter(
            IEventChannel<FeedbackEventData> channel,
            FeedbackOutputCatalog catalog,
            IAudioPlayer audioPlayer,
            IFeedbackPresentationClock clock)
        {
            _Subscription = new FeedbackOutputSubscription<FeedbackAudioCue>(
                channel,
                (FeedbackEventData feedback, out FeedbackAudioCue cue) =>
                    TryResolve(catalog, feedback, out cue),
                (cue, _) => audioPlayer?.PlayOneShot(cue.Clip, cue.Volume),
                clock);
        }

        public void Start() => _Subscription.Start();
        public void Dispose() => _Subscription.Dispose();

        private static bool TryResolve(
            FeedbackOutputCatalog catalog,
            FeedbackEventData feedback,
            out FeedbackAudioCue cue)
        {
            if (catalog != null)
            {
                return catalog.TryGetAudio(feedback, out cue);
            }

            cue = default;
            return false;
        }
    }
}
