using System;
using System.Collections.Generic;
using Common.Events;
using Game.Feedback.Data;
using UnityEngine.SceneManagement;

namespace Game.Feedback.Presentation
{
    public delegate bool FeedbackOutputResolver<TOutput>(FeedbackEventData feedback, out TOutput output);

    /// <summary>
    /// Owns one output path's channel subscription, exact duplicate cooldown, and scene/teardown cleanup.
    /// It has no gameplay side effects and does not queue or retry presentation.
    /// </summary>
    public sealed class FeedbackOutputSubscription<TOutput> : IDisposable
    {
        private readonly IEventChannel<FeedbackEventData> _Channel;
        private readonly FeedbackOutputResolver<TOutput> _Resolver;
        private readonly Action<TOutput, FeedbackEventData> _Present;
        private readonly IFeedbackPresentationClock _Clock;
        private readonly Dictionary<string, double> _LastPresentedAt = new(StringComparer.Ordinal);
        private bool _IsStarted;

        public bool IsStarted => _IsStarted;

        public FeedbackOutputSubscription(
            IEventChannel<FeedbackEventData> channel,
            FeedbackOutputResolver<TOutput> resolver,
            Action<TOutput, FeedbackEventData> present,
            IFeedbackPresentationClock clock)
        {
            _Channel = channel;
            _Resolver = resolver;
            _Present = present;
            _Clock = clock;
        }

        public void Start()
        {
            if (_IsStarted)
            {
                return;
            }

            _Channel?.Subscribe(OnFeedback);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            _IsStarted = true;
        }

        public void Dispose()
        {
            if (!_IsStarted)
            {
                Clear();
                return;
            }

            _Channel?.Unsubscribe(OnFeedback);
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _IsStarted = false;
            Clear();
        }

        public void Clear()
        {
            _LastPresentedAt.Clear();
        }

        private void OnFeedback(FeedbackEventData feedback)
        {
            if (_Resolver == null || _Present == null || !_Resolver(feedback, out TOutput output))
            {
                return;
            }

            double now = _Clock?.Now ?? 0d;
            if (_LastPresentedAt.TryGetValue(feedback.DuplicateKey, out double presentedAt) &&
                now - presentedAt < feedback.CooldownSeconds)
            {
                return;
            }

            _Present(output, feedback);
            _LastPresentedAt[feedback.DuplicateKey] = now;
        }

        private void OnActiveSceneChanged(Scene _, Scene __)
        {
            Clear();
        }
    }
}
