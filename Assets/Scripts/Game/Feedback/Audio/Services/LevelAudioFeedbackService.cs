using System;
using Common.Events;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Flow.Level;
using VContainer.Unity;

namespace Game.Feedback.Audio.Services
{
    /// <summary>
    /// Plays audio cues for level completion and failure.
    /// </summary>
    public class LevelAudioFeedbackService : IStartable, IDisposable
    {
        private readonly IAudioPlayer _AudioPlayer;
        private readonly SoundSettings _SoundSettings;
        private readonly IEventChannel<LevelData> _LevelCompletedChannel;
        private readonly IEventChannel<LevelData> _LevelFailedChannel;
        private bool _IsStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelAudioFeedbackService"/> class.
        /// </summary>
        public LevelAudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            IEventChannel<LevelData> levelCompletedChannel,
            IEventChannel<LevelData> levelFailedChannel)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_IsStarted)
            {
                return;
            }

            _LevelCompletedChannel?.Subscribe(OnLevelCompleted);
            _LevelFailedChannel?.Subscribe(OnLevelFailed);
            _IsStarted = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_IsStarted)
            {
                return;
            }

            _LevelCompletedChannel?.Unsubscribe(OnLevelCompleted);
            _LevelFailedChannel?.Unsubscribe(OnLevelFailed);
            _IsStarted = false;
        }

        private void OnLevelCompleted(LevelData _)
        {
            PlayClip(_SoundSettings?.WinClip);
        }

        private void OnLevelFailed(LevelData _)
        {
            PlayClip(_SoundSettings?.LoseClip);
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

