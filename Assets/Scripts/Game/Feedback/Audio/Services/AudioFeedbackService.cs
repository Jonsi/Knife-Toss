using System;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Knife.Data;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Flow.Level;
using VContainer.Unity;

namespace Game.Feedback.Audio.Services
{
    /// <summary>
    /// Unified service that plays audio feedback for game events.
    /// </summary>
    public class AudioFeedbackService : IStartable, IDisposable
    {
        private readonly IAudioPlayer _AudioPlayer;
        private readonly SoundSettings _SoundSettings;
        private readonly IEventChannel _KnifeThrownChannel;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly IEventChannel<LevelData> _LevelCompletedChannel;
        private readonly IEventChannel<LevelData> _LevelFailedChannel;
        private bool _IsStarted;

        public AudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            IEventChannel knifeThrownChannel,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            IEventChannel<LevelData> levelCompletedChannel,
            IEventChannel<LevelData> levelFailedChannel)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifeImpactChannel = knifeImpactChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
        }

        public void Start()
        {
            if (_IsStarted)
                return;

            _KnifeThrownChannel?.Subscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Subscribe(OnKnifeImpact);
            _CollectibleHitChannel?.Subscribe(OnCollectibleHit);
            _LevelCompletedChannel?.Subscribe(OnLevelCompleted);
            _LevelFailedChannel?.Subscribe(OnLevelFailed);
            _IsStarted = true;
        }

        public void Dispose()
        {
            if (!_IsStarted)
                return;

            _KnifeThrownChannel?.Unsubscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Unsubscribe(OnKnifeImpact);
            _CollectibleHitChannel?.Unsubscribe(OnCollectibleHit);
            _LevelCompletedChannel?.Unsubscribe(OnLevelCompleted);
            _LevelFailedChannel?.Unsubscribe(OnLevelFailed);
            _IsStarted = false;
        }

        private void OnKnifeThrown()
        {
            Play(_SoundSettings?.KnifeThrowClip);
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            var clip = data.ImpactType == KnifeImpactType.Stuck
                ? _SoundSettings?.HitTargetClip
                : _SoundSettings?.HitKnifeClip;
            Play(clip);
        }

        private void OnCollectibleHit(CollectibleHitEventData _)
        {
            Play(_SoundSettings?.HitCollectibleClip);
        }

        private void OnLevelCompleted(LevelData _)
        {
            Play(_SoundSettings?.WinClip);
        }

        private void OnLevelFailed(LevelData _)
        {
            Play(_SoundSettings?.LoseClip);
        }

        private void Play(UnityEngine.AudioClip clip)
        {
            if (clip != null)
            {
                _AudioPlayer?.PlayOneShot(clip);
            }
        }
    }
}
