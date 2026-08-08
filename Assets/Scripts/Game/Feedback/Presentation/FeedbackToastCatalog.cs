using System;
using Game.Feedback.Data;
using UnityEngine;

namespace Game.Feedback.Presentation
{
    [Serializable]
    public sealed class FeedbackToastCue
    {
        [SerializeField] private FeedbackKind _Kind;
        [SerializeField] private string _CueId;
        [SerializeField] private string _Message;
        [SerializeField] private Color _AccentColor = Color.white;
        [SerializeField, Min(0.1f)] private float _VisibleSeconds = 1.5f;

        public FeedbackKind Kind => _Kind;
        public string CueId => _CueId;
        public string Message => _Message;
        public Color AccentColor => _AccentColor;
        public float VisibleSeconds => Mathf.Max(0.1f, _VisibleSeconds);
    }

    public readonly struct FeedbackToastPresentation
    {
        public string Message { get; }
        public Color AccentColor { get; }
        public float VisibleSeconds { get; }

        public FeedbackToastPresentation(string message, Color accentColor, float visibleSeconds)
        {
            Message = message;
            AccentColor = accentColor;
            VisibleSeconds = visibleSeconds;
        }
    }

    /// <summary>
    /// Presentation copy and styling keyed by typed semantic cue identifiers.
    /// </summary>
    [CreateAssetMenu(
        fileName = "FeedbackToastCatalog",
        menuName = "Knife Toss/Feedback/Toast Catalog")]
    public sealed class FeedbackToastCatalog : ScriptableObject
    {
        [SerializeField] private FeedbackToastCue[] _Cues = Array.Empty<FeedbackToastCue>();

        public bool TryResolve(FeedbackEventData request, out FeedbackToastPresentation presentation)
        {
            for (int i = 0; i < _Cues.Length; i++)
            {
                FeedbackToastCue cue = _Cues[i];
                if (cue == null || cue.Kind != request.Kind ||
                    !string.Equals(cue.CueId, request.CueId, StringComparison.Ordinal))
                {
                    continue;
                }

                presentation = new FeedbackToastPresentation(
                    cue.Message,
                    cue.AccentColor,
                    cue.VisibleSeconds);
                return true;
            }

            presentation = default;
            return false;
        }
    }
}
