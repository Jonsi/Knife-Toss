using System;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using VContainer.Unity;

namespace Game.Feedback.Audio.Services
{
    /// <summary>
    /// Plays knife-related audio cues (throw, impact outcomes).
    /// </summary>
    public class KnifeAudioFeedbackService : IStartable, IDisposable
    {
        private readonly IAudioPlayer _AudioPlayer;
        private readonly SoundSettings _SoundSettings;
        private readonly IEventChannel _KnifeThrownChannel;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;
        private bool _IsStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnifeAudioFeedbackService"/> class.
        /// </summary>
        public KnifeAudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            IEventChannel knifeThrownChannel,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifeImpactChannel = knifeImpactChannel;
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
        }

        private void OnKnifeThrown()
        {
            PlayClip(_SoundSettings?.KnifeThrowClip);
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (data.ImpactType == KnifeImpactType.Stuck)
            {
                PlayClip(_SoundSettings?.HitTargetClip);
                return;
            }

            if (data.ImpactType == KnifeImpactType.Bounced)
            {
                PlayClip(_SoundSettings?.HitKnifeClip);
            }
        }

        private void PlayClip(UnityEngine.AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            _AudioPlayer?.PlayOneShot(clip);
        }
    }
}

