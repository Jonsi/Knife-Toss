using System;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Events;
using VContainer.Unity;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Plays hitable hit sounds in response to the hit event channel.
    /// </summary>
    public sealed class HitableObjectSoundPlayer : IStartable, IDisposable
    {
        private readonly HitableObjectHitEventChannel hitEventChannel;
        private readonly IAudioPlayer audioPlayer;
        private readonly SoundSettings soundSettings;

        /// <summary>
        /// Performs the HitableObjectSoundPlayer operation.
        /// </summary>
        public HitableObjectSoundPlayer(
            HitableObjectHitEventChannel hitEventChannel,
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings)
        {
            this.hitEventChannel = hitEventChannel;
            this.audioPlayer = audioPlayer;
            this.soundSettings = soundSettings;
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        public void Start()
        {
            if (hitEventChannel != null)
            {
                hitEventChannel.HitableHitAt += HandleHit;
            }
        }

        /// <summary>
        /// Performs the Dispose operation.
        /// </summary>
        public void Dispose()
        {
            if (hitEventChannel != null)
            {
                hitEventChannel.HitableHitAt -= HandleHit;
            }
        }

        /// <summary>
        /// Performs the HandleHit operation.
        /// </summary>
        private void HandleHit(UnityEngine.Vector3 _)
        {
            var clip = soundSettings != null ? soundSettings.HitHitableSound : null;
            if (clip == null)
            {
                return;
            }

            IAudioPlayer player = audioPlayer ?? NullAudioPlayer.Instance;
            player.Play2D(clip);
        }

        private sealed class NullAudioPlayer : IAudioPlayer
        {
            public static readonly IAudioPlayer Instance = new NullAudioPlayer();
            public void Play(UnityEngine.AudioClip clip, UnityEngine.Vector3 position, float volume) { }
            public void Play2D(UnityEngine.AudioClip clip) { }
            public void Play2D(UnityEngine.AudioClip clip, float volume) { }
            public void SetMuted(bool isMuted) { }
        }
    }
}
