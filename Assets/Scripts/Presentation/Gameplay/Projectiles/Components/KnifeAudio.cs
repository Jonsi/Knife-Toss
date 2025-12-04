using UnityEngine;
using KnifeToss.Core.Abstractions.Services;
using KnifeToss.Core.Data;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Handles audio playback for knife events (hit target, hit knife, etc.).
    /// </summary>
    public sealed class KnifeAudio : MonoBehaviour
    {
        [SerializeField] private float soundVolume = 1f;

        private IAudioService _audioService;
        private SoundSettings _soundSettings;

        public void Initialize(IAudioService audioService, SoundSettings soundSettings)
        {
            _audioService = audioService;
            _soundSettings = soundSettings;
        }

        public void PlayHitTargetSound()
        {
            if (_soundSettings?.HitTargetSound != null)
            {
                _audioService?.Play2D(_soundSettings.HitTargetSound, soundVolume);
            }
        }

        public void PlayHitKnifeSound()
        {
            if (_soundSettings?.HitKnifeSound != null)
            {
                _audioService?.Play2D(_soundSettings.HitKnifeSound, soundVolume);
            }
        }
    }
}

