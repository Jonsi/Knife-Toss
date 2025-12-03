using KnifeToss.Core.Abstractions;
using UnityEngine;

namespace KnifeToss.Infrastructure.Audio
{
    public sealed class AudioPlayer : IAudioPlayer
    {
        private AudioSource sharedSource;

        /// <summary>
        /// Performs the AudioPlayer operation.
        /// </summary>
        public AudioPlayer()
        {
            ConfigureLowLatencyIfNeeded();
            sharedSource = CreateOrGetSource();
        }

        /// <summary>
        /// Performs the Play operation.
        /// </summary>
        public void Play(AudioClip clip, Vector3 position, float volume)
        {
            PlayInternal(clip, position, volume);
        }

        /// <summary>
        /// Performs the Play2D operation.
        /// </summary>
        public void Play2D(AudioClip clip)
        {
            PlayInternal(clip, Vector3.zero, 1f);
        }

        /// <summary>
        /// Performs the Play2D operation.
        /// </summary>
        public void Play2D(AudioClip clip, float volume)
        {
            PlayInternal(clip, Vector3.zero, volume);
        }

        /// <summary>
        /// Performs the SetMuted operation.
        /// </summary>
        public void SetMuted(bool isMuted)
        {
            if (sharedSource != null)
            {
                sharedSource.mute = isMuted;
            }
        }

        /// <summary>
        /// Performs the PlayInternal operation.
        /// </summary>
        private void PlayInternal(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource source = sharedSource != null ? sharedSource : (sharedSource = CreateOrGetSource());
            if (source == null)
            {
                return;
            }

            source.transform.position = position;


            source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Performs the CreateOrGetSource operation.
        /// </summary>
        private static AudioSource CreateOrGetSource()
        {
            const string goName = "AudioPlayer (2D)";

            GameObject existing = GameObject.Find(goName);
            GameObject owner = existing != null ? existing : new GameObject(goName);

            AudioSource source = owner.GetComponent<AudioSource>();
            if (source == null)
            {
                source = owner.AddComponent<AudioSource>();
            }

            if (source == null)
            {

                owner = new GameObject(goName);
                source = owner.AddComponent<AudioSource>();
            }

            if (source == null)
            {
                Debug.LogError("AudioPlayer could not create an AudioSource; SFX will be silent.");
                return null;
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.priority = 0;
            source.spatialize = false;

            Object.DontDestroyOnLoad(owner);

            return source;
        }

        /// <summary>
        /// Performs the ConfigureLowLatencyIfNeeded operation.
        /// </summary>
        private static void ConfigureLowLatencyIfNeeded()
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();


            const int targetBuffer = 256;
            if (config.dspBufferSize != targetBuffer && config.dspBufferSize > 0)
            {
                config.dspBufferSize = targetBuffer;
                AudioSettings.Reset(config);
            }
        }
    }
}
