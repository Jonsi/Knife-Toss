using System;
using Common.Events;
using Game.Entities.Knife.Data;
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
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;
        private readonly IEventChannel<LevelData> _LevelCompletedChannel;
        private readonly IEventChannel<LevelData> _LevelFailedChannel;
        private bool _IsStarted;
        private bool _HasPendingCollisionFailure;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelAudioFeedbackService"/> class.
        /// </summary>
        public LevelAudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel,
            IEventChannel<LevelData> levelCompletedChannel,
            IEventChannel<LevelData> levelFailedChannel)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _KnifeImpactChannel = knifeImpactChannel;
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

            _KnifeImpactChannel?.Subscribe(OnKnifeImpact);
            _LevelCompletedChannel?.Subscribe(OnLevelCompleted);
            _LevelFailedChannel?.Subscribe(OnLevelFailed);
            _HasPendingCollisionFailure = false;
            _IsStarted = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_IsStarted)
            {
                return;
            }

            _KnifeImpactChannel?.Unsubscribe(OnKnifeImpact);
            _LevelCompletedChannel?.Unsubscribe(OnLevelCompleted);
            _LevelFailedChannel?.Unsubscribe(OnLevelFailed);
            _IsStarted = false;
            _HasPendingCollisionFailure = false;
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (data.ImpactType == KnifeImpactType.Bounced && data.CountsAsFailure)
            {
                _HasPendingCollisionFailure = true;
            }
        }

        private void OnLevelCompleted(LevelData _)
        {
            _HasPendingCollisionFailure = false;
            PlayClip(_SoundSettings?.WinClip, _SoundSettings?.WinVolume ?? 1f);
        }

        private void OnLevelFailed(LevelData _)
        {
            if (_HasPendingCollisionFailure)
            {
                // The typed knife-impact path already played the complete collision/failure cue.
                // Suppress the generic failure accent so one collision produces one audio beat.
                _HasPendingCollisionFailure = false;
                return;
            }

            PlayClip(_SoundSettings?.LoseClip, _SoundSettings?.LoseVolume ?? 1f);
        }

        private void PlayClip(UnityEngine.AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            _AudioPlayer?.PlayOneShot(clip, UnityEngine.Mathf.Clamp01(volume));
        }
    }
}
