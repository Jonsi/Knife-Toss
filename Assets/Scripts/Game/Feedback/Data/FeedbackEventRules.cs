using System;

namespace Game.Feedback.Data
{
    /// <summary>
    /// Shared arbitration contract for UI, audio, VFX, and haptic presenters.
    /// It contains no presentation implementation or Unity time dependency.
    /// </summary>
    public static class FeedbackEventRules
    {
        /// <summary>
        /// Maximum number of pending requests recommended for a presenter queue.
        /// </summary>
        public const int PendingQueueCapacity = 8;

        public static FeedbackPriority GetPriority(FeedbackKind kind)
        {
            return kind switch
            {
                FeedbackKind.Navigation => FeedbackPriority.Low,
                FeedbackKind.Success => FeedbackPriority.Normal,
                FeedbackKind.Consume => FeedbackPriority.Normal,
                FeedbackKind.Purchase => FeedbackPriority.Elevated,
                FeedbackKind.Reward => FeedbackPriority.Elevated,
                FeedbackKind.Blocked => FeedbackPriority.High,
                FeedbackKind.InsufficientFunds => FeedbackPriority.High,
                FeedbackKind.Failure => FeedbackPriority.Critical,
                FeedbackKind.Completion => FeedbackPriority.Critical,
                _ => throw UnknownKind(kind)
            };
        }

        /// <summary>
        /// Duplicate cooldown measured from presentation start using unscaled time.
        /// Cooldown applies only to an identical duplicate key.
        /// </summary>
        public static float GetCooldownSeconds(FeedbackKind kind)
        {
            return kind switch
            {
                FeedbackKind.Navigation => 0.15f,
                FeedbackKind.Success => 0.35f,
                FeedbackKind.Consume => 0.35f,
                FeedbackKind.Purchase => 1f,
                FeedbackKind.Reward => 1f,
                FeedbackKind.Blocked => 0.75f,
                FeedbackKind.InsufficientFunds => 0.75f,
                FeedbackKind.Failure => 1.5f,
                FeedbackKind.Completion => 1.5f,
                _ => throw UnknownKind(kind)
            };
        }

        /// <summary>
        /// A request interrupts only a currently presenting request with strictly lower priority.
        /// Equal-priority requests retain FIFO order and never interrupt each other.
        /// </summary>
        public static bool ShouldInterrupt(
            FeedbackEventData active,
            FeedbackEventData incoming)
        {
            return incoming.Priority > active.Priority;
        }

        /// <summary>
        /// Returns true when incoming belongs ahead of an already queued request.
        /// Returning false for equal priority preserves FIFO insertion order.
        /// </summary>
        public static bool ShouldInsertBefore(
            FeedbackEventData incoming,
            FeedbackEventData queued)
        {
            return incoming.Priority > queued.Priority;
        }

        /// <summary>
        /// When a pending queue is full, only a strictly higher-priority request may replace
        /// the newest request in the queue's lowest-priority tier.
        /// </summary>
        public static bool ShouldReplaceWhenQueueIsFull(
            FeedbackEventData incoming,
            FeedbackEventData lowestQueued)
        {
            return incoming.Priority > lowestQueued.Priority;
        }

        /// <summary>
        /// Active and pending requests with the same ordinal key are duplicates regardless of time.
        /// </summary>
        public static bool IsDuplicate(
            FeedbackEventData candidate,
            FeedbackEventData activeOrPending)
        {
            return string.Equals(
                candidate.DuplicateKey,
                activeOrPending.DuplicateKey,
                StringComparison.Ordinal);
        }

        /// <summary>
        /// Tests the post-presentation cooldown for an identical duplicate key. A clock rollback is
        /// treated as still cooling down; the boundary itself is accepted.
        /// </summary>
        public static bool IsInCooldown(
            FeedbackEventData candidate,
            string presentedDuplicateKey,
            double presentedAt,
            double now)
        {
            if (!string.Equals(
                    candidate.DuplicateKey,
                    presentedDuplicateKey,
                    StringComparison.Ordinal))
            {
                return false;
            }

            double elapsed = now - presentedAt;
            return elapsed < 0d || elapsed < candidate.CooldownSeconds;
        }

        public static void Validate(FeedbackKind kind)
        {
            _ = GetPriority(kind);
        }

        private static ArgumentOutOfRangeException UnknownKind(FeedbackKind kind)
        {
            return new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown feedback kind.");
        }
    }
}
