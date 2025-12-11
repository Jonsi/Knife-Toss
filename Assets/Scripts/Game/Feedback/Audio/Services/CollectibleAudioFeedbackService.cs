using System;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using VContainer.Unity;

namespace Game.Feedback.Audio.Services
{
    /// <summary>
    /// Plays audio cues for collectible hits.
    /// </summary>
    public class CollectibleAudioFeedbackService : IStartable, IDisposable
    {
        private readonly IAudioPlayer _AudioPlayer;
        private readonly SoundSettings _SoundSettings;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private bool _IsStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectibleAudioFeedbackService"/> class.
        /// </summary>
        public CollectibleAudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _CollectibleHitChannel = collectibleHitChannel;
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_IsStarted)
            {
                return;
            }

            _CollectibleHitChannel?.Subscribe(OnCollectibleHit);
            _IsStarted = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_IsStarted)
            {
                return;
            }

            _CollectibleHitChannel?.Unsubscribe(OnCollectibleHit);
            _IsStarted = false;
        }

        private void OnCollectibleHit(CollectibleHitEventData _)
        {
            PlayClip(_SoundSettings?.HitCollectibleClip);
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

