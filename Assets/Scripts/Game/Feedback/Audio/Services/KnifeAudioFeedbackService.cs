using System;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Presentation;
using UnityEngine;
using VContainer.Unity;

namespace Game.Feedback.Audio.Services
{
    /// <summary>
    /// Plays knife-related audio cues (throw, impact outcomes).
    /// </summary>
    public class KnifeAudioFeedbackService : IStartable, IDisposable
    {
        private const double CooldownBoundaryTolerance = 0.000001d;
        private readonly IAudioPlayer _AudioPlayer;
        private readonly SoundSettings _SoundSettings;
        private readonly IEventChannel _KnifeThrownChannel;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;
        private readonly IFeedbackPresentationClock _Clock;
        private bool _IsStarted;
        private double _LastThrowAt = double.NegativeInfinity;
        private double _LastTargetImpactAt = double.NegativeInfinity;
        private double _LastKnifeImpactAt = double.NegativeInfinity;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnifeAudioFeedbackService"/> class.
        /// </summary>
        public KnifeAudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            IEventChannel knifeThrownChannel,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel,
            IFeedbackPresentationClock clock)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifeImpactChannel = knifeImpactChannel;
            _Clock = clock;
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_IsStarted)
            {
                return;
            }

            _KnifeThrownChannel?.Subscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Subscribe(OnKnifeImpact);
            _IsStarted = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_IsStarted)
            {
                return;
            }

            _KnifeThrownChannel?.Unsubscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Unsubscribe(OnKnifeImpact);
            _IsStarted = false;
            ResetCooldowns();
        }

        private void OnKnifeThrown()
        {
            PlayClip(
                _SoundSettings?.KnifeThrowClip,
                _SoundSettings?.KnifeThrowVolume ?? 1f,
                _SoundSettings?.KnifeThrowCooldownSeconds ?? 0f,
                ref _LastThrowAt);
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (data.ImpactType == KnifeImpactType.Stuck)
            {
                PlayClip(
                    _SoundSettings?.HitTargetClip,
                    _SoundSettings?.HitTargetVolume ?? 1f,
                    _SoundSettings?.HitTargetCooldownSeconds ?? 0f,
                    ref _LastTargetImpactAt);
                return;
            }

            if (data.ImpactType == KnifeImpactType.Bounced)
            {
                PlayClip(
                    _SoundSettings?.HitKnifeClip,
                    _SoundSettings?.HitKnifeVolume ?? 1f,
                    _SoundSettings?.HitKnifeCooldownSeconds ?? 0f,
                    ref _LastKnifeImpactAt);
            }
        }

        private void PlayClip(AudioClip clip, float volume, float cooldownSeconds, ref double lastPlayedAt)
        {
            if (clip == null || _AudioPlayer == null)
            {
                return;
            }

            double now = _Clock?.Now ?? Time.unscaledTimeAsDouble;
            if (now - lastPlayedAt + CooldownBoundaryTolerance < Mathf.Max(0f, cooldownSeconds))
            {
                return;
            }

            _AudioPlayer.PlayOneShot(clip, Mathf.Clamp01(volume));
            lastPlayedAt = now;
        }

        private void ResetCooldowns()
        {
            _LastThrowAt = double.NegativeInfinity;
            _LastTargetImpactAt = double.NegativeInfinity;
            _LastKnifeImpactAt = double.NegativeInfinity;
        }
    }
}
