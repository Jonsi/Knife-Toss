using System;
using System.Collections.Generic;
using Game.Feedback.Data;
using UnityEngine;

namespace Game.Feedback.Presentation
{
    /// <summary>
    /// Presentation-side mapping from a shared semantic cue to optional audio, VFX, and haptic outputs.
    /// Exact ordinal matches are intentional: unknown cues never inherit a broad fallback effect.
    /// </summary>
    [CreateAssetMenu(fileName = "FeedbackOutputCatalog", menuName = "Knife Toss/Feedback/Output Catalog")]
    public sealed class FeedbackOutputCatalog : ScriptableObject
    {
        [SerializeField] private List<FeedbackOutputMapping> _Mappings = new();

        public IReadOnlyList<FeedbackOutputMapping> Mappings => _Mappings;

        public bool TryGetAudio(FeedbackEventData feedback, out FeedbackAudioCue cue)
        {
            FeedbackOutputMapping mapping = Find(feedback);
            if (mapping != null && mapping.AudioClip != null)
            {
                cue = new FeedbackAudioCue(mapping.AudioClip, mapping.AudioVolume);
                return true;
            }

            cue = default;
            return false;
        }

        public bool TryGetVfx(FeedbackEventData feedback, out FeedbackVfxCue cue)
        {
            FeedbackOutputMapping mapping = Find(feedback);
            if (mapping != null && !string.IsNullOrWhiteSpace(mapping.VfxCueId))
            {
                cue = new FeedbackVfxCue(mapping.VfxCueId);
                return true;
            }

            cue = default;
            return false;
        }

        public bool TryGetHaptic(FeedbackEventData feedback, out FeedbackHapticStyle style)
        {
            FeedbackOutputMapping mapping = Find(feedback);
            if (mapping != null && mapping.HapticStyle != FeedbackHapticStyle.None)
            {
                style = mapping.HapticStyle;
                return true;
            }

            style = FeedbackHapticStyle.None;
            return false;
        }

#if UNITY_EDITOR
        /// <summary>Editor-test seam; production mappings remain serialized in the catalog asset.</summary>
        public void SetMappingsForTests(params FeedbackOutputMapping[] mappings)
        {
            _Mappings = mappings == null ? new List<FeedbackOutputMapping>() : new List<FeedbackOutputMapping>(mappings);
        }
#endif

        private FeedbackOutputMapping Find(FeedbackEventData feedback)
        {
            if (_Mappings == null)
            {
                return null;
            }

            for (int i = 0; i < _Mappings.Count; i++)
            {
                FeedbackOutputMapping mapping = _Mappings[i];
                if (mapping != null && mapping.Matches(feedback))
                {
                    return mapping;
                }
            }

            return null;
        }
    }

    [Serializable]
    public sealed class FeedbackOutputMapping
    {
        [SerializeField] private FeedbackKind _Kind;
        [SerializeField] private string _CueId;
        [SerializeField] private AudioClip _AudioClip;
        [SerializeField, Range(0f, 1f)] private float _AudioVolume = 1f;
        [SerializeField] private string _VfxCueId;
        [SerializeField] private FeedbackHapticStyle _HapticStyle;

        public FeedbackKind Kind => _Kind;
        public string CueId => _CueId;
        public AudioClip AudioClip => _AudioClip;
        public float AudioVolume => Mathf.Clamp01(_AudioVolume);
        public string VfxCueId => _VfxCueId;
        public FeedbackHapticStyle HapticStyle => _HapticStyle;

        public FeedbackOutputMapping(
            FeedbackKind kind,
            string cueId,
            AudioClip audioClip = null,
            float audioVolume = 1f,
            string vfxCueId = null,
            FeedbackHapticStyle hapticStyle = FeedbackHapticStyle.None)
        {
            _Kind = kind;
            _CueId = cueId;
            _AudioClip = audioClip;
            _AudioVolume = audioVolume;
            _VfxCueId = vfxCueId;
            _HapticStyle = hapticStyle;
        }

        internal bool Matches(FeedbackEventData feedback)
        {
            return _Kind == feedback.Kind && string.Equals(_CueId, feedback.CueId, StringComparison.Ordinal);
        }
    }

    public readonly struct FeedbackAudioCue
    {
        public AudioClip Clip { get; }
        public float Volume { get; }

        public FeedbackAudioCue(AudioClip clip, float volume)
        {
            Clip = clip;
            Volume = Mathf.Clamp01(volume);
        }
    }

    public readonly struct FeedbackVfxCue
    {
        public string CueId { get; }

        public FeedbackVfxCue(string cueId)
        {
            CueId = cueId;
        }
    }

    public enum FeedbackHapticStyle
    {
        None = 0,
        Light = 1,
        Medium = 2,
        Heavy = 3
    }
}
