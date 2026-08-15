using System;
using Game.Feedback.Data;
using Game.Feedback.Presentation;

namespace Game.Feedback.VFX.Services
{
    public interface IFeedbackVfxPlayer
    {
        void Play(FeedbackVfxCue cue, FeedbackEventData source);
    }

    /// <summary>
    /// Presentation-only VFX seam. Scene or persistent VFX owners can subscribe and use their own
    /// anchors and pools; the shared feedback contract never carries prefabs or world positions.
    /// </summary>
    public sealed class FeedbackVfxSignal : IFeedbackVfxPlayer
    {
        private event Action<FeedbackVfxCue, FeedbackEventData> _Requested;

        public void Subscribe(Action<FeedbackVfxCue, FeedbackEventData> listener)
        {
            if (listener == null)
            {
                return;
            }

            _Requested -= listener;
            _Requested += listener;
        }

        public void Unsubscribe(Action<FeedbackVfxCue, FeedbackEventData> listener)
        {
            if (listener != null)
            {
                _Requested -= listener;
            }
        }

        public void Play(FeedbackVfxCue cue, FeedbackEventData source)
        {
            if (!string.IsNullOrWhiteSpace(cue.CueId))
            {
                _Requested?.Invoke(cue, source);
            }
        }
    }
}
