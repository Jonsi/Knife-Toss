using System;
using System.Collections.Generic;
using Game.Feedback.Data;

namespace Game.Feedback.Presentation
{
    /// <summary>
    /// Result of submitting a feedback request to a presentation queue.
    /// </summary>
    public enum FeedbackEnqueueResult
    {
        Presented,
        Interrupted,
        Queued,
        SuppressedDuplicate,
        SuppressedCooldown,
        DroppedQueueFull
    }

    /// <summary>
    /// Presentation-only queue that applies the shared feedback arbitration contract.
    /// It has no Unity time, UI, audio, VFX, haptic, gameplay, or economy dependency.
    /// </summary>
    public sealed class FeedbackPresentationQueue
    {
        private readonly struct PresentedRecord
        {
            public double StartedAt { get; }
            public float CooldownSeconds { get; }

            public PresentedRecord(double startedAt, float cooldownSeconds)
            {
                StartedAt = startedAt;
                CooldownSeconds = cooldownSeconds;
            }
        }

        private readonly List<FeedbackEventData> _Pending = new();
        private readonly Dictionary<string, PresentedRecord> _Presented =
            new(StringComparer.Ordinal);
        private readonly List<string> _ExpiredKeys = new();

        private FeedbackEventData _Active;

        public bool HasActive { get; private set; }
        public int PendingCount => _Pending.Count;
        public FeedbackEventData Active => HasActive
            ? _Active
            : throw new InvalidOperationException("There is no active feedback request.");
        public IReadOnlyList<FeedbackEventData> Pending => _Pending;

        /// <summary>
        /// Submits a request using an unscaled-time timestamp supplied by the presenter.
        /// </summary>
        public FeedbackEnqueueResult Enqueue(FeedbackEventData request, double now)
        {
            PruneExpiredCooldowns(now);

            if (IsActiveOrPendingDuplicate(request))
                return FeedbackEnqueueResult.SuppressedDuplicate;

            if (IsCoolingDown(request, now))
                return FeedbackEnqueueResult.SuppressedCooldown;

            if (!HasActive)
            {
                Start(request, now);
                return FeedbackEnqueueResult.Presented;
            }

            if (FeedbackEventRules.ShouldInterrupt(_Active, request))
            {
                Start(request, now);
                return FeedbackEnqueueResult.Interrupted;
            }

            if (_Pending.Count >= FeedbackEventRules.PendingQueueCapacity)
            {
                int replacementIndex = _Pending.Count - 1;
                if (!FeedbackEventRules.ShouldReplaceWhenQueueIsFull(
                        request,
                        _Pending[replacementIndex]))
                {
                    return FeedbackEnqueueResult.DroppedQueueFull;
                }

                _Pending.RemoveAt(replacementIndex);
            }

            InsertByPriority(request);
            return FeedbackEnqueueResult.Queued;
        }

        /// <summary>
        /// Completes the active request and starts the next queued request, if any.
        /// </summary>
        public bool CompleteActive(double now, out FeedbackEventData next)
        {
            if (!HasActive)
            {
                next = default;
                return false;
            }

            HasActive = false;
            _Active = default;

            if (_Pending.Count == 0)
            {
                next = default;
                return false;
            }

            next = _Pending[0];
            _Pending.RemoveAt(0);
            Start(next, now);
            return true;
        }

        /// <summary>
        /// Clears active, pending, and cooldown state during presenter or scene teardown.
        /// </summary>
        public void Clear()
        {
            HasActive = false;
            _Active = default;
            _Pending.Clear();
            _Presented.Clear();
            _ExpiredKeys.Clear();
        }

        private void Start(FeedbackEventData request, double now)
        {
            _Active = request;
            HasActive = true;
            _Presented[request.DuplicateKey] = new PresentedRecord(now, request.CooldownSeconds);
        }

        private bool IsActiveOrPendingDuplicate(FeedbackEventData request)
        {
            if (HasActive && FeedbackEventRules.IsDuplicate(request, _Active))
                return true;

            for (int i = 0; i < _Pending.Count; i++)
            {
                if (FeedbackEventRules.IsDuplicate(request, _Pending[i]))
                    return true;
            }

            return false;
        }

        private bool IsCoolingDown(FeedbackEventData request, double now)
        {
            return _Presented.TryGetValue(request.DuplicateKey, out PresentedRecord record) &&
                FeedbackEventRules.IsInCooldown(
                    request,
                    request.DuplicateKey,
                    record.StartedAt,
                    now);
        }

        private void InsertByPriority(FeedbackEventData request)
        {
            int index = 0;
            while (index < _Pending.Count &&
                   !FeedbackEventRules.ShouldInsertBefore(request, _Pending[index]))
            {
                index++;
            }

            _Pending.Insert(index, request);
        }

        private void PruneExpiredCooldowns(double now)
        {
            _ExpiredKeys.Clear();
            foreach (KeyValuePair<string, PresentedRecord> pair in _Presented)
            {
                double elapsed = now - pair.Value.StartedAt;
                if (elapsed >= pair.Value.CooldownSeconds)
                {
                    _ExpiredKeys.Add(pair.Key);
                }
            }

            for (int i = 0; i < _ExpiredKeys.Count; i++)
            {
                _Presented.Remove(_ExpiredKeys[i]);
            }
        }
    }
}
