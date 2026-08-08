using System;

namespace Game.Feedback.Data
{
    /// <summary>
    /// Typed, presentation-neutral request for shared feedback.
    /// CueId selects a semantic cue, not localized copy or a presentation asset.
    /// ContextId scopes duplicate suppression to an item, transaction, level, or other subject.
    /// </summary>
    public readonly struct FeedbackEventData
    {
        public FeedbackKind Kind { get; }
        public string CueId { get; }
        public string ContextId { get; }
        public FeedbackPriority Priority => FeedbackEventRules.GetPriority(Kind);
        public float CooldownSeconds => FeedbackEventRules.GetCooldownSeconds(Kind);

        /// <summary>
        /// Stable ordinal key used only for duplicate suppression and cooldown tracking.
        /// </summary>
        public string DuplicateKey => string.IsNullOrEmpty(ContextId)
            ? $"{Kind}:{CueId}"
            : $"{Kind}:{CueId}:{ContextId}";

        public FeedbackEventData(FeedbackKind kind, string cueId, string contextId = null)
        {
            if (string.IsNullOrWhiteSpace(cueId))
                throw new ArgumentException("A semantic feedback cue id is required.", nameof(cueId));

            FeedbackEventRules.Validate(kind);
            Kind = kind;
            CueId = cueId;
            ContextId = string.IsNullOrWhiteSpace(contextId) ? null : contextId;
        }
    }
}
